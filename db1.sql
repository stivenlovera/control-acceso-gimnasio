INSERT INTO
    `proyecto`(
        `Nombre`,
        `Descripcion`,
        `Dirrecion`,
        `Fecha_inicio`,
        `Fecha_fin`,
        `Telefono`,
        `Email`,
        `Estado`
    )
VALUES (
        'casa de sergio',
        'casas de sergio',
        '4to anillo radias 13',
        '2021-09-09',
        '2020-09-09',
        '3356690',
        'sergio@a.com',
        '1'
    )
    /*puertas */
INSERT INTO
    `portal`(
        `Nombre`,
        `Descripcion`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAreaFromId`,
        `ControlIdAreaToId`,
        `DispositivoId`
    )
VALUES (
        'Puerta 1',
        ' ',
        '1',
        'TO: Área Estándar',
        '1',
        '1',
        '1'
    );

INSERT INTO
    `portal`(
        `Nombre`,
        `Descripcion`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAreaFromId`,
        `ControlIdAreaToId`,
        `DispositivoId`
    )
VALUES (
        'Puerta 2',
        ' ',
        '2',
        'TO: Área Estándar',
        '1',
        '1',
        '1'
    );

INSERT INTO
    `portal`(
        `Nombre`,
        `Descripcion`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAreaFromId`,
        `ControlIdAreaToId`,
        `DispositivoId`
    )
VALUES (
        'Puerta 3',
        ' ',
        '3',
        'TO: Área Estándar',
        '1',
        '1',
        '1'
    );

INSERT INTO
    `portal`(
        `Nombre`,
        `Descripcion`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAreaFromId`,
        `ControlIdAreaToId`,
        `DispositivoId`
    )
VALUES (
        'Puerta 4',
        ' ',
        '4',
        'TO: Área Estándar',
        '1',
        '1',
        '1'
    );

/*acciones*/

INSERT INTO
    `accion`(
        `Nombre`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAction`,
        `ControlIdParametrers`,
        `ControlIdRunAt`
    )
VALUES (
        'puerta 1',
        '1',
        'Abrir porta 1',
        'door',
        'door=1, reason=1',
        '0'
    );

INSERT INTO
    `accion`(
        `Nombre`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAction`,
        `ControlIdParametrers`,
        `ControlIdRunAt`
    )
VALUES (
        'puerta 2',
        '2',
        'Abrir porta 2',
        'door',
        'door=2, reason=1',
        '0'
    );

INSERT INTO
    `accion`(
        `Nombre`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAction`,
        `ControlIdParametrers`,
        `ControlIdRunAt`
    )
VALUES (
        'puerta 3',
        '3',
        'Abrir porta 3',
        'door',
        'door=3, reason=1',
        '0'
    );

INSERT INTO
    `accion`(
        `Nombre`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdAction`,
        `ControlIdParametrers`,
        `ControlIdRunAt`
    )
VALUES (
        'puerta 4',
        '1',
        'Abrir porta 4',
        'door',
        'door=4, reason=1',
        '0'
    );

/*potal accion*/

INSERT INTO
    `accionportal`(
        `ControlIdPortalId`,
        `portalId`,
        `AccionId`,
        `ControlIdAreaId`
    )
VALUES ('1', '1', '1', '1');

INSERT INTO
    `accionportal`(
        `ControlIdPortalId`,
        `portalId`,
        `AccionId`,
        `ControlIdAreaId`
    )
VALUES ('2', '2', '2', '2');

INSERT INTO
    `accionportal`(
        `ControlIdPortalId`,
        `portalId`,
        `AccionId`,
        `ControlIdAreaId`
    )
VALUES ('3', '3', '3', '3');

INSERT INTO
    `accionportal`(
        `ControlIdPortalId`,
        `portalId`,
        `AccionId`,
        `ControlIdAreaId`
    )
VALUES ('4', '4', '4', '4');

/*area*/

INSERT INTO
    `area`(
        `Descripcion`,
        `Nombre`,
        `ControlId`,
        `ControlIdName`
    )
VALUES (
        ' ',
        'Estandar',
        '1',
        'Área Estándar'
    );

INSERT INTO
    `dispositivo`(
        `Nombre`,
        `Modelo`,
        `Puerto`,
        `Ip`,
        `NumeroSerie`,
        `Usuario`,
        `Password`,
        `ControlId`,
        `ControlIdName`,
        `ControlIdIp`,
        `ControlIdPublicKey`
    )
VALUES (
        'controladora',
        'IdBox',
        '80',
        '192.168.88.129',
        '0',
        'admin',
        'admin',
        '1',
        'L1QN',
        '192.168.88.129',
        'MIIBCgKCAQEAn4TC3VZ17dXymM6pYRdkG8IYTUVv6glzYD7VunE6R4jg9mjIyi51z7ZwfJxh3SL2euQZZnOaR+FKtMFIZIz9z+jFYqUd8mC/8Pq4N6sgCUfrHcURyMjAJi4zOE+gGzlTyFj21LWvh5Bywewhqt051+Xds1Vpm5h25vzrry0WWM7yjvIFk899xQ4P+LVxPYScpVViS768XmD8bept6ljZL03LMXOM1+fELRWCdBujFdSLSjMFnWMX5zfrmr9C0Tx/F+oUDKp52D81iUEWIctL+rYyVBrhAEwxB2XH/EV693cqPbhbeE6sHsuFfsDZRZylAYPIeKB2YUT0fRXSyJE70wIDAQAB'
    )

    INSERT INTO `paquete`( `nombre`, `dias`, `fechaCreacion`, `costo`) VALUES ('2 x 1 primavera','30','2022-09-26','200');