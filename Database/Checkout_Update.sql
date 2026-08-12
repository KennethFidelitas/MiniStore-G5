USE [Ministore_DB]
GO

IF COL_LENGTH('dbo.tbEventoCalendario', 'TipoEvento') IS NULL
BEGIN
    ALTER TABLE dbo.tbEventoCalendario
    ADD TipoEvento varchar(30) NOT NULL
        CONSTRAINT DF_tbEventoCalendario_TipoEvento DEFAULT 'Personal'
END
GO

IF COL_LENGTH('dbo.tbEventoCalendario', 'ConsecutivoPedido') IS NULL
BEGIN
    ALTER TABLE dbo.tbEventoCalendario
    ADD ConsecutivoPedido int NULL
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_tbEventoCalendario_tbPedido'
)
BEGIN
    ALTER TABLE dbo.tbEventoCalendario WITH CHECK
    ADD CONSTRAINT FK_tbEventoCalendario_tbPedido
        FOREIGN KEY(ConsecutivoPedido) REFERENCES dbo.tbPedido(Consecutivo)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spConsultarEventosCalendario]
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON

    SELECT E.Consecutivo, E.Titulo, E.Descripcion, E.FechaInicio, E.FechaFin,
           E.TipoEvento, E.ConsecutivoPedido, P.Estado AS EstadoPedido
    FROM dbo.tbEventoCalendario E
    LEFT JOIN dbo.tbPedido P ON P.Consecutivo = E.ConsecutivoPedido
    WHERE E.ConsecutivoUsuario = @ConsecutivoUsuario
    ORDER BY E.FechaInicio
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spGuardarEventoCalendario]
    @Consecutivo        int,
    @Titulo             varchar(150),
    @Descripcion        varchar(500) = NULL,
    @FechaInicio        datetime,
    @FechaFin           datetime,
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON

    IF @Consecutivo = 0
    BEGIN
        INSERT INTO dbo.tbEventoCalendario
            (Titulo, Descripcion, FechaInicio, FechaFin, ConsecutivoUsuario, TipoEvento)
        VALUES
            (@Titulo, @Descripcion, @FechaInicio, @FechaFin, @ConsecutivoUsuario, 'Personal')

        SELECT CONVERT(int, SCOPE_IDENTITY())
        RETURN
    END

    UPDATE dbo.tbEventoCalendario
    SET Titulo = @Titulo,
        Descripcion = @Descripcion,
        FechaInicio = @FechaInicio,
        FechaFin = @FechaFin
    WHERE Consecutivo = @Consecutivo
      AND ConsecutivoUsuario = @ConsecutivoUsuario
      AND TipoEvento = 'Personal'

    SELECT CASE WHEN @@ROWCOUNT > 0 THEN @Consecutivo ELSE 0 END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spEliminarEventoCalendario]
    @Consecutivo        int,
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON

    DELETE FROM dbo.tbEventoCalendario
    WHERE Consecutivo = @Consecutivo
      AND ConsecutivoUsuario = @ConsecutivoUsuario
      AND TipoEvento = 'Personal'

    SELECT @@ROWCOUNT
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spCrearPedidoDesdeCarrito]
    @ConsecutivoCarrito int,
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON
    SET XACT_ABORT ON

    BEGIN TRY
        BEGIN TRANSACTION

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.tbCarrito WITH (UPDLOCK, HOLDLOCK)
            WHERE Consecutivo = @ConsecutivoCarrito
              AND ConsecutivoUsuario = @ConsecutivoUsuario
        )
        BEGIN
            THROW 50001, 'El carrito no pertenece al usuario autenticado', 1;
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.tbDetalleCarrito
            WHERE ConsecutivoCarrito = @ConsecutivoCarrito
        )
        BEGIN
            THROW 50002, 'El carrito estÃ¡ vacÃ­o', 1;
        END

        IF EXISTS
        (
            SELECT 1
            FROM dbo.tbDetalleCarrito DC
            INNER JOIN dbo.tbProducto P WITH (UPDLOCK, HOLDLOCK)
                ON P.Consecutivo = DC.ConsecutivoProducto
            WHERE DC.ConsecutivoCarrito = @ConsecutivoCarrito
              AND (P.Estado = 0 OR P.Stock < DC.Cantidad)
        )
        BEGIN
            THROW 50003, 'Uno o mÃ¡s productos ya no tienen stock suficiente', 1;
        END

        DECLARE @Total decimal(10,2)
        DECLARE @ConsecutivoPedido int
        DECLARE @NombreCliente varchar(250)
        DECLARE @Productos varchar(500)
        DECLARE @FechaEntrega date = CONVERT(date, GETDATE())
        DECLARE @DiasHabiles int = 0

        SELECT @Total = SUM(Subtotal)
        FROM dbo.tbDetalleCarrito
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito

        SELECT @NombreCliente = Nombre
        FROM dbo.tbUsuario
        WHERE Consecutivo = @ConsecutivoUsuario

        INSERT INTO dbo.tbPedido(ConsecutivoUsuario, FechaPedido, Total, Estado)
        VALUES (@ConsecutivoUsuario, GETDATE(), @Total, 'Pendiente')

        SET @ConsecutivoPedido = CONVERT(int, SCOPE_IDENTITY())

        INSERT INTO dbo.tbDetallePedido
            (ConsecutivoPedido, ConsecutivoProducto, Cantidad, PrecioUnitario, Subtotal)
        SELECT @ConsecutivoPedido, ConsecutivoProducto, Cantidad, PrecioUnitario, Subtotal
        FROM dbo.tbDetalleCarrito
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito

        SELECT @Productos = LEFT(
            STRING_AGG(
                CONVERT(varchar(max), CONCAT(P.Nombre, ' x', DP.Cantidad)),
                ', '),
            500)
        FROM dbo.tbDetallePedido DP
        INNER JOIN dbo.tbProducto P ON P.Consecutivo = DP.ConsecutivoProducto
        WHERE DP.ConsecutivoPedido = @ConsecutivoPedido

        WHILE @DiasHabiles < 7
        BEGIN
            SET @FechaEntrega = DATEADD(DAY, 1, @FechaEntrega)

            IF ((DATEDIFF(DAY, '19000101', @FechaEntrega) % 7) + 7) % 7 NOT IN (5, 6)
                SET @DiasHabiles = @DiasHabiles + 1
        END

        DECLARE @FechaInicioEntrega datetime =
            DATEADD(HOUR, 9, CONVERT(datetime, @FechaEntrega))
        DECLARE @FechaFinEntrega datetime =
            DATEADD(HOUR, 18, CONVERT(datetime, @FechaEntrega))

        INSERT INTO dbo.tbEventoCalendario
            (Titulo, Descripcion, FechaInicio, FechaFin, ConsecutivoUsuario,
             TipoEvento, ConsecutivoPedido)
        VALUES
            (LEFT(CONCAT('Pedido #', @ConsecutivoPedido, ' - Entrega estimada'), 150),
             CONCAT('Productos: ', @Productos),
             @FechaInicioEntrega, @FechaFinEntrega, @ConsecutivoUsuario,
             'EntregaCliente', @ConsecutivoPedido)

        INSERT INTO dbo.tbEventoCalendario
            (Titulo, Descripcion, FechaInicio, FechaFin, ConsecutivoUsuario,
             TipoEvento, ConsecutivoPedido)
        SELECT
            LEFT(CONCAT('Entregar pedido #', @ConsecutivoPedido, ' a ', @NombreCliente), 150),
            CONCAT('Productos: ', @Productos),
            @FechaInicioEntrega,
            @FechaFinEntrega,
            U.Consecutivo,
            'EntregaAdmin',
            @ConsecutivoPedido
        FROM dbo.tbUsuario U
        INNER JOIN dbo.tbRol R ON R.Consecutivo = U.ConsecutivoRol
        WHERE U.Estado = 1
          AND R.Nombre = 'Administrador'

        UPDATE P
        SET P.Stock = P.Stock - DC.Cantidad
        FROM dbo.tbProducto P
        INNER JOIN dbo.tbDetalleCarrito DC
            ON P.Consecutivo = DC.ConsecutivoProducto
        WHERE DC.ConsecutivoCarrito = @ConsecutivoCarrito

        DELETE FROM dbo.tbDetalleCarrito
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito

        COMMIT TRANSACTION

        SELECT @ConsecutivoPedido AS ConsecutivoPedido,
               CONVERT(datetime, @FechaEntrega) AS FechaEntregaEstimada
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION

        THROW
    END CATCH
END
GO
