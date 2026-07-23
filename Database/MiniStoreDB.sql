-- ============================================
-- CREACIÓN DE LA BASE DE DATOS
-- ============================================

USE [master]
GO

-- Eliminar la base de datos si ya existe (opcional)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Ministore_DB')
BEGIN
    ALTER DATABASE [Ministore_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE [Ministore_DB]
END
GO

CREATE DATABASE [Ministore_DB]
GO

USE [Ministore_DB]
GO

-- ============================================
-- TABLAS
-- ============================================

-- 1. Tabla de Roles
CREATE TABLE [dbo].[tbRol](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [Nombre] [varchar](50) NOT NULL,
 CONSTRAINT [PK_tbRol] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- 2. Tabla de Usuarios
CREATE TABLE [dbo].[tbUsuario](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [Nombre] [varchar](250) NOT NULL,
    [CorreoElectronico] [varchar](100) NOT NULL,
    [Contrasenna] [varchar](100) NOT NULL,
    [Estado] [bit] NOT NULL,
    [UsaContrasennaTemp] [bit] NOT NULL,
    [ConsecutivoRol] [int] NOT NULL,
    [FechaRegistro] [datetime] NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_tbUsuario] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- 3. Tabla de Errores
CREATE TABLE [dbo].[tbError](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [Mensaje] [varchar](max) NOT NULL,
    [Lugar] [varchar](50) NOT NULL,
    [FechaHora] [datetime] NOT NULL,
    [ConsecutivoUsuario] [int] NOT NULL,
 CONSTRAINT [PK_tbError] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- 4. Tabla de Categorías
CREATE TABLE [dbo].[tbCategoria](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [NombreCategoria] [varchar](100) NOT NULL,
    [Descripcion] [text] NULL,
    [Estado] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_tbCategoria] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- 5. Tabla de Productos
CREATE TABLE [dbo].[tbProducto](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [Nombre] [varchar](150) NOT NULL,
    [Descripcion] [text] NULL,
    [Precio] [decimal](10,2) NOT NULL,
    [Stock] [int] NOT NULL DEFAULT 0,
    [Imagen] [varchar](255) NULL,
    [ConsecutivoCategoria] [int] NOT NULL,
    [Estado] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_tbProducto] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- 6. Tabla de Carritos
CREATE TABLE [dbo].[tbCarrito](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [ConsecutivoUsuario] [int] NOT NULL,
    [FechaCreacion] [datetime] NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_tbCarrito] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- 7. Tabla de Detalle Carritos
CREATE TABLE [dbo].[tbDetalleCarrito](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [ConsecutivoCarrito] [int] NOT NULL,
    [ConsecutivoProducto] [int] NOT NULL,
    [Cantidad] [int] NOT NULL,
    [PrecioUnitario] [decimal](10,2) NOT NULL,
    [Subtotal] [decimal](10,2) NOT NULL,
 CONSTRAINT [PK_tbDetalleCarrito] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- 8. Tabla de Pedidos
CREATE TABLE [dbo].[tbPedido](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [ConsecutivoUsuario] [int] NOT NULL,
    [FechaPedido] [datetime] NOT NULL DEFAULT GETDATE(),
    [Total] [decimal](10,2) NOT NULL,
    [Estado] [varchar](30) NOT NULL DEFAULT 'Pendiente',
 CONSTRAINT [PK_tbPedido] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- 9. Tabla de Detalle Pedido
CREATE TABLE [dbo].[tbDetallePedido](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [ConsecutivoPedido] [int] NOT NULL,
    [ConsecutivoProducto] [int] NOT NULL,
    [Cantidad] [int] NOT NULL,
    [PrecioUnitario] [decimal](10,2) NOT NULL,
    [Subtotal] [decimal](10,2) NOT NULL,
 CONSTRAINT [PK_tbDetallePedido] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- 10. Tabla de Pagos
CREATE TABLE [dbo].[tbPago](
    [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
    [ConsecutivoPedido] [int] NOT NULL,
    [MetodoPago] [varchar](50) NOT NULL,
    [Monto] [decimal](10,2) NOT NULL,
    [FechaPago] [datetime] NOT NULL DEFAULT GETDATE(),
    [EstadoPago] [varchar](30) NOT NULL DEFAULT 'Pendiente',
 CONSTRAINT [PK_tbPago] PRIMARY KEY CLUSTERED 
(
    [Consecutivo] ASC
)
) ON [PRIMARY]
GO

-- ============================================
-- ÍNDICES ÚNICOS
-- ============================================

ALTER TABLE [dbo].[tbUsuario] ADD CONSTRAINT [UK_CorreoElectronico] UNIQUE NONCLUSTERED 
(
    [CorreoElectronico] ASC
)
GO

-- ============================================
-- CLAVES FORÁNEAS
-- ============================================

-- Usuario - Rol
ALTER TABLE [dbo].[tbUsuario] WITH CHECK ADD CONSTRAINT [FK_tbUsuario_tbRol] 
FOREIGN KEY([ConsecutivoRol]) REFERENCES [dbo].[tbRol] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbUsuario] CHECK CONSTRAINT [FK_tbUsuario_tbRol]
GO

-- Error - Usuario
ALTER TABLE [dbo].[tbError] WITH CHECK ADD CONSTRAINT [FK_tbError_tbUsuario] 
FOREIGN KEY([ConsecutivoUsuario]) REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbError] CHECK CONSTRAINT [FK_tbError_tbUsuario]
GO

-- Producto - Categoría
ALTER TABLE [dbo].[tbProducto] WITH CHECK ADD CONSTRAINT [FK_tbProducto_tbCategoria] 
FOREIGN KEY([ConsecutivoCategoria]) REFERENCES [dbo].[tbCategoria] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbProducto] CHECK CONSTRAINT [FK_tbProducto_tbCategoria]
GO

-- Carrito - Usuario
ALTER TABLE [dbo].[tbCarrito] WITH CHECK ADD CONSTRAINT [FK_tbCarrito_tbUsuario] 
FOREIGN KEY([ConsecutivoUsuario]) REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbCarrito] CHECK CONSTRAINT [FK_tbCarrito_tbUsuario]
GO

-- DetalleCarrito - Carrito
ALTER TABLE [dbo].[tbDetalleCarrito] WITH CHECK ADD CONSTRAINT [FK_tbDetalleCarrito_tbCarrito] 
FOREIGN KEY([ConsecutivoCarrito]) REFERENCES [dbo].[tbCarrito] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbDetalleCarrito] CHECK CONSTRAINT [FK_tbDetalleCarrito_tbCarrito]
GO

-- DetalleCarrito - Producto
ALTER TABLE [dbo].[tbDetalleCarrito] WITH CHECK ADD CONSTRAINT [FK_tbDetalleCarrito_tbProducto] 
FOREIGN KEY([ConsecutivoProducto]) REFERENCES [dbo].[tbProducto] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbDetalleCarrito] CHECK CONSTRAINT [FK_tbDetalleCarrito_tbProducto]
GO

-- Pedido - Usuario
ALTER TABLE [dbo].[tbPedido] WITH CHECK ADD CONSTRAINT [FK_tbPedido_tbUsuario] 
FOREIGN KEY([ConsecutivoUsuario]) REFERENCES [dbo].[tbUsuario] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbPedido] CHECK CONSTRAINT [FK_tbPedido_tbUsuario]
GO

-- DetallePedido - Pedido
ALTER TABLE [dbo].[tbDetallePedido] WITH CHECK ADD CONSTRAINT [FK_tbDetallePedido_tbPedido] 
FOREIGN KEY([ConsecutivoPedido]) REFERENCES [dbo].[tbPedido] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbDetallePedido] CHECK CONSTRAINT [FK_tbDetallePedido_tbPedido]
GO

-- DetallePedido - Producto
ALTER TABLE [dbo].[tbDetallePedido] WITH CHECK ADD CONSTRAINT [FK_tbDetallePedido_tbProducto] 
FOREIGN KEY([ConsecutivoProducto]) REFERENCES [dbo].[tbProducto] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbDetallePedido] CHECK CONSTRAINT [FK_tbDetallePedido_tbProducto]
GO

-- Pago - Pedido
ALTER TABLE [dbo].[tbPago] WITH CHECK ADD CONSTRAINT [FK_tbPago_tbPedido] 
FOREIGN KEY([ConsecutivoPedido]) REFERENCES [dbo].[tbPedido] ([Consecutivo])
GO
ALTER TABLE [dbo].[tbPago] CHECK CONSTRAINT [FK_tbPago_tbPedido]
GO

-- ============================================
-- DATOS INICIALES
-- ============================================

-- Roles
SET IDENTITY_INSERT [dbo].[tbRol] ON 
GO
INSERT [dbo].[tbRol] ([Consecutivo], [Nombre]) VALUES (1, N'Usuario')
GO
INSERT [dbo].[tbRol] ([Consecutivo], [Nombre]) VALUES (2, N'Administrador')
GO
SET IDENTITY_INSERT [dbo].[tbRol] OFF
GO

-- Usuarios
SET IDENTITY_INSERT [dbo].[tbUsuario] ON 
GO
INSERT [dbo].[tbUsuario] ([Consecutivo], [Nombre], [CorreoElectronico], [Contrasenna], [Estado], [UsaContrasennaTemp], [ConsecutivoRol], [FechaRegistro]) 
VALUES (7, N'Eduardo Calvo Castillo', N'ecalvo90415@ufide.ac.cr', N'$2a$11$lR12jLMENpuG62nFkwyW/e7wSjZEnp837SGeArY1d0FaLeDEcsBzu', 1, 0, 2, GETDATE())
GO
INSERT [dbo].[tbUsuario] ([Consecutivo], [Nombre], [CorreoElectronico], [Contrasenna], [Estado], [UsaContrasennaTemp], [ConsecutivoRol], [FechaRegistro]) 
VALUES (8, N'Manuel Mora Monge', N'mmora90870@ufide.ac.cr', N'$2a$11$faCoDb2o.a3SmTdro0ePPOdiTyN2IykN6IgEOhtKXue8B8.cdUe0W', 1, 0, 1, GETDATE())
GO
SET IDENTITY_INSERT [dbo].[tbUsuario] OFF
GO

-- Categorías de ejemplo
SET IDENTITY_INSERT [dbo].[tbCategoria] ON 
GO
INSERT [dbo].[tbCategoria] ([Consecutivo], [NombreCategoria], [Descripcion], [Estado]) 
VALUES (1, N'Electrónicos', N'Productos electrónicos y tecnología', 1)
GO
INSERT [dbo].[tbCategoria] ([Consecutivo], [NombreCategoria], [Descripcion], [Estado]) 
VALUES (2, N'Ropa', N'Prendas de vestir y accesorios', 1)
GO
INSERT [dbo].[tbCategoria] ([Consecutivo], [NombreCategoria], [Descripcion], [Estado]) 
VALUES (3, N'Hogar', N'Artículos para el hogar y decoración', 1)
GO
SET IDENTITY_INSERT [dbo].[tbCategoria] OFF
GO

-- Productos de ejemplo
SET IDENTITY_INSERT [dbo].[tbProducto] ON 
GO
INSERT [dbo].[tbProducto] ([Consecutivo], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [ConsecutivoCategoria], [Estado]) 
VALUES (1, N'Laptop HP Pavilion', N'Laptop con procesador Intel i5, 8GB RAM, 512GB SSD', 850.00, 10, N'laptop_hp.jpg', 1, 1)
GO
INSERT [dbo].[tbProducto] ([Consecutivo], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [ConsecutivoCategoria], [Estado]) 
VALUES (2, N'Smartphone Samsung Galaxy', N'Teléfono inteligente con pantalla 6.5", 128GB', 650.00, 15, N'samsung_galaxy.jpg', 1, 1)
GO
INSERT [dbo].[tbProducto] ([Consecutivo], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [ConsecutivoCategoria], [Estado]) 
VALUES (3, N'Camisa Polo', N'Camisa de algodón para caballero', 35.00, 50, N'camisa_polo.jpg', 2, 1)
GO
INSERT [dbo].[tbProducto] ([Consecutivo], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [ConsecutivoCategoria], [Estado]) 
VALUES (4, N'Juego de Sábanas', N'Juego de sábanas 100% algodón, tamaño queen', 45.00, 20, N'sabanas.jpg', 3, 1)
GO
SET IDENTITY_INSERT [dbo].[tbProducto] OFF
GO

-- ============================================
-- PROCEDIMIENTOS ALMACENADOS
-- ============================================

-- 1. Procedimientos de Usuario
GO
CREATE PROCEDURE [dbo].[spActualizarContrasenna]
    @Consecutivo     int,
    @Contrasenna     varchar(100),
    @IndicadorTemp   bit
AS
BEGIN
    UPDATE  dbo.tbUsuario
       SET  Contrasenna = @Contrasenna,
            UsaContrasennaTemp = @IndicadorTemp
     WHERE  Consecutivo = @Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spActualizarPerfil]
    @Consecutivo        int,
    @Nombre             varchar(250),
    @CorreoElectronico  varchar(100)
AS
BEGIN
    UPDATE  dbo.tbUsuario
       SET  Nombre = @Nombre,
            CorreoElectronico = @CorreoElectronico
     WHERE  Consecutivo = @Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spConsultarUsuario]
    @Consecutivo  int
AS
BEGIN
    SELECT  Consecutivo,Nombre,CorreoElectronico,Estado,UsaContrasennaTemp,FechaRegistro
    FROM    dbo.tbUsuario
    WHERE   Consecutivo = @Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spIniciarSesionUsuario]
    @CorreoElectronico  varchar(100),
    @Contrasenna        varchar(100)
AS
BEGIN
    SELECT  U.Consecutivo,U.Nombre,CorreoElectronico,Estado,UsaContrasennaTemp,
            Contrasenna,ConsecutivoRol,R.Nombre 'NombreRol',FechaRegistro
    FROM    dbo.tbUsuario U
    INNER JOIN dbo.tbRol R ON U.ConsecutivoRol = R.Consecutivo
    WHERE   CorreoElectronico = @CorreoElectronico
        AND Estado = 1
END
GO

CREATE PROCEDURE [dbo].[spRegistrarError]
    @Mensaje            varchar(max),
    @Lugar              varchar(50),
    @FechaHora          datetime,
    @ConsecutivoUsuario int
AS
BEGIN
    INSERT INTO dbo.tbError(Mensaje,Lugar,FechaHora,ConsecutivoUsuario)
    VALUES (@Mensaje,@Lugar,@FechaHora,@ConsecutivoUsuario)
END
GO

CREATE PROCEDURE [dbo].[spRegistrarUsuario]
    @Nombre             varchar(250),
    @CorreoElectronico  varchar(100),
    @Contrasenna        varchar(100)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tbUsuario
                    WHERE CorreoElectronico = @CorreoElectronico)
    BEGIN
        DECLARE @Estado BIT = 1
        DECLARE @ContrasennaNOTemp BIT = 0
        DECLARE @Rol BIT = 1
        
        INSERT INTO dbo.tbUsuario(Nombre,CorreoElectronico,Contrasenna,Estado,UsaContrasennaTemp,ConsecutivoRol,FechaRegistro)
        VALUES(@Nombre,@CorreoElectronico,@Contrasenna,@Estado,@ContrasennaNOTemp,@Rol,GETDATE())
    END
END
GO

CREATE PROCEDURE [dbo].[spValidarCorreo]
    @CorreoElectronico  varchar(100)
AS
BEGIN
    SELECT  Consecutivo,Nombre,CorreoElectronico,Estado,UsaContrasennaTemp,FechaRegistro
    FROM    dbo.tbUsuario
    WHERE   CorreoElectronico = @CorreoElectronico
        AND Estado = 1
END
GO

-- 2. Procedimientos de Categorías
CREATE PROCEDURE [dbo].[spAgregarCategoria]
    @NombreCategoria varchar(100),
    @Descripcion text = NULL
AS
BEGIN
    INSERT INTO dbo.tbCategoria(NombreCategoria, Descripcion, Estado)
    VALUES (@NombreCategoria, @Descripcion, 1)
    
    SELECT SCOPE_IDENTITY() AS Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spListarCategorias]
AS
BEGIN
    SELECT Consecutivo, NombreCategoria, Descripcion, Estado
    FROM dbo.tbCategoria
    WHERE Estado = 1
    ORDER BY NombreCategoria
END
GO

CREATE PROCEDURE [dbo].[spActualizarCategoria]
    @Consecutivo int,
    @NombreCategoria varchar(100),
    @Descripcion text = NULL,
    @Estado bit
AS
BEGIN
    UPDATE dbo.tbCategoria
    SET NombreCategoria = @NombreCategoria,
        Descripcion = @Descripcion,
        Estado = @Estado
    WHERE Consecutivo = @Consecutivo
END
GO

-- 3. Procedimientos de Productos
CREATE PROCEDURE [dbo].[spAgregarProducto]
    @Nombre varchar(150),
    @Descripcion text = NULL,
    @Precio decimal(10,2),
    @Stock int,
    @Imagen varchar(255) = NULL,
    @ConsecutivoCategoria int
AS
BEGIN
    INSERT INTO dbo.tbProducto(Nombre, Descripcion, Precio, Stock, Imagen, ConsecutivoCategoria, Estado)
    VALUES (@Nombre, @Descripcion, @Precio, @Stock, @Imagen, @ConsecutivoCategoria, 1)
    
    SELECT SCOPE_IDENTITY() AS Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spListarProductos]
AS
BEGIN
    SELECT P.Consecutivo, P.Nombre, P.Descripcion, P.Precio, P.Stock, 
           P.Imagen, P.ConsecutivoCategoria, C.NombreCategoria, P.Estado
    FROM dbo.tbProducto P
    INNER JOIN dbo.tbCategoria C ON P.ConsecutivoCategoria = C.Consecutivo
    ORDER BY P.Nombre
END
GO

CREATE PROCEDURE [dbo].[spGuardarProducto]
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

CREATE PROCEDURE [dbo].[spCambiarEstadoProducto]
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

CREATE PROCEDURE [dbo].[spObtenerProducto]
    @Consecutivo int
AS
BEGIN
    SELECT P.Consecutivo, P.Nombre, P.Descripcion, P.Precio, P.Stock, 
           P.Imagen, P.ConsecutivoCategoria, C.NombreCategoria, P.Estado
    FROM dbo.tbProducto P
    INNER JOIN dbo.tbCategoria C ON P.ConsecutivoCategoria = C.Consecutivo
    WHERE P.Consecutivo = @Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spActualizarProducto]
    @Consecutivo int,
    @Nombre varchar(150),
    @Descripcion text = NULL,
    @Precio decimal(10,2),
    @Stock int,
    @Imagen varchar(255) = NULL,
    @ConsecutivoCategoria int,
    @Estado bit
AS
BEGIN
    UPDATE dbo.tbProducto
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        Precio = @Precio,
        Stock = @Stock,
        Imagen = @Imagen,
        ConsecutivoCategoria = @ConsecutivoCategoria,
        Estado = @Estado
    WHERE Consecutivo = @Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spActualizarStock]
    @ConsecutivoProducto int,
    @Cantidad int
AS
BEGIN
    UPDATE dbo.tbProducto
    SET Stock = Stock - @Cantidad
    WHERE Consecutivo = @ConsecutivoProducto
      AND Stock >= @Cantidad
    
    SELECT @@ROWCOUNT AS FilasAfectadas
END
GO

CREATE PROCEDURE [dbo].[spProductosPorCategoria]
    @ConsecutivoCategoria int
AS
BEGIN
    SELECT Consecutivo, Nombre, Descripcion, Precio, Stock, Imagen, Estado
    FROM dbo.tbProducto
    WHERE ConsecutivoCategoria = @ConsecutivoCategoria
      AND Estado = 1
    ORDER BY Nombre
END
GO

-- 4. Resumen del panel de administración
CREATE PROCEDURE [dbo].[spResumenAdmin]
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

-- 5. Procedimientos de Carritos
CREATE PROCEDURE [dbo].[spObtenerCarritoUsuario]
    @ConsecutivoUsuario int
AS
BEGIN
    DECLARE @ConsecutivoCarrito int
    
    SELECT TOP 1 @ConsecutivoCarrito = Consecutivo
    FROM dbo.tbCarrito
    WHERE ConsecutivoUsuario = @ConsecutivoUsuario
    
    IF @ConsecutivoCarrito IS NULL
    BEGIN
        INSERT INTO dbo.tbCarrito(ConsecutivoUsuario, FechaCreacion)
        VALUES (@ConsecutivoUsuario, GETDATE())
        
        SET @ConsecutivoCarrito = SCOPE_IDENTITY()
    END
    
    SELECT @ConsecutivoCarrito AS ConsecutivoCarrito
END
GO

CREATE PROCEDURE [dbo].[spAgregarProductoCarrito]
    @ConsecutivoCarrito int,
    @ConsecutivoProducto int,
    @Cantidad int,
    @PrecioUnitario decimal(10,2)
AS
BEGIN
    DECLARE @Subtotal decimal(10,2) = @Cantidad * @PrecioUnitario
    
    IF EXISTS (SELECT 1 FROM dbo.tbDetalleCarrito 
               WHERE ConsecutivoCarrito = @ConsecutivoCarrito 
                 AND ConsecutivoProducto = @ConsecutivoProducto)
    BEGIN
        UPDATE dbo.tbDetalleCarrito
        SET Cantidad = Cantidad + @Cantidad,
            Subtotal = (Cantidad + @Cantidad) * PrecioUnitario
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito 
          AND ConsecutivoProducto = @ConsecutivoProducto
    END
    ELSE
    BEGIN
        INSERT INTO dbo.tbDetalleCarrito(ConsecutivoCarrito, ConsecutivoProducto, Cantidad, PrecioUnitario, Subtotal)
        VALUES (@ConsecutivoCarrito, @ConsecutivoProducto, @Cantidad, @PrecioUnitario, @Subtotal)
    END
END
GO

CREATE PROCEDURE [dbo].[spListarCarrito]
    @ConsecutivoCarrito int
AS
BEGIN
    SELECT DC.Consecutivo, DC.ConsecutivoProducto, P.Nombre AS NombreProducto,
           DC.Cantidad, DC.PrecioUnitario, DC.Subtotal
    FROM dbo.tbDetalleCarrito DC
    INNER JOIN dbo.tbProducto P ON DC.ConsecutivoProducto = P.Consecutivo
    WHERE DC.ConsecutivoCarrito = @ConsecutivoCarrito
    ORDER BY DC.Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spEliminarProductoCarrito]
    @ConsecutivoDetalle int
AS
BEGIN
    DELETE FROM dbo.tbDetalleCarrito
    WHERE Consecutivo = @ConsecutivoDetalle
END
GO

CREATE PROCEDURE [dbo].[spVaciarCarrito]
    @ConsecutivoCarrito int
AS
BEGIN
    DELETE FROM dbo.tbDetalleCarrito
    WHERE ConsecutivoCarrito = @ConsecutivoCarrito
END
GO

-- 6. Procedimientos de Pedidos
CREATE PROCEDURE [dbo].[spCrearPedidoDesdeCarrito]
    @ConsecutivoCarrito int,
    @ConsecutivoUsuario int
AS
BEGIN
    BEGIN TRANSACTION
    
    BEGIN TRY
        DECLARE @Total decimal(10,2)
        DECLARE @ConsecutivoPedido int
        
        SELECT @Total = SUM(Subtotal)
        FROM dbo.tbDetalleCarrito
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito
        
        IF @Total IS NULL OR @Total = 0
        BEGIN
            RAISERROR('El carrito está vacío', 16, 1)
            ROLLBACK TRANSACTION
            RETURN
        END
        
        INSERT INTO dbo.tbPedido(ConsecutivoUsuario, FechaPedido, Total, Estado)
        VALUES (@ConsecutivoUsuario, GETDATE(), @Total, 'Pendiente')
        
        SET @ConsecutivoPedido = SCOPE_IDENTITY()
        
        INSERT INTO dbo.tbDetallePedido(ConsecutivoPedido, ConsecutivoProducto, Cantidad, PrecioUnitario, Subtotal)
        SELECT @ConsecutivoPedido, ConsecutivoProducto, Cantidad, PrecioUnitario, Subtotal
        FROM dbo.tbDetalleCarrito
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito
        
        UPDATE P
        SET P.Stock = P.Stock - DC.Cantidad
        FROM dbo.tbProducto P
        INNER JOIN dbo.tbDetalleCarrito DC ON P.Consecutivo = DC.ConsecutivoProducto
        WHERE DC.ConsecutivoCarrito = @ConsecutivoCarrito
        
        DELETE FROM dbo.tbDetalleCarrito
        WHERE ConsecutivoCarrito = @ConsecutivoCarrito
        
        COMMIT TRANSACTION
        
        SELECT @ConsecutivoPedido AS ConsecutivoPedido
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[spListarPedidosUsuario]
    @ConsecutivoUsuario int
AS
BEGIN
    SELECT P.Consecutivo, P.FechaPedido, P.Total, P.Estado,
           COUNT(DP.Consecutivo) AS CantidadProductos
    FROM dbo.tbPedido P
    LEFT JOIN dbo.tbDetallePedido DP ON P.Consecutivo = DP.ConsecutivoPedido
    WHERE P.ConsecutivoUsuario = @ConsecutivoUsuario
    GROUP BY P.Consecutivo, P.FechaPedido, P.Total, P.Estado
    ORDER BY P.FechaPedido DESC
END
GO

CREATE PROCEDURE [dbo].[spDetallePedido]
    @ConsecutivoPedido int
AS
BEGIN
    SELECT DP.Consecutivo, DP.ConsecutivoProducto, P.Nombre AS NombreProducto,
           DP.Cantidad, DP.PrecioUnitario, DP.Subtotal
    FROM dbo.tbDetallePedido DP
    INNER JOIN dbo.tbProducto P ON DP.ConsecutivoProducto = P.Consecutivo
    WHERE DP.ConsecutivoPedido = @ConsecutivoPedido
    ORDER BY DP.Consecutivo
END
GO

CREATE PROCEDURE [dbo].[spListarTodosPedidos]
AS
BEGIN
    SELECT P.Consecutivo, U.Nombre AS NombreUsuario, P.FechaPedido, 
           P.Total, P.Estado
    FROM dbo.tbPedido P
    INNER JOIN dbo.tbUsuario U ON P.ConsecutivoUsuario = U.Consecutivo
    ORDER BY P.FechaPedido DESC
END
GO

CREATE PROCEDURE [dbo].[spActualizarEstadoPedido]
    @ConsecutivoPedido int,
    @Estado varchar(30)
AS
BEGIN
    UPDATE dbo.tbPedido
    SET Estado = @Estado
    WHERE Consecutivo = @ConsecutivoPedido
END
GO

-- 7. Procedimientos de Pagos
CREATE PROCEDURE [dbo].[spRegistrarPago]
    @ConsecutivoPedido int,
    @MetodoPago varchar(50),
    @Monto decimal(10,2)
AS
BEGIN
    BEGIN TRANSACTION
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.tbPedido 
                       WHERE Consecutivo = @ConsecutivoPedido 
                         AND Estado = 'Pendiente')
        BEGIN
            RAISERROR('El pedido no existe o ya fue procesado', 16, 1)
            ROLLBACK TRANSACTION
            RETURN
        END
        
        INSERT INTO dbo.tbPago(ConsecutivoPedido, MetodoPago, Monto, FechaPago, EstadoPago)
        VALUES (@ConsecutivoPedido, @MetodoPago, @Monto, GETDATE(), 'Completado')
        
        UPDATE dbo.tbPedido
        SET Estado = 'Pagado'
        WHERE Consecutivo = @ConsecutivoPedido
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[spObtenerPagosPedido]
    @ConsecutivoPedido int
AS
BEGIN
    SELECT Consecutivo, MetodoPago, Monto, FechaPago, EstadoPago
    FROM dbo.tbPago
    WHERE ConsecutivoPedido = @ConsecutivoPedido
    ORDER BY FechaPago DESC
END
GO

CREATE PROCEDURE [dbo].[spListarPagos]
AS
BEGIN
    SELECT P.Consecutivo, U.Nombre AS NombreUsuario, PD.FechaPedido,
           P.MetodoPago, P.Monto, P.FechaPago, P.EstadoPago
    FROM dbo.tbPago P
    INNER JOIN dbo.tbPedido PD ON P.ConsecutivoPedido = PD.Consecutivo
    INNER JOIN dbo.tbUsuario U ON PD.ConsecutivoUsuario = U.Consecutivo
    ORDER BY P.FechaPago DESC
END
GO

-- ============================================
-- VISTAS ÚTILES (OPCIONALES)
-- ============================================

CREATE VIEW [dbo].[vwResumenUsuario]
AS
SELECT U.Consecutivo, U.Nombre, U.CorreoElectronico, R.Nombre AS Rol,
       COUNT(DISTINCT P.Consecutivo) AS TotalPedidos,
       ISNULL(SUM(P.Total), 0) AS TotalGastado
FROM dbo.tbUsuario U
INNER JOIN dbo.tbRol R ON U.ConsecutivoRol = R.Consecutivo
LEFT JOIN dbo.tbPedido P ON U.Consecutivo = P.ConsecutivoUsuario AND P.Estado = 'Pagado'
WHERE U.Estado = 1
GROUP BY U.Consecutivo, U.Nombre, U.CorreoElectronico, R.Nombre
GO

CREATE VIEW [dbo].[vwDetalleVentas]
AS
SELECT DP.ConsecutivoPedido, P.FechaPedido, U.Nombre AS Cliente,
       PR.Nombre AS Producto, DP.Cantidad, DP.PrecioUnitario, DP.Subtotal
FROM dbo.tbDetallePedido DP
INNER JOIN dbo.tbPedido P ON DP.ConsecutivoPedido = P.Consecutivo
INNER JOIN dbo.tbUsuario U ON P.ConsecutivoUsuario = U.Consecutivo
INNER JOIN dbo.tbProducto PR ON DP.ConsecutivoProducto = PR.Consecutivo
WHERE P.Estado = 'Pagado'
GO



USE Ministore_DB
GO

-- 1. Permitir que ConsecutivoUsuario sea NULL en tbError
--    (para poder registrar errores ocurridos en endpoints anónimos, como el registro de usuario,
--     donde todavía no existe un usuario autenticado)
ALTER TABLE [dbo].[tbError] ALTER COLUMN [ConsecutivoUsuario] [int] NULL
GO

-- 2. Actualizar spRegistrarError para que el parámetro sea opcional / acepte NULL
CREATE OR ALTER PROCEDURE [dbo].[spRegistrarError]
    @Mensaje            varchar(max),
    @Lugar              varchar(50),
    @FechaHora          datetime,
    @ConsecutivoUsuario int = NULL
AS
BEGIN
    INSERT INTO dbo.tbError(Mensaje,Lugar,FechaHora,ConsecutivoUsuario)
    VALUES (@Mensaje,@Lugar,@FechaHora,@ConsecutivoUsuario)
END
GO

EXEC spRegistrarUsuario
    @Nombre = 'Usuario de Prueba',
    @CorreoElectronico = 'prueba@ministore.com',
    @Contrasenna = '$2b$11$Rf1YaMg6w0tXBBK56vv0Yu929xom2cRIiH.c7Ednx/fDEOV9EsVBa'
