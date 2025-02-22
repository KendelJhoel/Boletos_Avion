-- #_ Z O N A S _#
SELECT 
    p.nombre AS Pais,
	p.idPais AS IDP,
    c.nombre AS Continente,
    c.idContinente AS IDC
FROM PAISES p
INNER JOIN CONTINENTES c ON p.idContinente = c.idContinente;

INSERT INTO CONTINENTES (nombre) VALUES
('América'),	-- 1
('Asia'),		-- 2
('Europa'),		-- 3
('Oceanía');	-- 4

INSERT INTO PAISES (nombre, idContinente) VALUES
-- América (idContinente = 1)
('Argentina', 1),
('Belice', 1),
('Brasil', 1),
('Canadá', 1),
('Chile', 1),
('Colombia', 1),
('Costa Rica', 1),
('El Salvador', 1),
('Estados Unidos', 1),
('Guatemala', 1),
('Honduras', 1),
('México', 1),
('Panamá', 1),
('Perú', 1),
('Puerto Rico', 1),
('República Dominicana', 1),

-- Asia (idContinente = 2)
('Japón', 2),

-- Europa (idContinente = 3)
('Alemania', 3),
('España', 3),
('Francia', 3),
('Portugal', 3),
('Reino Unido', 3),
('Suecia', 3),

-- Oceanía (idContinente = 4)
('Australia', 4),
('Nueva Zelanda', 4);




-- #_ U S U A R I O S _#

-- Insertar los 3 roles de usuario
INSERT INTO ROLES (nombre) 
VALUES
('Administrador'),('Agente'),('Cliente')

SELECT * FROM ROLES

-- Insertar los 3 usuarios administradores
INSERT INTO USUARIOS (nombre, correo, telefono, direccion, documento_identidad, contrasena, idRol)
VALUES
('a_ken', 'Kendel.Arevalo@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN001', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador')),
('a_mig', 'Miguel.Leiva@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN002', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador')),
('a_jon', 'Jonathan.Barrientos@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN003', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador'));

SELECT 
	u.idRol AS Id_Rol,
	r.nombre AS Rol,
	u.correo AS Correo,
	u.contrasena AS PSSWRD
FROM USUARIOS u
INNER JOIN ROLES r ON u.idRol = r.idRol;

select * from USUARIOS

delete USUARIOS where idUsuario=10
delete USUARIOS where idUsuario=11


-- #_ CIUDADES (5 más importantes de cada país agregado a la bd _#

-- Ciudades de Argentina (idPais = 1)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Buenos Aires', 1),
('Córdoba', 1),
('Rosario', 1),
('Mendoza', 1),
('La Plata', 1);

-- Ciudades de Belize (idPais = 2)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Belice City', 2),
('Belmopan', 2),
('San Ignacio', 2),
('Orange Walk', 2),
('Corozal', 2);

-- Ciudades de Brasil (idPais = 3)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('São Paulo', 3),
('Rio de Janeiro', 3),
('Brasília', 3),
('Salvador', 3),
('Fortaleza', 3);

-- Ciudades de Canadá (idPais = 4)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Toronto', 4),
('Montreal', 4),
('Vancouver', 4),
('Calgary', 4),
('Ottawa', 4);

-- Ciudades de Chile (idPais = 5)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Santiago', 5),
('Valparaíso', 5),
('Concepción', 5),
('La Serena', 5),
('Temuco', 5);

-- Ciudades de Colombia (idPais = 6)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Bogotá', 6),
('Medellín', 6),
('Cali', 6),
('Barranquilla', 6),
('Cartagena', 6);

-- Ciudades de Costa Rica (idPais = 7)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('San José', 7),
('Alajuela', 7),
('Heredia', 7),
('Cartago', 7),
('Liberia', 7);

-- Ciudades de El Salvador (idPais = 8)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('San Salvador', 8),
('Santa Ana', 8),
('San Miguel', 8),
('Soyapango', 8),
('Mejicanos', 8);

SELECT idCiudad FROM CIUDADES WHERE nombre='Santa Ana'

-- Ciudades de Estados Unidos (idPais = 9)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('New York', 9),
('Los Angeles', 9),
('Chicago', 9),
('Houston', 9),
('Phoenix', 9);

-- Ciudades de Guatemala (idPais = 10)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Ciudad de Guatemala', 10),
('Quetzaltenango', 10),
('Escuintla', 10),
('Mazatenango', 10),
('Cobán', 10);

-- Ciudades de Honduras (idPais = 11)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Tegucigalpa', 11),
('San Pedro Sula', 11),
('La Ceiba', 11),
('Choluteca', 11),
('El Progreso', 11);

-- Ciudades de México (idPais = 12)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Ciudad de México', 12),
('Guadalajara', 12),
('Monterrey', 12),
('Puebla', 12),
('Tijuana', 12);

-- Ciudades de Panamá (idPais = 13)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Ciudad de Panamá', 13),
('San Miguelito', 13),
('Tocumen', 13),
('Colón', 13),
('David', 13);

-- Ciudades de Perú (idPais = 14)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Lima', 14),
('Arequipa', 14),
('Trujillo', 14),
('Chiclayo', 14),
('Cusco', 14);

-- Ciudades de Puerto Rico (idPais = 15)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('San Juan', 15),
('Ponce', 15),
('Mayagüez', 15),
('Caguas', 15),
('Bayamón', 15);

-- Ciudades de República Dominicana (idPais = 16)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Santo Domingo', 16),
('Santiago de los Caballeros', 16),
('La Romana', 16),
('San Pedro de Macorís', 16),
('Puerto Plata', 16);

-- Ciudades de Japón (idPais = 17)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Tokio', 17),
('Yokohama', 17),
('Osaka', 17),
('Nagoya', 17),
('Sapporo', 17);

-- Ciudades de Alemania (idPais = 18)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Berlín', 18),
('Múnich', 18),
('Frankfurt', 18),
('Hamburgo', 18),
('Colonia', 18);

-- Ciudades de España (idPais = 19)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Madrid', 19),
('Barcelona', 19),
('Valencia', 19),
('Sevilla', 19),
('Zaragoza', 19);

-- Ciudades de Francia (idPais = 20)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('París', 20),
('Marsella', 20),
('Lyon', 20),
('Toulouse', 20),
('Niza', 20);

-- Ciudades de Portugal (idPais = 21)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Lisboa', 21),
('Oporto', 21),
('Braga', 21),
('Coimbra', 21),
('Faro', 21);

-- Ciudades de Reino Unido (idPais = 22)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Londres', 22),
('Birmingham', 22),
('Manchester', 22),
('Glasgow', 22),
('Liverpool', 22);

-- Ciudades de Suecia (idPais = 23)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Estocolmo', 23),
('Gotemburgo', 23),
('Malmö', 23),
('Uppsala', 23),
('Västerås', 23);

-- Ciudades de Australia (idPais = 24)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Sídney', 24),
('Melbourne', 24),
('Brisbane', 24),
('Perth', 24),
('Adelaida', 24);

-- Ciudades de Nueva Zelanda (idPais = 25)
INSERT INTO CIUDADES (nombre, idPais) VALUES
('Auckland', 25),
('Wellington', 25),
('Christchurch', 25),
('Hamilton', 25),
('Dunedin', 25);

-- ver ciudades y pais al que pertenecen
SELECT 
	ciu.nombre AS Ciudad,
	pai.nombre AS País
FROM CIUDADES	ciu
INNER JOIN PAISES pai ON ciu.idPais = pai.idPais;
-- ORDER BY ciu.nombre ASC;



-- #_ A E R O P U E R T O S _#

-- Argentina (idPais = 1)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('BUE', 'Argentinian BUE1-1 Airport', 1),
('COR', 'Argentinian COR1-2 Airport', 2),
('ROS', 'Argentinian ROS1-3 Airport', 3),
('MEN', 'Argentinian MEN1-4 Airport', 4),
('LAP', 'Argentinian LAP1-5 Airport', 5);

-- Belize (idPais = 2)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('BLC', 'Belizean BLC2-6 Airport', 6),
('BEM', 'Belizean BEM2-7 Airport', 7),
('SNI', 'Belizean SNI2-8 Airport', 8),
('ORW', 'Belizean ORW2-9 Airport', 9),
('CRZ', 'Belizean CRZ2-10 Airport', 10);

-- Brasil (idPais = 3)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SAO', 'Brazilian SAO3-11 Airport', 11),
('RIO', 'Brazilian RIO3-12 Airport', 12),
('BRA', 'Brazilian BRA3-13 Airport', 13),
('SAL', 'Brazilian SAL3-14 Airport', 14),
('FOR', 'Brazilian FOR3-15 Airport', 15);

-- Canadá (idPais = 4)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('TOR', 'Canadian TOR4-16 Airport', 16),
('MON', 'Canadian MON4-17 Airport', 17),
('VAN', 'Canadian VAN4-18 Airport', 18),
('CAL', 'Canadian CAL4-19 Airport', 19),
('OTT', 'Canadian OTT4-20 Airport', 20);

-- Chile (idPais = 5)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SAN', 'Chilean SAN5-21 Airport', 21),
('VAL', 'Chilean VAL5-22 Airport', 22),
('CON', 'Chilean CON5-23 Airport', 23),
('LAS', 'Chilean LAS5-24 Airport', 24),
('TEM', 'Chilean TEM5-25 Airport', 25);

-- Colombia (idPais = 6)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('BOG', 'Colombian BOG6-26 Airport', 26),
('MED', 'Colombian MED6-27 Airport', 27),
('CLI', 'Colombian CLI6-28 Airport', 28),
('BAR', 'Colombian BAR6-29 Airport', 29),
('CAR', 'Colombian CAR6-30 Airport', 30);

-- Costa Rica (idPais = 7)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SJO', 'Costa Rican SJO7-31 Airport', 31),
('ALA', 'Costa Rican ALA7-32 Airport', 32),
('HER', 'Costa Rican HER7-33 Airport', 33),
('CTA', 'Costa Rican CTA7-34 Airport', 34),
('LIB', 'Costa Rican LIB7-35 Airport', 35);

-- El Salvador (idPais = 8)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SSA', 'Salvadorian SSA8-36 Airport', 36),
('STA', 'Salvadorian STA8-37 Airport', 37),
('SMI', 'Salvadorian SMI8-38 Airport', 38),
('SOY', 'Salvadorian SOY8-39 Airport', 39),
('MEJ', 'Salvadorian MEJ8-40 Airport', 40);

-- Estados Unidos (idPais = 9)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('NYC', 'American NYC9-41 Airport', 41),
('LAX', 'American LAX9-42 Airport', 42),
('CHI', 'American CHI9-43 Airport', 43),
('HOU', 'American HOU9-44 Airport', 44),
('PHX', 'American PHX9-45 Airport', 45);

-- Guatemala (idPais = 10)
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('GUA', 'Guatemalan GUA10-46 Airport', 46),
('QUE', 'Guatemalan QUE10-47 Airport', 47),
('ESC', 'Guatemalan ESC10-48 Airport', 48),
('MAZ', 'Guatemalan MAZ10-49 Airport', 49),
('COB', 'Guatemalan COB10-50 Airport', 50);

-- Honduras (idPais = 11, gentilicio "Honduran")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('TEG', 'Honduran TEG11-51 Airport', 51),
('SPS', 'Honduran SPS11-52 Airport', 52),
('LCE', 'Honduran LCE11-53 Airport', 53),
('CHO', 'Honduran CHO11-54 Airport', 54),
('EPR', 'Honduran EPR11-55 Airport', 55);

-- México (idPais = 12, gentilicio "Mexican")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('CDM', 'Mexican CDM12-56 Airport', 56),
('GDL', 'Mexican GDL12-57 Airport', 57),
('MTY', 'Mexican MON12-58 Airport', 58),
('PUB', 'Mexican PUB12-59 Airport', 59),
('TIJ', 'Mexican TIJ12-60 Airport', 60);

-- Panamá (idPais = 13, gentilicio "Panamanian")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('CDP', 'Panamanian CDP13-61 Airport', 61),
('SMG', 'Panamanian SMG13-62 Airport', 62),
('TOC', 'Panamanian TOC13-63 Airport', 63),
('CLN', 'Panamanian COL13-64 Airport', 64),
('DAV', 'Panamanian DAV13-65 Airport', 65);

-- Perú (idPais = 14, gentilicio "Peruvian")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('LIM', 'Peruvian LIM14-66 Airport', 66),
('ARE', 'Peruvian ARE14-67 Airport', 67),
('TRU', 'Peruvian TRU14-68 Airport', 68),
('CHY', 'Peruvian CHI14-69 Airport', 69),
('CUS', 'Peruvian CUS14-70 Airport', 70);

-- Puerto Rico (idPais = 15, gentilicio "Puerto Rican")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SJU', 'Puerto Rican SJU15-71 Airport', 71),
('PON', 'Puerto Rican PON15-72 Airport', 72),
('MAY', 'Puerto Rican MAY15-73 Airport', 73),
('CAG', 'Puerto Rican CAG15-74 Airport', 74),
('BAY', 'Puerto Rican BAY15-75 Airport', 75);

-- República Dominicana (idPais = 16, gentilicio "Dominican")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SDO', 'Dominican SDO16-76 Airport', 76),
('STC', 'Dominican STC16-77 Airport', 77),
('LRO', 'Dominican LRO16-78 Airport', 78),
('SPM', 'Dominican SPM16-79 Airport', 79),
('PPL', 'Dominican PPL16-80 Airport', 80);

-- Japón (idPais = 17, gentilicio "Japanese")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('TYO', 'Japanese TYO17-81 Airport', 81),
('YOK', 'Japanese YOK17-82 Airport', 82),
('OSA', 'Japanese OSA17-83 Airport', 83),
('NAG', 'Japanese NAG17-84 Airport', 84),
('SAP', 'Japanese SAP17-85 Airport', 85);

-- Alemania (idPais = 18, gentilicio "German")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('BER', 'German BER18-86 Airport', 86),
('MUN', 'German MUN18-87 Airport', 87),
('FRA', 'German FRA18-88 Airport', 88),
('HAM', 'German HAM18-89 Airport', 89),
('COL', 'German COL18-90 Airport', 90);

-- España (idPais = 19, gentilicio "Spanish")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('MAD', 'Spanish MAD19-91 Airport', 91),
('BRC', 'Spanish BAR19-92 Airport', 92),
('VLC', 'Spanish VAL19-93 Airport', 93),
('SEV', 'Spanish SEV19-94 Airport', 94),
('ZAR', 'Spanish ZAR19-95 Airport', 95);

-- Francia (idPais = 20, gentilicio "French")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('PAR', 'French PAR20-96 Airport', 96),
('MAR', 'French MAR20-97 Airport', 97),
('LYO', 'French LYO20-98 Airport', 98),
('TOU', 'French TOU20-99 Airport', 99),
('NIZ', 'French NIZ20-100 Airport', 100);

-- Portugal (idPais = 21, gentilicio "Portuguese")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('LIS', 'Portuguese LIS21-101 Airport', 101),
('OPO', 'Portuguese OPO21-102 Airport', 102),
('BRG', 'Portuguese BRG21-103 Airport', 103),
('COI', 'Portuguese COI21-104 Airport', 104),
('FAR', 'Portuguese FAR21-105 Airport', 105);

-- Reino Unido (idPais = 22, gentilicio "British")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('LON', 'British LON22-106 Airport', 106),
('BIR', 'British BIR22-107 Airport', 107),
('MAN', 'British MAN22-108 Airport', 108),
('GLA', 'British GLA22-109 Airport', 109),
('LIV', 'British LIV22-110 Airport', 110);

-- Suecia (idPais = 23, gentilicio "Swedish")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('STO', 'Swedish STO23-111 Airport', 111),
('GOT', 'Swedish GOT23-112 Airport', 112),
('MAL', 'Swedish MAL23-113 Airport', 113),
('UPP', 'Swedish UPP23-114 Airport', 114),
('VAS', 'Swedish VAS23-115 Airport', 115);

-- Australia (idPais = 24, gentilicio "Australian")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('SYD', 'Australian SYD24-116 Airport', 116),
('MEL', 'Australian MEL24-117 Airport', 117),
('BNE', 'Australian BNE24-118 Airport', 118),
('PER', 'Australian PER24-119 Airport', 119),
('ADL', 'Australian ADL24-120 Airport', 120);

-- Nueva Zelanda (idPais = 25, gentilicio "New Zealander")
INSERT INTO AEROPUERTOS (codigo, nombre, idCiudad) VALUES
('AKL', 'New Zealander AKL25-121 Airport', 121),
('WLG', 'New Zealander WLG25-122 Airport', 122),
('CHC', 'New Zealander CHC25-123 Airport', 123),
('HLM', 'New Zealander HLM25-124 Airport', 124),
('DUD', 'New Zealander DUD25-125 Airport', 125);

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




-- #_ A E R O L I N E A S _#
INSERT INTO AEROLINEAS (nombre) VALUES
('American Airlines'),
('LATAM Airlines'),
('Iberia'),
('Air France'),
('Lufthansa'),
('British Airways'),
('Qantas'),
('Japan Airlines'),
('Emirates'),
('Aeroméxico');




-- #_ M E T O D O S - D E - P A G O _#
INSERT INTO METODOS_PAGO (nombre) VALUES
('Tarjeta'),
('Efectivo');

INSERT INTO TIPOS_TARJETAS (nombre) VALUES
('Visa'),
('MasterCard'),
('American Express');




-- #_ C A T E G O R I A S - D E - A S I E N T O S _#
INSERT INTO CATEGORIAS_ASIENTOS (nombre) VALUES
('Turista'),
('Business'),
('1a Clase'),
('Economica');




-- #_ V U E L O S _#

-------------------------------
-- Vuelos desde San Salvador (SSA)
-------------------------------

-- Round-trip 1: SSA <-> NYC
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SSANYC-1', 'SSA', 'NYC', '2025-07-01 08:00:00', '2025-07-01 12:00:00', 9, 500.00, 150, 'Disponible'),
('SV-NYCSSA-2', 'NYC', 'SSA', '2025-07-02 14:00:00', '2025-07-02 18:00:00', 9, 500.00, 150, 'Disponible');

-- Round-trip 2: SSA <-> LON
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SSALON-1', 'SSA', 'LON', '2025-07-03 09:00:00', '2025-07-03 19:00:00', 9, 600.00, 150, 'Disponible'),
('SV-LONSSA-2', 'LON', 'SSA', '2025-07-04 10:00:00', '2025-07-04 20:00:00', 9, 600.00, 150, 'Disponible');

-- Round-trip 3: SSA <-> MAD
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SSAMAD-1', 'SSA', 'MAD', '2025-07-05 08:00:00', '2025-07-05 12:00:00', 9, 550.00, 150, 'Disponible'),
('SV-MADSSA-2', 'MAD', 'SSA', '2025-07-06 14:00:00', '2025-07-06 18:00:00', 9, 550.00, 150, 'Disponible');

-- Round-trip 4: SSA <-> TOR
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SSATOR-1', 'SSA', 'TOR', '2025-07-07 07:00:00', '2025-07-07 11:00:00', 9, 650.00, 150, 'Disponible'),
('SV-TORSSA-2', 'TOR', 'SSA', '2025-07-08 15:00:00', '2025-07-08 19:00:00', 9, 650.00, 150, 'Disponible');

-------------------------------
-- Vuelos desde Santa Ana (STA)
-------------------------------

-- Round-trip 1: STA <-> SAO
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-STASAO-1', 'STA', 'SAO', '2025-07-09 08:00:00', '2025-07-09 14:00:00', 9, 700.00, 150, 'Disponible'),
('SV-SAOSTA-2', 'SAO', 'STA', '2025-07-10 16:00:00', '2025-07-10 22:00:00', 9, 700.00, 150, 'Disponible');

-- Round-trip 2: STA <-> LIM
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-STALIM-1', 'STA', 'LIM', '2025-07-11 09:00:00', '2025-07-11 13:00:00', 9, 720.00, 150, 'Disponible'),
('SV-LIMSTA-2', 'LIM', 'STA', '2025-07-12 15:00:00', '2025-07-12 19:00:00', 9, 720.00, 150, 'Disponible');

-- Round-trip 3: STA <-> CDM
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-STACDM-1', 'STA', 'CDM', '2025-07-13 07:00:00', '2025-07-13 11:00:00', 9, 680.00, 150, 'Disponible'),
('SV-CDMSTA-2', 'CDM', 'STA', '2025-07-14 12:00:00', '2025-07-14 16:00:00', 9, 680.00, 150, 'Disponible');

-- Round-trip 4: STA <-> BOG
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-STABOG-1', 'STA', 'BOG', '2025-07-15 08:00:00', '2025-07-15 12:00:00', 9, 690.00, 150, 'Disponible'),
('SV-BOGSTA-2', 'BOG', 'STA', '2025-07-16 14:00:00', '2025-07-16 18:00:00', 9, 690.00, 150, 'Disponible');

-------------------------------
-- Vuelos desde San Miguel (SMI)
-------------------------------

-- Round-trip 1: SMI <-> SAN
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SMISAN-1', 'SMI', 'SAN', '2025-07-17 06:00:00', '2025-07-17 10:00:00', 9, 800.00, 150, 'Disponible'),
('SV-SANSMI-2', 'SAN', 'SMI', '2025-07-18 11:00:00', '2025-07-18 15:00:00', 9, 800.00, 150, 'Disponible');

-- Round-trip 2: SMI <-> TYO
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SMITYO-1', 'SMI', 'TYO', '2025-07-19 07:00:00', '2025-07-19 17:00:00', 9, 1000.00, 150, 'Disponible'),
('SV-TYOSMI-2', 'TYO', 'SMI', '2025-07-20 18:00:00', '2025-07-21 02:00:00', 9, 1000.00, 150, 'Disponible');

-- Round-trip 3: SMI <-> BER
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SMIBER-1', 'SMI', 'BER', '2025-07-21 08:00:00', '2025-07-21 16:00:00', 9, 950.00, 150, 'Disponible'),
('SV-BERSMI-2', 'BER', 'SMI', '2025-07-22 10:00:00', '2025-07-22 18:00:00', 9, 950.00, 150, 'Disponible');

-- Round-trip 4: SMI <-> PAR
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SMIPAR-1', 'SMI', 'PAR', '2025-07-23 09:00:00', '2025-07-23 13:00:00', 9, 980.00, 150, 'Disponible'),
('SV-PARSMI-2', 'PAR', 'SMI', '2025-07-24 14:00:00', '2025-07-24 18:00:00', 9, 980.00, 150, 'Disponible');

-------------------------------
-- Vuelos desde Soyapango (SOY)
-------------------------------

-- Round-trip 1: SOY <-> VAN
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SOYVAN-1', 'SOY', 'VAN', '2025-07-25 08:00:00', '2025-07-25 14:00:00', 9, 1100.00, 150, 'Disponible'),
('SV-VANSOY-2', 'VAN', 'SOY', '2025-07-26 15:00:00', '2025-07-26 21:00:00', 9, 1100.00, 150, 'Disponible');

-- Round-trip 2: SOY <-> LAX
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SOYLAX-1', 'SOY', 'LAX', '2025-07-27 07:00:00', '2025-07-27 11:00:00', 9, 1050.00, 150, 'Disponible'),
('SV-LAXSOY-2', 'LAX', 'SOY', '2025-07-28 12:00:00', '2025-07-28 16:00:00', 9, 1050.00, 150, 'Disponible');

-- Round-trip 3: SOY <-> AKL
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SOYAKL-1', 'SOY', 'AKL', '2025-07-29 09:00:00', '2025-07-29 17:00:00', 9, 1200.00, 150, 'Disponible'),
('SV-AKLSOY-2', 'AKL', 'SOY', '2025-07-30 18:00:00', '2025-07-31 02:00:00', 9, 1200.00, 150, 'Disponible');

-- Round-trip 4: SOY <-> MEL
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-SOYMEL-1', 'SOY', 'MEL', '2025-07-31 08:00:00', '2025-07-31 16:00:00', 9, 1180.00, 150, 'Disponible'),
('SV-MELSOY-2', 'MEL', 'SOY', '2025-08-01 10:00:00', '2025-08-01 18:00:00', 9, 1180.00, 150, 'Disponible');

-------------------------------
-- Vuelos desde Mejicanos (MEJ)
-------------------------------

-- Round-trip 1: MEJ <-> GUA
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-MEJGUA-1', 'MEJ', 'GUA', '2025-08-02 07:00:00', '2025-08-02 09:00:00', 9, 400.00, 150, 'Disponible'),
('SV-GUAMEJ-2', 'GUA', 'MEJ', '2025-08-02 15:00:00', '2025-08-02 17:00:00', 9, 400.00, 150, 'Disponible');

-- Round-trip 2: MEJ <-> CDP
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-MEJCDP-1', 'MEJ', 'CDP', '2025-08-03 08:00:00', '2025-08-03 10:00:00', 9, 420.00, 150, 'Disponible'),
('SV-CDPMEJ-2', 'CDP', 'MEJ', '2025-08-03 14:00:00', '2025-08-03 16:00:00', 9, 420.00, 150, 'Disponible');

-- Round-trip 3: MEJ <-> SJO
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-MEJSJO-1', 'MEJ', 'SJO', '2025-08-04 09:00:00', '2025-08-04 11:00:00', 9, 440.00, 150, 'Disponible'),
('SV-SJOMEJ-2', 'SJO', 'MEJ', '2025-08-04 13:00:00', '2025-08-04 15:00:00', 9, 440.00, 150, 'Disponible');

-- Round-trip 4: MEJ <-> RIO
INSERT INTO VUELOS (codigo_vuelo, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada, idAerolinea, precio_base, capacidad, estado)
VALUES
('SV-MEJRIO-1', 'MEJ', 'RIO', '2025-08-05 07:00:00', '2025-08-05 13:00:00', 9, 500.00, 150, 'Disponible'),
('SV-RIOMEJ-2', 'RIO', 'MEJ', '2025-08-05 14:00:00', '2025-08-05 20:00:00', 9, 500.00, 150, 'Disponible');
