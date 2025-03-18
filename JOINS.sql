-- Ver cant de aeropuertos por pais
SELECT 
    pai.idPais,
    pai.nombre AS País,
    COUNT(aer.idCiudad) AS Total_Aeropuertos
FROM PAISES pai
LEFT JOIN CIUDADES ciu ON pai.idPais = ciu.idPais
LEFT JOIN AEROPUERTOS aer ON ciu.idCiudad = aer.idCiudad
GROUP BY pai.idPais, pai.nombre
ORDER BY pai.idPais;

-- ver info de pais y continente al que pertenece
SELECT 
    p.nombre AS Pais,
	p.idPais AS IDP,
    c.nombre AS Continente,
    c.idContinente AS IDC
FROM PAISES p
INNER JOIN CONTINENTES c ON p.idContinente = c.idContinente;

-- ver users y sus roles
SELECT 
	u.idRol AS Id_Rol,
	r.nombre AS Rol,
	u.correo AS Correo,
	u.contrasena AS PSSWRD
FROM USUARIOS u
INNER JOIN ROLES r ON u.idRol = r.idRol;

-- ver ciudades y pais al que pertenecen
SELECT 
	ciu.nombre AS Ciudad,
	pai.nombre AS País
FROM CIUDADES	ciu
INNER JOIN PAISES pai ON ciu.idPais = pai.idPais;
-- ORDER BY ciu.nombre ASC;

-- aeropuertos y sus ciudades
SELECT 
    A.idAeropuerto, 
    A.nombre AS NombreAeropuerto, 
    C.idCiudad, 
    C.nombre AS NombreCiudad
FROM AEROPUERTOS A
INNER JOIN CIUDADES C ON A.idCiudad = C.idCiudad;

SELECT COUNT(*) AS TotalAeropuertos FROM AEROPUERTOS;

-- Eliminar todos los registros de la tabla
DELETE FROM AEROPUERTOS;

-- Reiniciar el contador de IDENTITY en SQL Server
DBCC CHECKIDENT ('AEROPUERTOS', RESEED, 0);

-- Ver todas las fk de las tablas
SELECT 
    tc.TABLE_NAME, 
    tc.CONSTRAINT_NAME, 
    kcu.COLUMN_NAME, 
    rc.UNIQUE_CONSTRAINT_NAME AS Referenced_Constraint
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu 
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS rc
    ON tc.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY';

-- Ver asientos reservados
SELECT 
    va.idVueloAsiento, 
    va.numero AS Asiento, 
    ca.nombre AS Categoria, 
    ca.precio AS Precio, 
    va.estado, 
    v.codigo_vuelo AS CodigoVuelo, 
    v.fecha_salida, 
    v.fecha_llegada 
FROM VUELOS_ASIENTOS va
JOIN CATEGORIAS_ASIENTOS ca ON va.idCategoria = ca.idCategoria
JOIN VUELOS v ON va.idVuelo = v.idVuelo
WHERE va.estado <> 'Disponible'; -- Filtra los asientos que no están disponibles

UPDATE VUELOS_ASIENTOS
SET estado = 'Disponible'
WHERE estado = 'Reservado';
