USE [Ministore_DB]
GO

/*
    Actualización no destructiva para una base MiniStore ya existente.
    Agrega o actualiza únicamente los procedimientos requeridos por
    el panel de administración.
*/

CREATE OR ALTER PROCEDURE [dbo].[spResumenAdmin]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM dbo.tbProducto) AS TotalProductos,
        (SELECT COUNT(*) FROM dbo.tbProducto WHERE Estado = 1) AS ProductosActivos,
        (SELECT COUNT(*) FROM dbo.tbProducto WHERE Estado = 0) AS ProductosInactivos,
        (SELECT COUNT(*) FROM dbo.tbProducto WHERE Stock = 0) AS ProductosSinStock,
        (SELECT COUNT(*) FROM dbo.tbCategoria WHERE Estado = 1) AS TotalCategorias,
        (SELECT COUNT(*) FROM dbo.tbUsuario) AS TotalUsuarios,
        (SELECT COUNT(*) FROM dbo.tbPedido) AS TotalPedidos,
        (
            SELECT ISNULL(SUM(Total), 0)
            FROM dbo.tbPedido
            WHERE Estado = 'Pagado'
        ) AS VentasTotales;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spListarProductos]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Consecutivo,
           P.Nombre,
           P.Descripcion,
           P.Precio,
           P.Stock,
           P.Imagen,
           P.ConsecutivoCategoria,
           C.NombreCategoria,
           P.Estado
    FROM dbo.tbProducto P
    INNER JOIN dbo.tbCategoria C
        ON P.ConsecutivoCategoria = C.Consecutivo
    ORDER BY P.Nombre;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spGuardarProducto]
    @Consecutivo int = 0,
    @Nombre varchar(150),
    @Descripcion text = NULL,
    @Precio decimal(10,2),
    @Stock int,
    @Imagen varchar(255) = NULL,
    @ConsecutivoCategoria int,
    @Estado bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @Precio <= 0
        THROW 50001, 'El precio debe ser mayor que cero.', 1;

    IF @Stock < 0
        THROW 50002, 'El stock no puede ser negativo.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.tbCategoria
        WHERE Consecutivo = @ConsecutivoCategoria
          AND Estado = 1
    )
        THROW 50003, 'La categoría seleccionada no existe o está inactiva.', 1;

    IF ISNULL(@Consecutivo, 0) = 0
    BEGIN
        INSERT INTO dbo.tbProducto
        (
            Nombre,
            Descripcion,
            Precio,
            Stock,
            Imagen,
            ConsecutivoCategoria,
            Estado
        )
        VALUES
        (
            @Nombre,
            @Descripcion,
            @Precio,
            @Stock,
            @Imagen,
            @ConsecutivoCategoria,
            @Estado
        );

        SET @Consecutivo = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.tbProducto
           SET Nombre = @Nombre,
               Descripcion = @Descripcion,
               Precio = @Precio,
               Stock = @Stock,
               Imagen = @Imagen,
               ConsecutivoCategoria = @ConsecutivoCategoria,
               Estado = @Estado
         WHERE Consecutivo = @Consecutivo;

        IF @@ROWCOUNT = 0
            THROW 50004, 'El producto que intentó actualizar no existe.', 1;
    END

    SELECT @Consecutivo AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spCambiarEstadoProducto]
    @Consecutivo int,
    @Estado bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbProducto
       SET Estado = @Estado
     WHERE Consecutivo = @Consecutivo;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO
