-- #_ Z O N A S _#

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

-- Insertar los 3 usuarios administradores
INSERT INTO USUARIOS (nombre, correo, telefono, direccion, documento_identidad, contrasena, idRol)
VALUES
('a_ken', 'Kendel.Arevalo@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN001', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador')),
('a_mig', 'Miguel.Leiva@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN002', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador')),
('a_jon', 'Jonathan.Barrientos@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN003', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador')),
('a_raf', 'rafael.sandoval@catolica.edu.sv', '1234-5678', 'Dirección default', 'ADMIN004', 'admin123', (SELECT idRol FROM ROLES WHERE nombre = 'Administrador'));




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




-- #_ A E R O P U E R T O S _#

-- Argentina (idPais = 1)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Argentine BUENOS AIRES 1-1 Airport', 1),
('Argentine CORDOBA 1-2 Airport', 2),
('Argentine ROSARIO 1-3 Airport', 3),
('Argentine MENDOZA 1-4 Airport', 4),
('Argentine LA PLATA 1-5 Airport', 5);

-- Belize (idPais = 2)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Belizean BELICE CITY 2-6 Airport', 6),
('Belizean BELMOPAN 2-7 Airport', 7),
('Belizean SAN IGNACIO 2-8 Airport', 8),
('Belizean ORANGE WALK 2-9 Airport', 9),
('Belizean COROZAL 2-10 Airport', 10);

-- Brasil (idPais = 3)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Brazilian SAO PAULO 3-11 Airport', 11),
('Brazilian RIO DE JANEIRO 3-12 Airport', 12),
('Brazilian BRASILIA 3-13 Airport', 13),
('Brazilian SALVADOR 3-14 Airport', 14),
('Brazilian FORTALEZA 3-15 Airport', 15);

-- Canadá (idPais = 4)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Canadian TORONTO 4-16 Airport', 16),
('Canadian MONTREAL 4-17 Airport', 17),
('Canadian VANCOUVER 4-18 Airport', 18),
('Canadian CALGARY 4-19 Airport', 19),
('Canadian OTTAWA 4-20 Airport', 20);

-- Chile (idPais = 5)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Chilean SANTIAGO 5-21 Airport', 21),
('Chilean VALPARAISO 5-22 Airport', 22),
('Chilean CONCEPCION 5-23 Airport', 23),
('Chilean LA SERENA 5-24 Airport', 24),
('Chilean TEMUCO 5-25 Airport', 25);

-- Colombia (idPais = 6)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Colombian BOGOTA 6-26 Airport', 26),
('Colombian MEDELLIN 6-27 Airport', 27),
('Colombian CALI 6-28 Airport', 28),
('Colombian BARRANQUILLA 6-29 Airport', 29),
('Colombian CARTAGENA 6-30 Airport', 30);

-- Costa Rica (idPais = 7)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Costa Rican SAN JOSE 7-31 Airport', 31),
('Costa Rican ALAJUELA 7-32 Airport', 32),
('Costa Rican HEREDIA 7-33 Airport', 33),
('Costa Rican CARTAGO 7-34 Airport', 34),
('Costa Rican LIBERIA 7-35 Airport', 35);

-- El Salvador (idPais = 8)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Salvadoran SAN SALVADOR 8-36 Airport', 36),
('Salvadoran SANTA ANA 8-37 Airport', 37),
('Salvadoran SAN MIGUEL 8-38 Airport', 38),
('Salvadoran SOYAPANGO 8-39 Airport', 39),
('Salvadoran MEJICANOS 8-40 Airport', 40);

-- Estados Unidos (idPais = 9)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('American NEW YORK 9-41 Airport', 41),
('American LOS ANGELES 9-42 Airport', 42),
('American CHICAGO 9-43 Airport', 43),
('American HOUSTON 9-44 Airport', 44),
('American PHOENIX 9-45 Airport', 45);

-- Guatemala (idPais = 10)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Guatemalan CIUDAD DE GUATEMALA 10-46 Airport', 46),
('Guatemalan QUETZALTENANGO 10-47 Airport', 47),
('Guatemalan ESCUINTLA 10-48 Airport', 48),
('Guatemalan MAZATENANGO 10-49 Airport', 49),
('Guatemalan COBAN 10-50 Airport', 50);

-- Honduras (idPais = 11)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Honduran TEGUCIGALPA 11-51 Airport', 51),
('Honduran SAN PEDRO SULA 11-52 Airport', 52),
('Honduran LA CEIBA 11-53 Airport', 53),
('Honduran CHOLUTECA 11-54 Airport', 54),
('Honduran EL PROGRESO 11-55 Airport', 55);

-- México (idPais = 12)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Mexican CIUDAD DE MEXICO 12-56 Airport', 56),
('Mexican GUADALAJARA 12-57 Airport', 57),
('Mexican MONTERREY 12-58 Airport', 58),
('Mexican PUEBLA 12-59 Airport', 59),
('Mexican TIJUANA 12-60 Airport', 60);

-- Panamá (idPais = 13)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Panamanian CIUDAD DE PANAMA 13-61 Airport', 61),
('Panamanian SAN MIGUELITO 13-62 Airport', 62),
('Panamanian TOCUMEN 13-63 Airport', 63),
('Panamanian COLON 13-64 Airport', 64),
('Panamanian DAVID 13-65 Airport', 65);

-- Perú (idPais = 14)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Peruvian LIMA 14-66 Airport', 66),
('Peruvian AREQUIPA 14-67 Airport', 67),
('Peruvian TRUJILLO 14-68 Airport', 68),
('Peruvian CHICLAYO 14-69 Airport', 69),
('Peruvian CUSCO 14-70 Airport', 70);

-- Puerto Rico (idPais = 15)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Puerto Rican SAN JUAN 15-71 Airport', 71),
('Puerto Rican PONCE 15-72 Airport', 72),
('Puerto Rican MAYAGUEZ 15-73 Airport', 73),
('Puerto Rican CAGUAS 15-74 Airport', 74),
('Puerto Rican BAYAMON 15-75 Airport', 75);

-- República Dominicana (idPais = 16)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Dominican SANTO DOMINGO 16-76 Airport', 76),
('Dominican SANTIAGO DE LOS CABALLEROS 16-77 Airport', 77),
('Dominican LA ROMANA 16-78 Airport', 78),
('Dominican SAN PEDRO DE MACORIS 16-79 Airport', 79),
('Dominican PUERTO PLATA 16-80 Airport', 80);

-- Japón (idPais = 17)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Japanese TOKYO 17-81 Airport', 81),
('Japanese YOKOHAMA 17-82 Airport', 82),
('Japanese OSAKA 17-83 Airport', 83),
('Japanese NAGOYA 17-84 Airport', 84),
('Japanese SAPPORO 17-85 Airport', 85);

-- Alemania (idPais = 18)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('German BERLIN 18-86 Airport', 86),
('German MUNICH 18-87 Airport', 87),
('German FRANKFURT 18-88 Airport', 88),
('German HAMBURG 18-89 Airport', 89),
('German COLOGNE 18-90 Airport', 90);

-- España (idPais = 19)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Spanish MADRID 19-91 Airport', 91),
('Spanish BARCELONA 19-92 Airport', 92),
('Spanish VALENCIA 19-93 Airport', 93),
('Spanish SEVILLA 19-94 Airport', 94),
('Spanish ZARAGOZA 19-95 Airport', 95);

-- Francia (idPais = 20)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('French PARIS 20-96 Airport', 96),
('French MARSEILLE 20-97 Airport', 97),
('French LYON 20-98 Airport', 98),
('French TOULOUSE 20-99 Airport', 99),
('French NICE 20-100 Airport', 100);

-- Portugal (idPais = 21)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Portuguese LISBON 21-101 Airport', 101),
('Portuguese PORTO 21-102 Airport', 102),
('Portuguese BRAGA 21-103 Airport', 103),
('Portuguese COIMBRA 21-104 Airport', 104),
('Portuguese FARO 21-105 Airport', 105);

-- Reino Unido (idPais = 22)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('British LONDON 22-106 Airport', 106),
('British BIRMINGHAM 22-107 Airport', 107),
('British MANCHESTER 22-108 Airport', 108),
('British GLASGOW 22-109 Airport', 109),
('British LIVERPOOL 22-110 Airport', 110);

-- Suecia (idPais = 23)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Swedish STOCKHOLM 23-111 Airport', 111),
('Swedish GOTHENBURG 23-112 Airport', 112),
('Swedish MALMO 23-113 Airport', 113),
('Swedish UPPSALA 23-114 Airport', 114),
('Swedish VASTERAS 23-115 Airport', 115);

-- Australia (idPais = 24)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('Australian SYDNEY 24-111 Airport', 111),
('Australian MELBOURNE 24-112 Airport', 112),
('Australian BRISBANE 24-113 Airport', 113),
('Australian PERTH 24-114 Airport', 114),
('Australian ADELAIDE 24-115 Airport', 115);

-- Nueva Zelanda (idPais = 25)
INSERT INTO AEROPUERTOS (nombre, idCiudad) VALUES
('New Zealander AUCKLAND 25-116 Airport', 116),
('New Zealander WELLINGTON 25-117 Airport', 117),
('New Zealander CHRISTCHURCH 25-118 Airport', 118),
('New Zealander HAMILTON 25-119 Airport', 119),
('New Zealander DUNEDIN 25-120 Airport', 120);




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



--#_ C A T E G O R I A S - V U E L O S _#
INSERT INTO CATEGORIAS_VUELOS (nombre) VALUES
('Nacional'),
('Internacional'),
('Directo'),
('Con escalas'),
('Chárter'),
('Regular');




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
('Business'),
('Turista'),
('Primera Clase');