
using Microsoft.AspNetCore.Mvc;
using ControlIDMvc.ServicesCI;
using ControlIDMvc.Querys;
using ControlIDMvc.ServicesCI.UtilidadesCI;

using ControlIDMvc.ServicesCI.QueryCI;
using ControlIDMvc.ServicesCI.Dtos.usersDto;
using Newtonsoft.Json;
using ControlIDMvc.Dtos;
using ControlIDMvc.ServicesCI.Dtos.cardsDto;
using ControlIDMvc.Dtos.Tarjeta;
using ControlIDMvc.Dtos.Persona;
using Microsoft.AspNetCore.Authorization;
using ControlIDMvc.Entities;
using System.Web;

namespace ControlIDMvc.Controllers;

[Authorize]
[Route("persona")]
public class PersonaController : Controller
{
    /* propiedades */
    public string controlador = "192.168.88.129";
    public string user = "admin";
    public string password = "admin";
    public int port { get; set; }
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClientService _httpClientService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private PersonaQuery _personaQuery;
    private readonly TarjetaQuery _tarjetaQuery;
    private readonly LoginControlIdQuery _loginControlIdQuery;
    private readonly UsuarioControlIdQuery _usuarioControlIdQuery;
    private readonly CardControlIdQuery _cardControlIdQuery;
    private readonly DispositivoQuery _dispositivoQuery;
    private readonly RegistroRostroControlIdQuery _registroRostroControlIdQuery;
    private readonly UsuarioRulesAccessControlIdQuery _usuarioRulesAccessControlIdQuery;
    private readonly ImagenPerfilQuery _imagenPerfilQuery;
    private readonly ReglaAccesoQuery _reglaAccesoQuery;
    private readonly PersonaReglaAccesoQuery _personaReglaAccesoQuery;
    ApiRutas _ApiRutas;
    public PersonaController(
        ILogger<HomeController> logger,
        HttpClientService httpClientService,
        IWebHostEnvironment webHostEnvironment,
        PersonaQuery personaQuery,
        TarjetaQuery tarjetaQuery,
        LoginControlIdQuery loginControlIdQuery,
        UsuarioControlIdQuery usuarioControlIdQuery,
        CardControlIdQuery cardControlIdQuery,
        DispositivoQuery dispositivoQuery,
        RegistroRostroControlIdQuery registroRostroControlIdQuery,
        UsuarioRulesAccessControlIdQuery usuarioRulesAccessControlIdQuery,
        ImagenPerfilQuery imagenPerfilQuery,
        ReglaAccesoQuery reglaAccesoQuery,
        PersonaReglaAccesoQuery personaReglaAccesoQuery
        )
    {
        this._httpClientService = httpClientService;
        this._webHostEnvironment = webHostEnvironment;
        this._personaQuery = personaQuery;
        this._tarjetaQuery = tarjetaQuery;

        this._usuarioControlIdQuery = usuarioControlIdQuery;
        this._cardControlIdQuery = cardControlIdQuery;
        this._dispositivoQuery = dispositivoQuery;
        this._registroRostroControlIdQuery = registroRostroControlIdQuery;
        this._usuarioRulesAccessControlIdQuery = usuarioRulesAccessControlIdQuery;
        this._imagenPerfilQuery = imagenPerfilQuery;
        this._reglaAccesoQuery = reglaAccesoQuery;
        this._personaReglaAccesoQuery = personaReglaAccesoQuery;
        this._logger = logger;

        this._loginControlIdQuery = loginControlIdQuery;
        this._ApiRutas = new ApiRutas();
    }

    [HttpGet]
    public async Task<ActionResult> Index()
    {
        this._logger.LogWarning("Index() Iniciando....");
        var personas = await this._personaQuery.GetAll();
        return View("~/Views/Persona/Lista.cshtml");
    }

    [HttpGet("create")]
    public async Task<ActionResult> Create()
    {
        
        ViewData["ReglasAcceso"] = await this._reglaAccesoQuery.GetAll();
        return View("~/Views/Persona/Create.cshtml");
    }
    private async Task<Boolean> loginControlId()
    {
        BodyLogin cuerpo = _loginControlIdQuery.Login(this.user, this.password);
        Response login = await this._httpClientService.LoginRun(this.controlador, this.port, this._ApiRutas.ApiUrlLogin, cuerpo, "");
        return login.estado;
    }

    [HttpPost("store")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Post(PersonaCreateDto personaCreateDto)
    {
        if (ModelState.IsValid)
        {
            if (await this._personaQuery.ValidarUsuario(personaCreateDto.Ci))
            {
                if (!await this.validarCardsRepetido(personaCreateDto))
                {
                    /*insertar*/
                    personaCreateDto.ControlIdSalt = "";
                    personaCreateDto.ControlIdRegistration = "";
                    personaCreateDto.ControlIdName = personaCreateDto.Nombre;

                    var persona = await this._personaQuery.Store(new Persona
                    {
                        Ci = personaCreateDto.Ci,
                        Nombre = personaCreateDto.Nombre,
                        Apellido = personaCreateDto.Apellido,
                        Celular = personaCreateDto.Celular,
                        Dirrecion = personaCreateDto.Dirrecion,
                        Fecha_nac = personaCreateDto.Fecha_nac,
                        Email = personaCreateDto.Email,
                        Observaciones = personaCreateDto.Observaciones,
                        ControlIdBegin_time = this.DateTimeToUnix(personaCreateDto.ControlIdBegin_time),
                        ControlIdEnd_time = this.DateTimeToUnix(personaCreateDto.ControlIdEnd_time),
                        ControlIdName = personaCreateDto.Nombre,
                        ControlIdPassword = personaCreateDto.ControlIdPassword,
                        ControlIdRegistration = "",
                        ControlIdSalt = "",
                    });
                    //guardar imagen
                    if (personaCreateDto.perfil != null)
                    {
                        var fileFoto = await this.UploadFoto(personaCreateDto.perfil);
                        var base64 = this.ConvertToBase64(personaCreateDto.perfil);
                        var imagenSave = await this.GuardarImagen(persona, personaCreateDto.perfil, fileFoto, base64);
                    }

                    if (personaCreateDto.Area != null)
                    {
                        int index = 0;
                        foreach (var area in personaCreateDto.Area)
                        {
                            TarjetaCreateDto tarjetaCreateDto = new TarjetaCreateDto
                            {
                                Area = Convert.ToInt32(area),
                                Codigo = Convert.ToInt32(personaCreateDto.Codigo[index]),
                                ControlId = 0,
                                PersonaId = persona.Id,
                                ControlIdsecret = "",
                                ControlIdValue = 0,
                                ControlIdUserId = 0
                            };
                            await this._tarjetaQuery.Store(tarjetaCreateDto);
                            index++;
                        }
                    }
                    var reglaAcceso = await this._reglaAccesoQuery.GetOne(personaCreateDto.ReglaAccesoId);
                    var insert = new PersonaReglasAcceso
                    {
                        PersonaId = persona.Id,
                        ReglaAccesoId = reglaAcceso.Id
                    };
                    var personaReglaAcceso = await this._personaReglaAccesoQuery.Store(insert);
                    await this.SaveDispositivo(persona, persona.perfil == null ? null : persona.perfil, reglaAcceso);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["ErrorCard"] = "Tarjeta ya registrada";
                return View("~/Views/Persona/Create.cshtml", personaCreateDto);
            }
            else
            {
                ViewData["Error"] = "CI ya fue registrado";
                return View("~/Views/Persona/Create.cshtml", personaCreateDto);
            }
        }
        else
        {
            return View("~/Views/Persona/Create.cshtml", personaCreateDto);
        }
    }
    //subir image 
    private async Task<string> UploadFoto(IFormFile imagen)
    {
        string folder = "images/perfiles/";
        string name = Guid.NewGuid().ToString() + imagen.FileName;
        string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder, name);
        var file = new FileStream(serverFolder, FileMode.Create);
        await imagen.CopyToAsync(file);
        await file.DisposeAsync();
        return name;
    }
    private string ConvertToBase64(IFormFile imagen)
    {
        var memoria = new MemoryStream();
        imagen.CopyTo(memoria);
        var bit = memoria.ToArray();
        var base64 = Convert.ToBase64String(bit);
        return base64;
    }
    private async Task<ImagenPerfil> GuardarImagen(Persona persona, IFormFile imagen, string ubicacion, string base64)
    {
        var nuevaImagen = new ImagenPerfil
        {
            base64 = base64,
            ControlIdImage = "",
            ControlUserId = 0,
            ControlIdTimestamp = 0,
            Name = imagen.FileName,
            Caption = imagen.ContentType,
            FechaCreacion = DateTime.Now,
            Path = ubicacion,
            Size = imagen.Length,
            PersonaId = persona.Id
        };
        var imagenStore = await this._imagenPerfilQuery.store(nuevaImagen);
        return imagenStore;
    }
    private async Task<ImagenPerfil> UpdateImagen(int id, Persona persona, IFormFile imagen, string ubicacion, string base64)
    {
        var nuevaImagen = new ImagenPerfil
        {
            base64 = base64,
            ControlIdImage = "",
            ControlUserId = 0,
            ControlIdTimestamp = 0,
            Name = imagen.FileName,
            Caption = imagen.ContentType,
            FechaCreacion = DateTime.Now,
            Path = ubicacion,
            Size = imagen.Length,
            PersonaId = persona.Id
        };
        var imagenStore = await this._imagenPerfilQuery.Update(id, nuevaImagen);
        return imagenStore;
    }
    private async Task<bool> validarCardsRepetido(PersonaCreateDto personaCreateDto)
    {
        bool bandera = false;
        int index = 0;
        if (personaCreateDto.Area != null)
        {
            foreach (var area in personaCreateDto.Area)
            {
                bandera = await this._tarjetaQuery.VerityCard(Convert.ToInt32(area), Convert.ToInt32(personaCreateDto.Codigo[index]));
                index++;
            }
        }
        return bandera;
    }
    private async Task<bool> validarCardsRepetidoEdit(PersonaDto personaCreate, int personaId)
    {
        bool bandera = false;
        int index = 0;
        if (personaCreate.Area != null)
        {
            foreach (var area in personaCreate.Area)
            {
                bandera = await this._tarjetaQuery.VerityCardEditar(Convert.ToInt32(area), Convert.ToInt32(personaCreate.Codigo[index]), personaId);
                index++;
            }
        }
        return bandera;
    }
    [HttpPost("data-table")]
    public ActionResult DataTable()
    {
        var dataTable = this._personaQuery.DataTable(Request);
        return Json(dataTable);
    }

    [HttpGet("editar/{id:int}")]
    public async Task<ActionResult> Edit(int id)
    {

        ViewData["ReglasAcceso"] = await this._reglaAccesoQuery.GetAll();
        ViewData["tarjetas"] = await this._tarjetaQuery.GetAllByPersona(id);
        var persona = await this._personaQuery.GetOne(id);
        if (persona == null)
        {
            return NotFound();
        }
        else
        {
            /*validar perfil*/
            var validarPerfi = persona.perfil == null ? "" : persona.perfil.Path;
            var editPersona = new PersonaDto
            {
                pathImagen = validarPerfi,
                Id = persona.Id,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Area = null,
                Celular = persona.Celular,
                Ci = persona.Ci,
                Codigo = null,
                Dirrecion = persona.Dirrecion,
                Email = persona.Email,
                Fecha_nac = persona.Fecha_nac,
                Observaciones = persona.Observaciones,
                ControlIdPassword = persona.ControlIdPassword,
                ControlIdBegin_time = this.UnixTimeStampToDateTime(persona.ControlIdBegin_time),
                ControlIdEnd_time = this.UnixTimeStampToDateTime(persona.ControlIdEnd_time)
            };
            return View("~/Views/Persona/Edit.cshtml", editPersona);
        }
    }

    [HttpPost("update/{id:int}")]
    public async Task<ActionResult> Update(int id, PersonaDto personaDto)
    {
        if (ModelState.IsValid)
        {
            //recuperacion de tarjetas anteriores
            ViewData["tarjetas"] = await this._tarjetaQuery.GetAllByPersona(id);
            if (await this._personaQuery.ValidateExistExceptoId(personaDto.Ci, id))
            {
                if (!await this.validarCardsRepetidoEdit(personaDto, id))
                {
                    var personaUpdate = new Persona
                    {
                        Id = id,
                        Celular = personaDto.Celular,
                        Apellido = personaDto.Apellido,
                        Ci = personaDto.Ci,
                        Nombre = personaDto.Nombre,
                        Email = personaDto.Email,
                        Observaciones = personaDto.Observaciones,
                        Dirrecion = personaDto.Dirrecion,
                        Fecha_nac = personaDto.Fecha_nac,
                        ControlIdPassword = personaDto.ControlIdPassword,

                        ControlIdBegin_time = this.DateTimeToUnix(personaDto.ControlIdBegin_time),
                        ControlIdEnd_time = this.DateTimeToUnix(personaDto.ControlIdEnd_time),
                    };
                    //guardar imagen

                    if (personaDto.perfil != null)
                    {
                        var delete = await this._imagenPerfilQuery.Delete(id);
                        var fileFoto = await this.UploadFoto(personaDto.perfil);
                        var base64 = this.ConvertToBase64(personaDto.perfil);
                        var imagenSave = await this.GuardarImagen(personaUpdate, personaDto.perfil, fileFoto, base64);
                    }
                    var persona = await this._personaQuery.UpdateOne(personaUpdate);

                    if (personaDto.Area != null)
                    {
                        var ids = new List<int>();
                        int index = 0;
                        foreach (var area in personaDto.Area)
                        {

                            var verificar = await this._tarjetaQuery.VerityCardAndReturn(Convert.ToInt32(area), Convert.ToInt32(personaDto.Codigo[index]));
                            if (verificar == null)
                            {
                                var nuevo = await this._tarjetaQuery.StoreControlId(new Tarjeta
                                {
                                    area = Convert.ToInt32(area),
                                    codigo = Convert.ToInt32(personaDto.Codigo[index]),
                                    PersonaId = id
                                });
                                ids.Add(nuevo.Id);
                            }
                            else
                            {
                                var update = await this._tarjetaQuery.UpdateOne(verificar);
                                ids.Add(update.Id);
                            }
                            index++;
                        }
                        var DeleteTarjeta = this._tarjetaQuery.DeleteTarjetasNoUsed(id, ids);
                    }
                    //borrado inicial
                    var personaReglaAcceso = await this._personaReglaAccesoQuery.CheckIfExist(id, personaDto.ReglaAccesoId);
                    if (personaReglaAcceso == null)
                    {
                        var insert = new PersonaReglasAcceso
                        {
                            PersonaId = persona.Id,
                            ReglaAccesoId = personaDto.ReglaAccesoId
                        };
                        var nuevo = await this._personaReglaAccesoQuery.Store(insert);
                    }

                    //dipositivo
                    await this.UpdateDispositivo(persona, persona.perfil == null ? null : persona.perfil, personaReglaAcceso.ReglaAcceso);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["ErrorCard"] = "Card ya fue registrado";
                return View("~/Views/Persona/Edit.cshtml", personaDto);
            }
            else
            {
                ViewData["Error"] = "CI ya fue registrado";
                return View("~/Views/Persona/Edit.cshtml", personaDto);
            }
        }
        else
        {
            return View("~/Views/Persona/Edit.cshtml", personaDto);
        }
    }
    [HttpDelete("delete/{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (ModelState.IsValid)
        {
            var persona = await this._personaQuery.GetOne(id);
            var delete = await this._personaQuery.Delete(persona.Id);
            await this.DeleteDispositivo(persona);
            return Json(new
            {
                status = "success",
                message = "Eliminado correctamente",
            });
        }
        return Json(new
        {
            status = "error",
            message = "A ocurrido un error",
        });
    }
    [HttpPost("buscar")]
    public async Task<ActionResult> buscar(PersonaCreateDto personaCreateDto)
    {
        var personas = await this._personaQuery.GetAllLikeCi(Convert.ToInt32(personaCreateDto.Ci));

        List<object> lista_personas = new List<object>();

        foreach (var persona in personas)
        {
            var fecha_inicio = this.UnixTimeStampToDateTime(persona.ControlIdBegin_time);
            var fecha_fin = this.UnixTimeStampToDateTime(persona.ControlIdEnd_time);
            lista_personas.Add(new
            {
                id = persona.Id,
                ci = persona.Ci,
                nombre = persona.Nombre,
                apellido = persona.Apellido,
                fecha_inicio = fecha_inicio,
                fecha_fin = fecha_fin,
                perfil = persona.perfil
            });
        }
        return Json(lista_personas);
    }

    [HttpPost("store-ajax")]
    public async Task<ActionResult> StoreAjax(PersonaCreateDto personaCreateDto)
    {
        var storePersona = await this._personaQuery.Store(new Persona
        {
            Ci = personaCreateDto.Ci,
            Nombre = personaCreateDto.Nombre,
            Apellido = personaCreateDto.Apellido,
            Celular = personaCreateDto.Celular,
            Dirrecion = personaCreateDto.Dirrecion,
            Email = personaCreateDto.Email,
            Fecha_nac = personaCreateDto.Fecha_nac,
            Observaciones = personaCreateDto.Observaciones,
            ControlIdBegin_time = this.DateTimeToUnix(personaCreateDto.ControlIdBegin_time),
            ControlIdEnd_time = this.DateTimeToUnix(personaCreateDto.ControlIdEnd_time),
            ControlIdName = personaCreateDto.Nombre,
            ControlIdPassword = personaCreateDto.ControlIdPassword,
            ControlIdRegistration = "",
            ControlIdSalt = "",
        });
        return Json(
            new
            {
                status = "success",
                data = storePersona,
                message = "Persona creado correctamente"
            }
        );
    }

    private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp son los segundos pasados después de una fecha establecida, por lo general unix utiliza esta fecha 
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    private long DateTimeToUnix(DateTime MyDateTime)
    {
        TimeSpan timeSpan = MyDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        var data = (long)timeSpan.TotalSeconds;
        System.Console.WriteLine(data);
        return data;
    }
    private long ConvertCard(string area, string codigo)
    {
        int area_convert = Int32.Parse(area);
        int area_codigo = Int32.Parse(codigo);
        long calculo = (area_convert * Convert.ToInt64((Math.Pow(2, 32)))) + area_codigo;

        return calculo;
    }
    /*
        *CARGAR DATA CONTROL ID 
    */
    /*login dispositivo*/
    private async Task<bool> LoginControlId(string ip, int port, string user, string api, string password)
    {
        BodyLogin cuerpo = _loginControlIdQuery.Login(user, password);
        Response login = await this._httpClientService.LoginRun(ip, port, api, cuerpo, "");
        /*valido si es el login fue ok*/
        this._usuarioControlIdQuery.Params(port, ip, user, password, login.data);
        this._cardControlIdQuery.Params(port, ip, user, password, login.data);
        this._registroRostroControlIdQuery.Params(port, ip, user, password, login.data);
        this._usuarioRulesAccessControlIdQuery.Params(port, ip, user, password, login.data);
        return login.estado;
    }
    /*------USUARIO------*/
    private async Task<bool> SaveDispositivo(Persona persona, ImagenPerfil imagenPerfil, ReglaAcceso reglasAcceso)
    {
        /*consutar por dispositivos*/
        var dispositivos = await this._dispositivoQuery.GetAll();
        foreach (var dispositivo in dispositivos)
        {
            var loginStatus = await this.LoginControlId(dispositivo.Ip, dispositivo.Puerto, dispositivo.Usuario, this._ApiRutas.ApiUrlLogin, dispositivo.Password);
            if (loginStatus)
            {
                //crear usuario
                await this.UserStoreControlId(persona);
                //CREATE REGLA ACCESOO
                var nueva = new List<PersonaReglasAcceso>();
                nueva.Add(new PersonaReglasAcceso
                {
                    ControlIdAccessRulesId = reglasAcceso.ControlId,
                    ControlIdUserId = persona.ControlId,
                    PersonaId = persona.Id,
                    ReglaAccesoId = reglasAcceso.Id,
                    Persona = persona,
                    ReglaAcceso = reglasAcceso

                });
                await this.StorePersonaAccesoRules(nueva);
                //crear tarjetas si hay 
                if (persona.card != null)
                {
                    await this.CardStoreControlId(persona);
                }
                if (persona.perfil != null)
                {
                    await this.ImageStoreControlId(persona, imagenPerfil);
                }
            }
        }
        return true;
    }

    private async Task<bool> UserStoreControlId(Persona persona)
    {
        var addUsuario = await this._usuarioControlIdQuery.CreateOneUser(persona);
        if (addUsuario.status)
        {
            persona.ControlId = addUsuario.ids[0];
            var updateUsuario = await this._personaQuery.UpdateOne(persona);
        }
        return addUsuario.status;
    }

    private async Task<bool> StorePersonaAccesoRules(List<PersonaReglasAcceso> personaReglasAccesos)
    {
        var apiResponse = await this._usuarioRulesAccessControlIdQuery.CreateAll(personaReglasAccesos);
        if (apiResponse.status)
        {
            foreach (var personaReglasAcceso in personaReglasAccesos)
            {
                personaReglasAcceso.ControlIdAccessRulesId = personaReglasAcceso.ReglaAcceso.ControlId;
                personaReglasAcceso.ControlIdUserId = personaReglasAcceso.Persona.ControlId;
                await this._personaReglaAccesoQuery.UpdateControlId(personaReglasAcceso);
            }
            return apiResponse.status;
        }
        else
        {
            return apiResponse.status;
        }
    }

    private async Task<bool> ImageStoreControlId(Persona persona, ImagenPerfil imagenPerfil)
    {
        var addImagen = await this._registroRostroControlIdQuery.Create(persona.ControlId, imagenPerfil.base64, this.DateTimeToUnix(imagenPerfil.FechaCreacion));
        if (addImagen.status)
        {
            imagenPerfil.ControlUserId = persona.ControlId;
            imagenPerfil.ControlIdImage = imagenPerfil.ControlIdImage;
            imagenPerfil.ControlIdTimestamp = this.DateTimeToUnix(imagenPerfil.FechaCreacion);
            var updatePefil = await this._imagenPerfilQuery.UpdateControlId(imagenPerfil);
        }
        return addImagen.status;
    }
    private async Task<bool> ImageDeleteControlId(Persona persona)
    {
        var addImagen = await this._registroRostroControlIdQuery.Delete(persona.ControlId);
        if (addImagen.status)
        {
            //var updatePefil = await this._imagenPerfilQuery.UpdateControlId(imagenPerfil);
        }
        return addImagen.status;
    }
    /*------CARD------*/
    private cardsCreateDto ConvertCard(int usuarioId, string area, string codigo)
    {
        int area_convert = Int32.Parse(area);
        int area_codigo = Int32.Parse(codigo);
        long calculo = (area_convert * Convert.ToInt64((Math.Pow(2, 32)))) + area_codigo;
        var card = new cardsCreateDto
        {
            user_id = usuarioId,
            value = calculo
        };
        return card;
    }
    private async Task<bool> CardStoreControlId(Persona persona)
    {
        List<cardsCreateDto> cardsCreateDto = new List<cardsCreateDto>();
        if (persona.card.Count > 0)
        {
            foreach (var card in persona.card)
            {
                var createCard = await this._cardControlIdQuery.CreateAll(persona.card);
                if (createCard.status)
                {
                    card.ControlId = createCard.ids[0];
                    card.ControlIdsecret = "";
                    card.ControlIdUserId = persona.ControlId;
                    card.ControlIdValue = this.ConvertCard(card.area.ToString(), card.codigo.ToString());
                    var updateCard = await this._tarjetaQuery.UpdateOneControlId(card);
                }
            }
        }
        return true;
    }
    /*
       *UPDATE DATA CONTROL ID 
   */
    private async Task<bool> UpdateDispositivo(Persona persona, ImagenPerfil imagenPerfil, ReglaAcceso reglasAcceso)
    {
        /*consutar por dispositivos*/
        var dispositivos = await this._dispositivoQuery.GetAll();
        foreach (var dispositivo in dispositivos)
        {
            var loginStatus = await this.LoginControlId(dispositivo.Ip, dispositivo.Puerto, dispositivo.Usuario, this._ApiRutas.ApiUrlLogin, dispositivo.Password);
            if (loginStatus)
            {
                //crear usuario
                await this.UserUpdateControlId(persona);
                //DELETE y CREATE REGLA ACCESOO
                await this.DeletePersonaAccesoRules(persona.ControlId);
                var nueva = new List<PersonaReglasAcceso>();
                nueva.Add(new PersonaReglasAcceso
                {
                    ControlIdAccessRulesId = reglasAcceso.ControlId,
                    ControlIdUserId = persona.ControlId,
                    PersonaId = persona.Id,
                    ReglaAccesoId = reglasAcceso.Id,
                    Persona = persona,
                    ReglaAcceso = reglasAcceso
                });
                await this.StorePersonaAccesoRules(nueva);
                //crear tarjetas si hay 
                if (persona.card.Count > 0)
                {
                    await this.CardDeleteControlId(persona);
                    await this.CardStoreControlId(persona);
                }
                if (persona.perfil != null)
                {
                    await this.ImageDeleteControlId(persona);
                    await this.ImageStoreControlId(persona, imagenPerfil);
                }
            }
        }
        return true;
    }
    private async Task<bool> UserUpdateControlId(Persona persona)
    {
        var addUsuario = await this._usuarioControlIdQuery.Update(persona);
        if (addUsuario.status)
        {
            //si el cambio due exitoso
            if (addUsuario.changes == 1)
            {
                var updateUsuario = await this._personaQuery.UpdateOne(persona);
            }
        }
        return addUsuario.status;
    }

    private async Task<bool> DeletePersonaAccesoRules(int UsuarioId)
    {
        var apiResponse = await this._usuarioRulesAccessControlIdQuery.DeleteReglaUsuarioId(UsuarioId);
        if (apiResponse.status)
        {
            return apiResponse.status;
        }
        else
        {
            return apiResponse.status;
        }
    }
    private async Task<bool> CardUpdateControlId(Persona persona)
    {
        List<cardsCreateDto> cardsCreateDto = new List<cardsCreateDto>();
        //elimina base de tarjetas anteriores para recrear
        await this._cardControlIdQuery.DeleteAllUserId(persona.ControlId);
        foreach (var card in persona.card)
        {
            if (card.ControlId != 0)
            {
                var update = await this._cardControlIdQuery.Create(card);
                if (update.status)
                {
                    card.ControlId = update.ids[0];
                    card.ControlIdsecret = "";
                    card.ControlIdUserId = persona.ControlId;
                    card.ControlIdValue = this.ConvertCard(card.area.ToString(), card.codigo.ToString());
                    var updateCard = await this._tarjetaQuery.UpdateOneControlId(card);
                }
            }
        }
        return true;
    }
    /*
       *DELETE DATA CONTROL ID 
    */
    private async Task<bool> DeleteDispositivo(Persona persona)
    {
        /*consutar por dispositivos*/
        var dispositivos = await this._dispositivoQuery.GetAll();
        foreach (var dispositivo in dispositivos)
        {
            var loginStatus = await this.LoginControlId(dispositivo.Ip, dispositivo.Puerto, dispositivo.Usuario, this._ApiRutas.ApiUrlLogin, dispositivo.Password);
            if (loginStatus)
            {
                //crear usuario
                await this.UserDeleteControlId(persona);
                //crear tarjetas si hay 
                if (persona.card != null)
                {
                    //await this.CardUpdateControlId(persona);
                }
            }
        }
        return true;
    }
    private async Task<bool> UserDeleteControlId(Persona persona)
    {
        var addUsuario = await this._usuarioControlIdQuery.Delete(persona);
        if (addUsuario.status)
        {
            //si el eliminacion due exitoso
        }
        return addUsuario.status;
    }
    private async Task<bool> CardDeleteControlId(Persona persona)
    {
        if (persona.card.Count > 0)
        {
            var createCard = await this._cardControlIdQuery.DeleteAllUserId(persona.ControlId);
            if (createCard.status)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}


