
$(document).on("click", "#buscar_cliente", function () {
    console.log('evento click');
    $('#contenido').html('');
    $('#modal_buscar_cliente').modal('show');
    $.ajax({
        type: 'POST',
        url: `/persona/buscar`,
        dataType: "json",
        data: {
            Ci: $('#Buscar').val()
        },
        success: function (response) {
            $('#contenido').append(render_resutado(response));
        },
        error: function (jqXHR, textStatus, errorThrown) {
            error_status(jqXHR)
        },
        fail: function () {
            fail()
        }
    });
});
$(document).on("keypress", "#Buscar", function (e) {
    if (e.which == 13) {
        $('#modal_buscar_cliente').modal('show');
    }
});


function render_resutado(response) {
    resultadoHtml = ``;
    if (response.length > 0) {
        response.forEach(persona => {
            let image = "";
            if (persona.perfil == null) {
                image = "profile_user.jpg";
            } else {
                image = persona.perfil.path;
            }
            resultadoHtml += ` 
        <div class="col-md-6 col-sm-6 profile_details">
            <div class="well profile_view">
                <div class="col-sm-12">
                    <div class="left col-md-7 col-sm-7">
                        <p><strong>CI: </strong>${persona.ci}</p>
                        <p><strong>Nombres: </strong>${persona.nombre}</p>
                        <p><strong>Apellidos: </strong>${persona.apellido}</p>
                        <p><strong>Dirrecion: </strong>${persona.dirrecion}</p>
                        <ul class="list-unstyled">
                            <!--li><i class="fa fa-building"></i>${persona.dirrecion}</li-->
                            <li><i class="fa fa-phone"></i>${persona.celular} </li>
                        </ul>
                    </div>
                    <div class="right col-md-5 col-sm-5 text-center">
                        <img src="/images/perfiles/${image}"
                            alt="" class="img-circle img-fluid">
                    </div>
                </div>
                <div class=" profile-bottom text-center">
                    <div class="col-sm-12 emphasis">
                        <button type="button" class="btn btn-success btn-sm añadir_cliente" data-id="${persona.id}" data-ci="${persona.ci}" data-nombre="${persona.nombre}" data-apellido="${persona.apellido}"  data-fecha_inicio="${persona.fecha_inicio}"  data-fecha_fin="${persona.fecha_fin}">
                            Inscribir
                        </button>
                    </div>
                </div>
            </div>
        </div>`;
        });
    } else {
        resultadoHtml = `No hay resultado.`; // en el caso de no haber  usuario
    }
    return resultadoHtml;
}

/*captura de informacion */
$(document).on('click', '.añadir_cliente', function () {
    /*adicionar campos */
    añadir_cliente(
        $(this).data('id'),
        $(this).data('ci'),
        $(this).data('nombre'),
        $(this).data('apellido'),
        $(this).data('fecha_inicio'),
        $(this).data('fecha_fin')
    );
    $('#PaqueteId').prop("disabled", false);
    $('#modal_buscar_cliente').modal('hide');
});
/* añadir cliente */
function añadir_cliente(
    id,
    ci,
    nombre,
    apellido,
    fecha_inicio,
    fecha_fin
) {
    $('#PersonaId').val(id)
    $('#CI').val(`${ci}`)
    $('#Nombres').val(`${nombre}`)
    $('#Apellidos').val(`${apellido}`)
    $('#FechaInicio').val(moment(fecha_inicio).format("YYYY/MM/DD"))
    $('#FechaFin').val(moment(fecha_fin).format("YYYY/MM/DD"))
    $('#FechaFinCliente').val(moment(fecha_inicio).format("YYYY/MM/DD"))
    $('#FechaInicioCliente').val(moment(fecha_fin).format("YYYY/MM/DD"))
}

