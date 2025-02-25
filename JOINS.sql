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
