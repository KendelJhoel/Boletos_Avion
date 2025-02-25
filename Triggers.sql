-- Trigger para actualizar el número de asientos disponibles automáticamente
CREATE TRIGGER ActualizarAsientosDisponibles
ON VUELOS_ASIENTOS
AFTER INSERT, UPDATE
AS
BEGIN
    DECLARE @idVuelo INT
    SELECT @idVuelo = INSERTED.idVuelo FROM INSERTED

    DECLARE @disponibles INT

    -- Contar los asientos disponibles
    SELECT 
        @disponibles = COUNT(*)
    FROM VUELOS_ASIENTOS
    WHERE idVuelo = @idVuelo AND estado = 'Disponible'

    -- Actualizar el número de asientos disponibles en la tabla VUELOS
    UPDATE VUELOS
    SET asientos_disponibles = @disponibles
    WHERE idVuelo = @idVuelo
END
