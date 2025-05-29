-- Crear base de datos 
CREATE DATABASE GestionBoletos
GO

USE GestionBoletos
GO

-- Tabla de Roles
CREATE TABLE ROLES (
    idRol INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL CHECK (nombre IN ('Cliente', 'Agente', 'Administrador'))
);

-- Tabla de Usuarios
CREATE TABLE USUARIOS (
    idUsuario INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(100) NOT NULL UNIQUE,
    telefono VARCHAR(15),
    direccion VARCHAR(200),
    documento_identidad VARCHAR(20) NOT NULL UNIQUE,
    contrasena VARCHAR(100) NOT NULL,
    idRol INT NOT NULL,
    fecha_registro DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (idRol) REFERENCES ROLES(idRol)
);

-- Tabla de Continentes
CREATE TABLE CONTINENTES (
    idContinente INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL
);

-- Tabla de Pa�ses
CREATE TABLE PAISES (
    idPais INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    idContinente INT NOT NULL,
    FOREIGN KEY (idContinente) REFERENCES CONTINENTES(idContinente)
);

-- Tabla de Ciudades
CREATE TABLE CIUDADES (
    idCiudad INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    idPais INT NOT NULL,
    FOREIGN KEY (idPais) REFERENCES PAISES(idPais),
    CONSTRAINT UQ_CIUDADES UNIQUE (nombre, idPais)
);

-- Tabla de Aeropuertos
CREATE TABLE AEROPUERTOS (
    idAeropuerto INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    idCiudad INT NOT NULL,
    FOREIGN KEY (idCiudad) REFERENCES CIUDADES(idCiudad)
);

-- Tabla de Aerol�neas
CREATE TABLE AEROLINEAS (
    idAerolinea INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL
);

-- Tabla de Categor�as de Vuelo
CREATE TABLE CATEGORIAS_VUELOS (
    idCategoriaVuelo INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL UNIQUE
);


-- 🔹 Crear la tabla de Categorías de Asientos con precio
CREATE TABLE CATEGORIAS_ASIENTOS (
    idCategoria INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL CHECK (nombre IN ('Business', 'Turista', 'Primera Clase')),
    precio DECIMAL(10,2) NOT NULL DEFAULT 0.00
);

-- Tabla de Vuelos
CREATE TABLE VUELOS (
    idVuelo INT PRIMARY KEY IDENTITY(1,1),
    codigo_vuelo VARCHAR(20) NOT NULL UNIQUE,
    idAeropuertoOrigen INT NOT NULL,
    idAeropuertoDestino INT NOT NULL,
    fecha_salida DATETIME2 NOT NULL,
    fecha_llegada DATETIME2 NOT NULL,
    idAerolinea INT NOT NULL,
    idCategoriaVuelo INT NULL,
    precio_base DECIMAL(10,2) NOT NULL,
    cantidad_asientos INT NOT NULL,
    asientos_disponibles INT DEFAULT 0,
    estado VARCHAR(20) NOT NULL CHECK (estado IN ('Disponible', 'Lleno', 'Cancelado')),
    FOREIGN KEY (idAeropuertoOrigen) REFERENCES AEROPUERTOS(idAeropuerto),
    FOREIGN KEY (idAeropuertoDestino) REFERENCES AEROPUERTOS(idAeropuerto),
    FOREIGN KEY (idAerolinea) REFERENCES AEROLINEAS(idAerolinea),
    FOREIGN KEY (idCategoriaVuelo) REFERENCES CATEGORIAS_VUELOS(idCategoriaVuelo)
);

-- Tabla de Asientos por Vuelo
CREATE TABLE VUELOS_ASIENTOS (
    idVueloAsiento INT PRIMARY KEY IDENTITY(1,1),
    idVuelo INT NOT NULL,
    numero VARCHAR(10) NOT NULL,
    idCategoria INT NOT NULL,
    estado VARCHAR(20) NOT NULL CHECK (estado IN ('Disponible', 'Reservado')),
    FOREIGN KEY (idVuelo) REFERENCES VUELOS(idVuelo),
    FOREIGN KEY (idCategoria) REFERENCES CATEGORIAS_ASIENTOS(idCategoria),
    CONSTRAINT UQ_VUELOS_ASIENTOS UNIQUE (idVuelo, numero)
);


-- Tabla de Boletos
CREATE TABLE BOLETOS (
    idBoleto INT PRIMARY KEY IDENTITY(1,1),
    idUsuario INT NOT NULL,
    fecha_emision DATETIME2 DEFAULT GETDATE(),
    precio_total DECIMAL(10,2) NOT NULL,
    estado VARCHAR(20) NOT NULL CHECK (estado IN ('Activo', 'Cancelado', 'Usado')),
    FOREIGN KEY (idUsuario) REFERENCES USUARIOS(idUsuario)
);

-- Tabla de Detalle de Boletos
CREATE TABLE BOLETOS_DETALLE (
    idBoleto INT NOT NULL,
    idVuelo INT NOT NULL,
    idVueloAsiento INT,
    PRIMARY KEY (idBoleto, idVuelo),
    FOREIGN KEY (idBoleto) REFERENCES BOLETOS(idBoleto),
    FOREIGN KEY (idVuelo) REFERENCES VUELOS(idVuelo),
    FOREIGN KEY (idVueloAsiento) REFERENCES VUELOS_ASIENTOS(idVueloAsiento)
);

-- Tabla de M�todos de Pago
CREATE TABLE METODOS_PAGO (
    idMetodoPago INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL CHECK (nombre IN ('Tarjeta', 'Efectivo'))
);

CREATE TABLE TIPOS_TARJETAS (
    idTipoTarjeta INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL UNIQUE  -- Ejemplos: 'Visa', 'MasterCard', 'American Express'
);

-- Tabla de Pagos
CREATE TABLE PAGOS (
    idPago INT PRIMARY KEY IDENTITY(1,1),
    idBoleto INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    idMetodoPago INT NOT NULL,
    fecha_pago DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (idBoleto) REFERENCES BOLETOS(idBoleto),
    FOREIGN KEY (idMetodoPago) REFERENCES METODOS_PAGO(idMetodoPago)
);

-- Tabla de Equipajes
CREATE TABLE EQUIPAJES (
    idEquipaje INT PRIMARY KEY IDENTITY(1,1),
    idBoleto INT NOT NULL,
    tipo VARCHAR(20) NOT NULL CHECK (tipo IN ('Mano', 'Facturado')),
    precio DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (idBoleto) REFERENCES BOLETOS(idBoleto)
);

-- Tabla de Historial de Boletos
CREATE TABLE HISTORIAL_BOLETOS (
    idHistorial INT PRIMARY KEY IDENTITY(1,1),
    idBoleto INT NOT NULL,
    accion VARCHAR(20) NOT NULL CHECK (accion IN ('Modificaci�n', 'Cancelaci�n')),
    fecha DATETIME2 DEFAULT GETDATE(),
    descripcion TEXT,
    FOREIGN KEY (idBoleto) REFERENCES BOLETOS(idBoleto)
);

-- Tabla de Bit�cora (Auditor�a general)
CREATE TABLE BITACORA (
    idBitacora INT PRIMARY KEY IDENTITY(1,1),
    nombreTabla VARCHAR(50) NOT NULL,
    idRegistro INT NULL,
    accion VARCHAR(20) NOT NULL CHECK (accion IN ('INSERT', 'UPDATE', 'DELETE')),
    idUsuario INT NULL,
    fecha DATETIME2 DEFAULT GETDATE(),
    descripcion NVARCHAR(MAX)
);

-- 🔹 Crear la tabla de Reservas con el campo total
CREATE TABLE RESERVAS (
    idReserva INT PRIMARY KEY IDENTITY(1,1),
    numeroReserva VARCHAR(50) UNIQUE NOT NULL,
    idUsuario INT NOT NULL,
    idVuelo INT NOT NULL,
    fechaReserva DATETIME DEFAULT GETDATE(),
    total DECIMAL(10,2) NOT NULL DEFAULT 0.00,
	estado VARCHAR(20) DEFAULT 'Activo';
    FOREIGN KEY (idUsuario) REFERENCES USUARIOS(idUsuario),
    FOREIGN KEY (idVuelo) REFERENCES VUELOS(idVuelo)
);


-- 🔹 Crear la tabla de Relación entre Reservas y Asientos
CREATE TABLE RESERVA_ASIENTOS (
    idReservaAsiento INT PRIMARY KEY IDENTITY(1,1),
    idReserva INT NOT NULL,
    idVueloAsiento INT NOT NULL,
    FOREIGN KEY (idReserva) REFERENCES RESERVAS(idReserva) ON DELETE CASCADE,
    FOREIGN KEY (idVueloAsiento) REFERENCES VUELOS_ASIENTOS(idVueloAsiento) ON DELETE CASCADE
);