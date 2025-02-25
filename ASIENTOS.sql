CREATE OR ALTER PROCEDURE GenerarAsientosPorVuelo
AS
BEGIN
    DECLARE @idVuelo INT

    -- Cursor para recorrer todos los vuelos sin asientos registrados
    DECLARE vuelo_cursor CURSOR FOR
    SELECT idVuelo FROM VUELOS
    WHERE idVuelo NOT IN (SELECT DISTINCT idVuelo FROM VUELOS_ASIENTOS);

    OPEN vuelo_cursor
    FETCH NEXT FROM vuelo_cursor INTO @idVuelo

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @AsientoNum INT = 1
        DECLARE @Fila INT
        DECLARE @Letra CHAR(1)
        DECLARE @idCategoria INT
        DECLARE @iAsiento INT = 1

        -- Business (10% = 10 asientos)
        SET @idCategoria = 1
        WHILE @iAsiento <= 10
        BEGIN
            SET @Fila = CEILING(@iAsiento / 4.0)
            SET @Letra = CHAR(65 + ((@iAsiento - 1) % 4))  -- A, B, C, D
            INSERT INTO VUELOS_ASIENTOS (idVuelo, numero, idCategoria, estado)
            VALUES (@idVuelo, CONCAT(@Fila, @Letra), @idCategoria, 'Disponible')
            SET @iAsiento += 1
        END

        -- Turista (70% = 70 asientos)
        SET @idCategoria = 2
        SET @iAsiento = 1
        WHILE @iAsiento <= 70
        BEGIN
            SET @Fila = CEILING((@iAsiento + 10) / 4.0)
            SET @Letra = CHAR(65 + ((@iAsiento - 1) % 4))  -- A, B, C, D
            INSERT INTO VUELOS_ASIENTOS (idVuelo, numero, idCategoria, estado)
            VALUES (@idVuelo, CONCAT(@Fila, @Letra), @idCategoria, 'Disponible')
            SET @iAsiento += 1
        END

        -- Primera Clase (20% = 20 asientos)
        SET @idCategoria = 3
        SET @iAsiento = 1
        WHILE @iAsiento <= 20
        BEGIN
            SET @Fila = CEILING((@iAsiento + 80) / 4.0)
            SET @Letra = CHAR(65 + ((@iAsiento - 1) % 4))  -- A, B, C, D
            INSERT INTO VUELOS_ASIENTOS (idVuelo, numero, idCategoria, estado)
            VALUES (@idVuelo, CONCAT(@Fila, @Letra), @idCategoria, 'Disponible')
            SET @iAsiento += 1
        END

        FETCH NEXT FROM vuelo_cursor INTO @idVuelo
    END

    CLOSE vuelo_cursor
    DEALLOCATE vuelo_cursor
END;

-- EJECUTAR ESTO PARA GENERAR LOS ASIENTOS
EXEC GenerarAsientosPorVuelo;

-- VER TOTAL DE ASIENTOS
SELECT COUNT(*) AS TotalAsientos FROM VUELOS_ASIENTOS;
