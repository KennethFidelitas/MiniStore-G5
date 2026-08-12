USE [Ministore_DB]
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================
   ESTRUCTURAS ADICIONALES
   ============================================================ */

IF OBJECT_ID('dbo.tbHistorialEstadoPedido', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbHistorialEstadoPedido
    (
        Consecutivo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbHistorialEstadoPedido PRIMARY KEY,
        ConsecutivoPedido int NOT NULL,
        EstadoAnterior varchar(30) NOT NULL,
        EstadoNuevo varchar(30) NOT NULL,
        FechaCambio datetime NOT NULL CONSTRAINT DF_tbHistorialEstadoPedido_Fecha DEFAULT GETDATE(),
        ConsecutivoAdministrador int NULL,
        CONSTRAINT FK_tbHistorialEstadoPedido_Pedido
            FOREIGN KEY (ConsecutivoPedido) REFERENCES dbo.tbPedido(Consecutivo),
        CONSTRAINT FK_tbHistorialEstadoPedido_Administrador
            FOREIGN KEY (ConsecutivoAdministrador) REFERENCES dbo.tbUsuario(Consecutivo)
    );
END
GO

IF OBJECT_ID('dbo.tbPublicacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbPublicacion
    (
        Consecutivo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbPublicacion PRIMARY KEY,
        Titulo varchar(180) NOT NULL,
        Resumen varchar(500) NOT NULL,
        Contenido varchar(max) NOT NULL,
        Autor varchar(150) NOT NULL,
        Imagen varchar(255) NULL,
        FechaPublicacion datetime NOT NULL CONSTRAINT DF_tbPublicacion_Fecha DEFAULT GETDATE(),
        Estado bit NOT NULL CONSTRAINT DF_tbPublicacion_Estado DEFAULT 1
    );
END
GO

IF OBJECT_ID('dbo.tbComentarioPublicacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbComentarioPublicacion
    (
        Consecutivo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbComentarioPublicacion PRIMARY KEY,
        ConsecutivoPublicacion int NOT NULL,
        ConsecutivoUsuario int NULL,
        NombreAutor varchar(150) NOT NULL,
        Contenido varchar(1000) NOT NULL,
        FechaComentario datetime NOT NULL CONSTRAINT DF_tbComentarioPublicacion_Fecha DEFAULT GETDATE(),
        Estado bit NOT NULL CONSTRAINT DF_tbComentarioPublicacion_Estado DEFAULT 1,
        CONSTRAINT FK_tbComentarioPublicacion_Publicacion
            FOREIGN KEY (ConsecutivoPublicacion) REFERENCES dbo.tbPublicacion(Consecutivo),
        CONSTRAINT FK_tbComentarioPublicacion_Usuario
            FOREIGN KEY (ConsecutivoUsuario) REFERENCES dbo.tbUsuario(Consecutivo)
    );
END
GO

IF COL_LENGTH('dbo.tbComentarioPublicacion', 'ConsecutivoComentarioPadre') IS NULL
BEGIN
    ALTER TABLE dbo.tbComentarioPublicacion ADD ConsecutivoComentarioPadre int NULL;
    ALTER TABLE dbo.tbComentarioPublicacion ADD CONSTRAINT FK_tbComentarioPublicacion_Padre
        FOREIGN KEY (ConsecutivoComentarioPadre) REFERENCES dbo.tbComentarioPublicacion(Consecutivo);
END
GO

IF OBJECT_ID('dbo.tbConsultaContacto', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbConsultaContacto
    (
        Consecutivo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbConsultaContacto PRIMARY KEY,
        Nombre varchar(150) NOT NULL,
        CorreoElectronico varchar(150) NOT NULL,
        Asunto varchar(180) NOT NULL,
        Mensaje varchar(2000) NOT NULL,
        FechaRegistro datetime NOT NULL CONSTRAINT DF_tbConsultaContacto_Fecha DEFAULT GETDATE(),
        Estado varchar(20) NOT NULL CONSTRAINT DF_tbConsultaContacto_Estado DEFAULT 'Nueva'
    );
END
GO

IF OBJECT_ID('dbo.tbPromocion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbPromocion
    (
        Consecutivo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbPromocion PRIMARY KEY,
        Nombre varchar(150) NOT NULL,
        Porcentaje decimal(5,2) NOT NULL,
        ConsecutivoProducto int NULL,
        ConsecutivoCategoria int NULL,
        FechaInicio datetime NOT NULL,
        FechaFin datetime NOT NULL,
        Estado bit NOT NULL CONSTRAINT DF_tbPromocion_Estado DEFAULT 1,
        CONSTRAINT FK_tbPromocion_Producto
            FOREIGN KEY (ConsecutivoProducto) REFERENCES dbo.tbProducto(Consecutivo),
        CONSTRAINT FK_tbPromocion_Categoria
            FOREIGN KEY (ConsecutivoCategoria) REFERENCES dbo.tbCategoria(Consecutivo),
        CONSTRAINT CK_tbPromocion_Porcentaje CHECK (Porcentaje > 0 AND Porcentaje <= 90),
        CONSTRAINT CK_tbPromocion_Fechas CHECK (FechaFin >= FechaInicio),
        CONSTRAINT CK_tbPromocion_Objetivo CHECK
        (
            (ConsecutivoProducto IS NOT NULL AND ConsecutivoCategoria IS NULL)
            OR
            (ConsecutivoProducto IS NULL AND ConsecutivoCategoria IS NOT NULL)
        )
    );
END
GO

/* ============================================================
   CATÁLOGO, BÚSQUEDA Y PROMOCIONES
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spListarProductos
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
           P.Estado,
           CONVERT(decimal(5,2), ISNULL(D.PorcentajeDescuento, 0)) AS PorcentajeDescuento,
           CONVERT(decimal(10,2), P.Precio * (1 - ISNULL(D.PorcentajeDescuento, 0) / 100.0)) AS PrecioFinal
      FROM dbo.tbProducto P
      INNER JOIN dbo.tbCategoria C
              ON C.Consecutivo = P.ConsecutivoCategoria
      OUTER APPLY
      (
          SELECT MAX(PR.Porcentaje) AS PorcentajeDescuento
            FROM dbo.tbPromocion PR
           WHERE PR.Estado = 1
             AND GETDATE() BETWEEN PR.FechaInicio AND PR.FechaFin
             AND
             (
                 PR.ConsecutivoProducto = P.Consecutivo
                 OR PR.ConsecutivoCategoria = P.ConsecutivoCategoria
             )
      ) D
     ORDER BY P.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.spObtenerProducto
    @Consecutivo int
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
           P.Estado,
           CONVERT(decimal(5,2), ISNULL(D.PorcentajeDescuento, 0)) AS PorcentajeDescuento,
           CONVERT(decimal(10,2), P.Precio * (1 - ISNULL(D.PorcentajeDescuento, 0) / 100.0)) AS PrecioFinal
      FROM dbo.tbProducto P
      INNER JOIN dbo.tbCategoria C
              ON C.Consecutivo = P.ConsecutivoCategoria
      OUTER APPLY
      (
          SELECT MAX(PR.Porcentaje) AS PorcentajeDescuento
            FROM dbo.tbPromocion PR
           WHERE PR.Estado = 1
             AND GETDATE() BETWEEN PR.FechaInicio AND PR.FechaFin
             AND
             (
                 PR.ConsecutivoProducto = P.Consecutivo
                 OR PR.ConsecutivoCategoria = P.ConsecutivoCategoria
             )
      ) D
     WHERE P.Consecutivo = @Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spBuscarProductos
    @Busqueda varchar(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Termino varchar(150) = LTRIM(RTRIM(ISNULL(@Busqueda, '')));

    SELECT P.Consecutivo,
           P.Nombre,
           P.Descripcion,
           P.Precio,
           P.Stock,
           P.Imagen,
           P.ConsecutivoCategoria,
           C.NombreCategoria,
           P.Estado,
           CONVERT(decimal(5,2), ISNULL(D.PorcentajeDescuento, 0)) AS PorcentajeDescuento,
           CONVERT(decimal(10,2), P.Precio * (1 - ISNULL(D.PorcentajeDescuento, 0) / 100.0)) AS PrecioFinal
      FROM dbo.tbProducto P
      INNER JOIN dbo.tbCategoria C
              ON C.Consecutivo = P.ConsecutivoCategoria
      OUTER APPLY
      (
          SELECT MAX(PR.Porcentaje) AS PorcentajeDescuento
            FROM dbo.tbPromocion PR
           WHERE PR.Estado = 1
             AND GETDATE() BETWEEN PR.FechaInicio AND PR.FechaFin
             AND
             (
                 PR.ConsecutivoProducto = P.Consecutivo
                 OR PR.ConsecutivoCategoria = P.ConsecutivoCategoria
             )
      ) D
     WHERE P.Estado = 1
       AND C.Estado = 1
       AND (@Termino = '' OR P.Nombre LIKE '%' + @Termino + '%')
     ORDER BY P.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarCategoriasAdmin
AS
BEGIN
    SET NOCOUNT ON;

    SELECT C.Consecutivo,
           C.NombreCategoria,
           CONVERT(varchar(500), C.Descripcion) AS Descripcion,
           C.Estado,
           COUNT(P.Consecutivo) AS CantidadProductos
      FROM dbo.tbCategoria C
      LEFT JOIN dbo.tbProducto P
             ON P.ConsecutivoCategoria = C.Consecutivo
     GROUP BY C.Consecutivo, C.NombreCategoria, CONVERT(varchar(500), C.Descripcion), C.Estado
     ORDER BY C.NombreCategoria;
END
GO

CREATE OR ALTER PROCEDURE dbo.spGuardarCategoriaAdmin
    @Consecutivo int = 0,
    @NombreCategoria varchar(100),
    @Descripcion varchar(500) = NULL,
    @Estado bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    SET @NombreCategoria = LTRIM(RTRIM(@NombreCategoria));

    IF @NombreCategoria = ''
        THROW 51100, 'El nombre de la categoría es obligatorio.', 1;

    IF EXISTS
    (
        SELECT 1
          FROM dbo.tbCategoria
         WHERE LOWER(NombreCategoria) = LOWER(@NombreCategoria)
           AND Consecutivo <> ISNULL(@Consecutivo, 0)
    )
        THROW 51101, 'Ya existe una categoría con ese nombre.', 1;

    IF ISNULL(@Consecutivo, 0) = 0
    BEGIN
        INSERT INTO dbo.tbCategoria (NombreCategoria, Descripcion, Estado)
        VALUES (@NombreCategoria, @Descripcion, @Estado);

        SET @Consecutivo = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.tbCategoria
           SET NombreCategoria = @NombreCategoria,
               Descripcion = @Descripcion,
               Estado = @Estado
         WHERE Consecutivo = @Consecutivo;

        IF @@ROWCOUNT = 0
            THROW 51102, 'La categoría solicitada no existe.', 1;
    END

    SELECT @Consecutivo AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spEliminarCategoriaAdmin
    @Consecutivo int
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.tbProducto WHERE ConsecutivoCategoria = @Consecutivo)
        THROW 51103, 'No se puede eliminar una categoría que tiene productos asociados.', 1;

    IF EXISTS (SELECT 1 FROM dbo.tbPromocion WHERE ConsecutivoCategoria = @Consecutivo)
        THROW 51104, 'No se puede eliminar una categoría que tiene promociones asociadas.', 1;

    DELETE FROM dbo.tbCategoria WHERE Consecutivo = @Consecutivo;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarPromociones
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PR.Consecutivo,
           PR.Nombre,
           PR.Porcentaje,
           PR.ConsecutivoProducto,
           P.Nombre AS NombreProducto,
           PR.ConsecutivoCategoria,
           C.NombreCategoria,
           PR.FechaInicio,
           PR.FechaFin,
           PR.Estado
      FROM dbo.tbPromocion PR
      LEFT JOIN dbo.tbProducto P ON P.Consecutivo = PR.ConsecutivoProducto
      LEFT JOIN dbo.tbCategoria C ON C.Consecutivo = PR.ConsecutivoCategoria
     ORDER BY PR.FechaInicio DESC, PR.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.spGuardarPromocion
    @Consecutivo int = 0,
    @Nombre varchar(150),
    @Porcentaje decimal(5,2),
    @ConsecutivoProducto int = NULL,
    @ConsecutivoCategoria int = NULL,
    @FechaInicio datetime,
    @FechaFin datetime,
    @Estado bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @Porcentaje <= 0 OR @Porcentaje > 90
        THROW 51200, 'El descuento debe ser mayor que cero y no puede superar el 90%.', 1;

    IF @FechaFin < @FechaInicio
        THROW 51201, 'La fecha final no puede ser anterior a la fecha inicial.', 1;

    IF (@ConsecutivoProducto IS NULL AND @ConsecutivoCategoria IS NULL)
       OR (@ConsecutivoProducto IS NOT NULL AND @ConsecutivoCategoria IS NOT NULL)
        THROW 51202, 'La promoción debe asignarse a un producto o a una categoría.', 1;

    IF @ConsecutivoProducto IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.tbProducto WHERE Consecutivo = @ConsecutivoProducto)
        THROW 51203, 'El producto seleccionado no existe.', 1;

    IF @ConsecutivoCategoria IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.tbCategoria WHERE Consecutivo = @ConsecutivoCategoria)
        THROW 51204, 'La categoría seleccionada no existe.', 1;

    IF ISNULL(@Consecutivo, 0) = 0
    BEGIN
        INSERT INTO dbo.tbPromocion
            (Nombre, Porcentaje, ConsecutivoProducto, ConsecutivoCategoria, FechaInicio, FechaFin, Estado)
        VALUES
            (@Nombre, @Porcentaje, @ConsecutivoProducto, @ConsecutivoCategoria, @FechaInicio, @FechaFin, @Estado);
        SET @Consecutivo = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.tbPromocion
           SET Nombre = @Nombre,
               Porcentaje = @Porcentaje,
               ConsecutivoProducto = @ConsecutivoProducto,
               ConsecutivoCategoria = @ConsecutivoCategoria,
               FechaInicio = @FechaInicio,
               FechaFin = @FechaFin,
               Estado = @Estado
         WHERE Consecutivo = @Consecutivo;

        IF @@ROWCOUNT = 0
            THROW 51205, 'La promoción solicitada no existe.', 1;
    END

    SELECT @Consecutivo AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoPromocion
    @Consecutivo int,
    @Estado bit
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.tbPromocion SET Estado = @Estado WHERE Consecutivo = @Consecutivo;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

/* ============================================================
   CARRITO
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spActualizarCantidadCarrito
    @ConsecutivoDetalle int,
    @ConsecutivoUsuario int,
    @Cantidad int
AS
BEGIN
    SET NOCOUNT ON;

    IF @Cantidad < 1
        THROW 51300, 'La cantidad debe ser mayor o igual a uno.', 1;

    DECLARE @Stock int;
    DECLARE @EstadoProducto bit;

    SELECT @Stock = P.Stock,
           @EstadoProducto = P.Estado
      FROM dbo.tbDetalleCarrito DC
      INNER JOIN dbo.tbCarrito C ON C.Consecutivo = DC.ConsecutivoCarrito
      INNER JOIN dbo.tbProducto P ON P.Consecutivo = DC.ConsecutivoProducto
     WHERE DC.Consecutivo = @ConsecutivoDetalle
       AND C.ConsecutivoUsuario = @ConsecutivoUsuario;

    IF @Stock IS NULL
        THROW 51301, 'El producto no pertenece al carrito del usuario.', 1;

    IF @EstadoProducto = 0
        THROW 51302, 'El producto ya no está disponible.', 1;

    IF @Cantidad > @Stock
        THROW 51303, 'La cantidad solicitada supera el stock disponible.', 1;

    UPDATE DC
       SET DC.Cantidad = @Cantidad,
           DC.Subtotal = @Cantidad * DC.PrecioUnitario
      FROM dbo.tbDetalleCarrito DC
      INNER JOIN dbo.tbCarrito C ON C.Consecutivo = DC.ConsecutivoCarrito
     WHERE DC.Consecutivo = @ConsecutivoDetalle
       AND C.ConsecutivoUsuario = @ConsecutivoUsuario;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

/* ============================================================
   PEDIDOS, CLIENTES E HISTORIAL
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spListarPedidosUsuario
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Consecutivo,
           P.FechaPedido,
           P.Total,
           P.Estado,
           ISNULL(SUM(DP.Cantidad), 0) AS CantidadProductos,
           E.FechaInicio AS FechaEntregaEstimada
      FROM dbo.tbPedido P
      LEFT JOIN dbo.tbDetallePedido DP ON DP.ConsecutivoPedido = P.Consecutivo
      LEFT JOIN dbo.tbEventoCalendario E
             ON E.ConsecutivoPedido = P.Consecutivo
            AND E.TipoEvento = 'EntregaCliente'
     WHERE P.ConsecutivoUsuario = @ConsecutivoUsuario
     GROUP BY P.Consecutivo, P.FechaPedido, P.Total, P.Estado, E.FechaInicio
     ORDER BY P.FechaPedido DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spDetallePedidoSeguro
    @ConsecutivoPedido int,
    @ConsecutivoUsuario int = NULL,
    @EsAdministrador bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
          FROM dbo.tbPedido
         WHERE Consecutivo = @ConsecutivoPedido
           AND (@EsAdministrador = 1 OR ConsecutivoUsuario = @ConsecutivoUsuario)
    )
        THROW 51400, 'El pedido solicitado no existe o no pertenece al usuario.', 1;

    SELECT P.Consecutivo,
           P.ConsecutivoUsuario,
           U.Nombre AS NombreCliente,
           U.CorreoElectronico,
           P.FechaPedido,
           P.Total,
           P.Estado,
           E.FechaInicio AS FechaEntregaEstimada
      FROM dbo.tbPedido P
      INNER JOIN dbo.tbUsuario U ON U.Consecutivo = P.ConsecutivoUsuario
      LEFT JOIN dbo.tbEventoCalendario E
             ON E.ConsecutivoPedido = P.Consecutivo
            AND E.TipoEvento = 'EntregaCliente'
     WHERE P.Consecutivo = @ConsecutivoPedido;

    SELECT DP.Consecutivo,
           DP.ConsecutivoProducto,
           PR.Nombre AS NombreProducto,
           PR.Imagen,
           DP.Cantidad,
           DP.PrecioUnitario,
           DP.Subtotal
      FROM dbo.tbDetallePedido DP
      INNER JOIN dbo.tbProducto PR ON PR.Consecutivo = DP.ConsecutivoProducto
     WHERE DP.ConsecutivoPedido = @ConsecutivoPedido
     ORDER BY DP.Consecutivo;

    SELECT H.Consecutivo,
           H.EstadoAnterior,
           H.EstadoNuevo,
           H.FechaCambio,
           U.Nombre AS NombreAdministrador
      FROM dbo.tbHistorialEstadoPedido H
      LEFT JOIN dbo.tbUsuario U ON U.Consecutivo = H.ConsecutivoAdministrador
     WHERE H.ConsecutivoPedido = @ConsecutivoPedido
     ORDER BY H.FechaCambio DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarTodosPedidos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Consecutivo,
           P.ConsecutivoUsuario,
           U.Nombre AS NombreCliente,
           U.CorreoElectronico,
           P.FechaPedido,
           P.Total,
           P.Estado,
           ISNULL(SUM(DP.Cantidad), 0) AS CantidadProductos,
           E.FechaInicio AS FechaEntregaEstimada
      FROM dbo.tbPedido P
      INNER JOIN dbo.tbUsuario U ON U.Consecutivo = P.ConsecutivoUsuario
      LEFT JOIN dbo.tbDetallePedido DP ON DP.ConsecutivoPedido = P.Consecutivo
      LEFT JOIN dbo.tbEventoCalendario E
             ON E.ConsecutivoPedido = P.Consecutivo
            AND E.TipoEvento = 'EntregaCliente'
     GROUP BY P.Consecutivo, P.ConsecutivoUsuario, U.Nombre, U.CorreoElectronico,
              P.FechaPedido, P.Total, P.Estado, E.FechaInicio
     ORDER BY P.FechaPedido DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spActualizarEstadoPedidoSeguro
    @ConsecutivoPedido int,
    @Estado varchar(30),
    @ConsecutivoAdministrador int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Estado NOT IN ('Pendiente', 'Confirmado', 'Preparando', 'Enviado', 'Entregado', 'Cancelado')
        THROW 51401, 'El estado seleccionado no es válido.', 1;

    DECLARE @EstadoAnterior varchar(30);
    SELECT @EstadoAnterior = Estado FROM dbo.tbPedido WHERE Consecutivo = @ConsecutivoPedido;

    IF @EstadoAnterior IS NULL
        THROW 51402, 'El pedido solicitado no existe.', 1;

    IF @EstadoAnterior = @Estado
        THROW 51403, 'El pedido ya tiene el estado seleccionado.', 1;

    BEGIN TRANSACTION;

    UPDATE dbo.tbPedido SET Estado = @Estado WHERE Consecutivo = @ConsecutivoPedido;

    INSERT INTO dbo.tbHistorialEstadoPedido
        (ConsecutivoPedido, EstadoAnterior, EstadoNuevo, FechaCambio, ConsecutivoAdministrador)
    VALUES
        (@ConsecutivoPedido, @EstadoAnterior, @Estado, GETDATE(), @ConsecutivoAdministrador);

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarClientes
AS
BEGIN
    SET NOCOUNT ON;

    SELECT U.Consecutivo,
           U.Nombre,
           U.CorreoElectronico,
           U.Estado,
           U.FechaRegistro,
           COUNT(P.Consecutivo) AS CantidadPedidos,
           CONVERT(decimal(12,2), ISNULL(SUM(P.Total), 0)) AS TotalCompras
      FROM dbo.tbUsuario U
      INNER JOIN dbo.tbRol R ON R.Consecutivo = U.ConsecutivoRol
      LEFT JOIN dbo.tbPedido P ON P.ConsecutivoUsuario = U.Consecutivo
     WHERE R.Nombre <> 'Administrador'
     GROUP BY U.Consecutivo, U.Nombre, U.CorreoElectronico, U.Estado, U.FechaRegistro
     ORDER BY U.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.spHistorialComprasCliente
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Consecutivo,
           P.FechaPedido,
           P.Total,
           P.Estado,
           ISNULL(SUM(DP.Cantidad), 0) AS CantidadProductos
      FROM dbo.tbPedido P
      LEFT JOIN dbo.tbDetallePedido DP ON DP.ConsecutivoPedido = P.Consecutivo
     WHERE P.ConsecutivoUsuario = @ConsecutivoUsuario
     GROUP BY P.Consecutivo, P.FechaPedido, P.Total, P.Estado
     ORDER BY P.FechaPedido DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoCliente
    @ConsecutivoUsuario int,
    @Estado bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE U
       SET U.Estado = @Estado
      FROM dbo.tbUsuario U
      INNER JOIN dbo.tbRol R ON R.Consecutivo = U.ConsecutivoRol
     WHERE U.Consecutivo = @ConsecutivoUsuario
       AND R.Nombre <> 'Administrador';

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

/* ============================================================
   BLOG Y CONTACTO
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.spListarPublicaciones
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Consecutivo,
           P.Titulo,
           P.Resumen,
           P.Contenido,
           P.Autor,
           P.Imagen,
           P.FechaPublicacion,
           P.Estado,
           COUNT(C.Consecutivo) AS CantidadComentarios
      FROM dbo.tbPublicacion P
      LEFT JOIN dbo.tbComentarioPublicacion C
             ON C.ConsecutivoPublicacion = P.Consecutivo
            AND C.Estado = 1
     WHERE P.Estado = 1
     GROUP BY P.Consecutivo, P.Titulo, P.Resumen, P.Contenido, P.Autor,
              P.Imagen, P.FechaPublicacion, P.Estado
     ORDER BY P.FechaPublicacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spObtenerPublicacion
    @Consecutivo int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Consecutivo, Titulo, Resumen, Contenido, Autor, Imagen, FechaPublicacion, Estado
      FROM dbo.tbPublicacion
     WHERE Consecutivo = @Consecutivo
       AND Estado = 1;

    SELECT C.Consecutivo,
           C.ConsecutivoPublicacion,
           C.ConsecutivoUsuario,
           C.ConsecutivoComentarioPadre,
           C.NombreAutor,
           C.Contenido,
           C.FechaComentario,
           CASE WHEN R.Nombre = 'Administrador' THEN CONVERT(bit, 1) ELSE CONVERT(bit, 0) END AS EsRespuestaAdmin
      FROM dbo.tbComentarioPublicacion C
      LEFT JOIN dbo.tbUsuario U ON U.Consecutivo = C.ConsecutivoUsuario
      LEFT JOIN dbo.tbRol R ON R.Consecutivo = U.ConsecutivoRol
     WHERE C.ConsecutivoPublicacion = @Consecutivo
       AND C.Estado = 1
     ORDER BY COALESCE(C.ConsecutivoComentarioPadre, C.Consecutivo),
              CASE WHEN C.ConsecutivoComentarioPadre IS NULL THEN 0 ELSE 1 END,
              C.FechaComentario;
END
GO

CREATE OR ALTER PROCEDURE dbo.spAgregarComentarioPublicacion
    @ConsecutivoPublicacion int,
    @ConsecutivoUsuario int = NULL,
    @NombreAutor varchar(150),
    @Contenido varchar(1000)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.tbPublicacion
         WHERE Consecutivo = @ConsecutivoPublicacion AND Estado = 1
    )
        THROW 51500, 'La publicación solicitada no existe.', 1;

    IF LTRIM(RTRIM(@NombreAutor)) = '' OR LTRIM(RTRIM(@Contenido)) = ''
        THROW 51501, 'El autor y el comentario son obligatorios.', 1;

    INSERT INTO dbo.tbComentarioPublicacion
        (ConsecutivoPublicacion, ConsecutivoUsuario, NombreAutor, Contenido, FechaComentario, Estado)
    VALUES
        (@ConsecutivoPublicacion, @ConsecutivoUsuario, @NombreAutor, @Contenido, GETDATE(), 1);

    SELECT CONVERT(int, SCOPE_IDENTITY()) AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarPublicacionesAdmin
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.Consecutivo, P.Titulo, P.Resumen, P.Contenido, P.Autor, P.Imagen,
           P.FechaPublicacion, P.Estado,
           COUNT(C.Consecutivo) AS CantidadComentarios
      FROM dbo.tbPublicacion P
      LEFT JOIN dbo.tbComentarioPublicacion C
             ON C.ConsecutivoPublicacion = P.Consecutivo AND C.Estado = 1
     GROUP BY P.Consecutivo, P.Titulo, P.Resumen, P.Contenido, P.Autor,
              P.Imagen, P.FechaPublicacion, P.Estado
     ORDER BY P.FechaPublicacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spObtenerPublicacionAdmin
    @Consecutivo int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Consecutivo, Titulo, Resumen, Contenido, Autor, Imagen,
           FechaPublicacion, Estado
      FROM dbo.tbPublicacion
     WHERE Consecutivo = @Consecutivo;

    SELECT C.Consecutivo, C.ConsecutivoPublicacion, C.ConsecutivoUsuario,
           C.ConsecutivoComentarioPadre, C.NombreAutor, C.Contenido,
           C.FechaComentario, C.Estado,
           CASE WHEN R.Nombre = 'Administrador' THEN CONVERT(bit, 1) ELSE CONVERT(bit, 0) END AS EsRespuestaAdmin
      FROM dbo.tbComentarioPublicacion C
      LEFT JOIN dbo.tbUsuario U ON U.Consecutivo = C.ConsecutivoUsuario
      LEFT JOIN dbo.tbRol R ON R.Consecutivo = U.ConsecutivoRol
     WHERE C.ConsecutivoPublicacion = @Consecutivo
     ORDER BY COALESCE(C.ConsecutivoComentarioPadre, C.Consecutivo),
              CASE WHEN C.ConsecutivoComentarioPadre IS NULL THEN 0 ELSE 1 END,
              C.FechaComentario;
END
GO

CREATE OR ALTER PROCEDURE dbo.spGuardarPublicacionAdmin
    @Consecutivo int = 0,
    @Titulo varchar(180),
    @Resumen varchar(500),
    @Contenido varchar(max),
    @Autor varchar(150),
    @Imagen varchar(255),
    @Estado bit
AS
BEGIN
    SET NOCOUNT ON;
    IF LTRIM(RTRIM(@Titulo)) = '' OR LTRIM(RTRIM(@Resumen)) = '' OR LTRIM(RTRIM(@Contenido)) = ''
        THROW 51600, 'El título, resumen y contenido son obligatorios.', 1;
    IF LTRIM(RTRIM(ISNULL(@Imagen, ''))) = ''
        THROW 51601, 'La imagen de la publicación es obligatoria.', 1;

    IF ISNULL(@Consecutivo, 0) = 0
    BEGIN
        INSERT INTO dbo.tbPublicacion(Titulo, Resumen, Contenido, Autor, Imagen, FechaPublicacion, Estado)
        VALUES(@Titulo, @Resumen, @Contenido, @Autor, @Imagen, GETDATE(), @Estado);
        SET @Consecutivo = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.tbPublicacion
           SET Titulo=@Titulo, Resumen=@Resumen, Contenido=@Contenido,
               Imagen=@Imagen, Estado=@Estado
         WHERE Consecutivo=@Consecutivo;
        IF @@ROWCOUNT = 0 THROW 51602, 'La publicación solicitada no existe.', 1;
    END
    SELECT @Consecutivo AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spEliminarPublicacionAdmin
    @Consecutivo int
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.tbPublicacion SET Estado=0 WHERE Consecutivo=@Consecutivo;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoPublicacionAdmin
    @Consecutivo int,
    @Estado bit
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.tbPublicacion SET Estado=@Estado WHERE Consecutivo=@Consecutivo;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.spResponderComentarioPublicacion
    @ConsecutivoComentarioPadre int,
    @ConsecutivoAdministrador int,
    @Contenido varchar(1000)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ConsecutivoPublicacion int, @NombreAdministrador varchar(150);
    SELECT @ConsecutivoPublicacion=ConsecutivoPublicacion
      FROM dbo.tbComentarioPublicacion
     WHERE Consecutivo=@ConsecutivoComentarioPadre AND Estado=1;
    SELECT @NombreAdministrador=U.Nombre
      FROM dbo.tbUsuario U INNER JOIN dbo.tbRol R ON R.Consecutivo=U.ConsecutivoRol
     WHERE U.Consecutivo=@ConsecutivoAdministrador AND R.Nombre='Administrador';
    IF @ConsecutivoPublicacion IS NULL THROW 51603, 'El comentario solicitado no existe.', 1;
    IF @NombreAdministrador IS NULL THROW 51604, 'El usuario no tiene permisos administrativos.', 1;
    IF LTRIM(RTRIM(@Contenido))='' THROW 51605, 'La respuesta es obligatoria.', 1;

    INSERT INTO dbo.tbComentarioPublicacion
        (ConsecutivoPublicacion, ConsecutivoUsuario, ConsecutivoComentarioPadre,
         NombreAutor, Contenido, FechaComentario, Estado)
    VALUES
        (@ConsecutivoPublicacion, @ConsecutivoAdministrador, @ConsecutivoComentarioPadre,
         @NombreAdministrador, @Contenido, GETDATE(), 1);
    SELECT CONVERT(int, SCOPE_IDENTITY()) AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spEliminarComentarioPublicacion
    @Consecutivo int
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.tbComentarioPublicacion
       SET Estado=0
     WHERE Consecutivo=@Consecutivo;
    UPDATE dbo.tbComentarioPublicacion
       SET Estado=0
     WHERE ConsecutivoComentarioPadre=@Consecutivo;
    SELECT CASE WHEN @@ROWCOUNT >= 0 AND EXISTS
        (SELECT 1 FROM dbo.tbComentarioPublicacion WHERE Consecutivo=@Consecutivo)
        THEN 1 ELSE 0 END AS FilasAfectadas;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRegistrarConsultaContacto
    @Nombre varchar(150),
    @CorreoElectronico varchar(150),
    @Asunto varchar(180),
    @Mensaje varchar(2000)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tbConsultaContacto
        (Nombre, CorreoElectronico, Asunto, Mensaje, FechaRegistro, Estado)
    VALUES
        (@Nombre, @CorreoElectronico, @Asunto, @Mensaje, GETDATE(), 'Nueva');

    SELECT CONVERT(int, SCOPE_IDENTITY()) AS Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarConsultasContacto
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Consecutivo, Nombre, CorreoElectronico, Asunto, Mensaje, FechaRegistro, Estado
      FROM dbo.tbConsultaContacto
     ORDER BY FechaRegistro DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spListarCarrito
    @ConsecutivoCarrito INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT D.Consecutivo,
           D.ConsecutivoProducto,
           P.Nombre AS NombreProducto,
           D.Cantidad,
           D.PrecioUnitario,
           D.Subtotal,
           P.Stock
    FROM dbo.tbDetalleCarrito D
    INNER JOIN dbo.tbProducto P ON P.Consecutivo = D.ConsecutivoProducto
    WHERE D.ConsecutivoCarrito = @ConsecutivoCarrito
    ORDER BY D.Consecutivo;
END
GO

CREATE OR ALTER PROCEDURE dbo.spConsultarEventosCalendario
    @ConsecutivoUsuario int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT E.Consecutivo, E.Titulo, E.Descripcion, E.FechaInicio, E.FechaFin,
           E.TipoEvento, E.ConsecutivoPedido, P.Estado AS EstadoPedido
      FROM dbo.tbEventoCalendario E
      LEFT JOIN dbo.tbPedido P ON P.Consecutivo = E.ConsecutivoPedido
     WHERE E.ConsecutivoUsuario = @ConsecutivoUsuario
     ORDER BY E.FechaInicio;
END
GO

/* Publicaciones iniciales para demostrar el requisito RFC-020. */
IF NOT EXISTS (SELECT 1 FROM dbo.tbPublicacion)
BEGIN
    INSERT INTO dbo.tbPublicacion
        (Titulo, Resumen, Contenido, Autor, Imagen, FechaPublicacion, Estado)
    VALUES
        ('Cómo elegir una laptop para estudiar y trabajar',
         'Los puntos esenciales que conviene revisar antes de comprar una computadora portátil.',
         'Elegir una laptop comienza por identificar el uso principal. Para tareas universitarias y trabajo de oficina, 16 GB de memoria RAM y una unidad SSD ofrecen una experiencia fluida. También conviene considerar la autonomía, el peso y la disponibilidad de puertos. Si el equipo se utilizará para diseño o programación, un procesador de varios núcleos y una pantalla de buena resolución aportan comodidad y rendimiento durante jornadas extensas.',
         'Equipo MiniStore', 'post-image1.jpg', DATEADD(DAY, -12, GETDATE()), 1),

        ('Cinco hábitos para cuidar la batería del teléfono',
         'Buenas prácticas sencillas para prolongar la vida útil de la batería.',
         'La batería se conserva mejor cuando se evita exponer el dispositivo a temperaturas extremas. No es necesario descargarlo por completo antes de cargarlo y, cuando sea posible, conviene mantener la carga entre niveles moderados. Reducir el brillo, revisar las aplicaciones en segundo plano y utilizar cargadores confiables también ayuda a mantener un rendimiento estable con el paso del tiempo.',
         'Equipo MiniStore', 'post-item2.jpg', DATEADD(DAY, -7, GETDATE()), 1),

        ('Tecnología útil para organizar el día',
         'Accesorios y dispositivos que facilitan la productividad cotidiana.',
         'Un reloj inteligente puede centralizar recordatorios y actividad física, mientras que unos audífonos inalámbricos ayudan a concentrarse en espacios compartidos. Una batería portátil y un mouse cómodo completan un conjunto práctico para quienes estudian o trabajan en movimiento. La mejor tecnología no es la más compleja, sino la que resuelve una necesidad concreta.',
         'Equipo MiniStore', 'post-image.jpg', DATEADD(DAY, -3, GETDATE()), 1);
END
GO
