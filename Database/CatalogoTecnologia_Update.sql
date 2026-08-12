USE [Ministore_DB]
GO

/*
    Actualiza el catálogo de demostración de MiniStore a productos tecnológicos.

    Características:
    - Es idempotente: puede ejecutarse más de una vez sin duplicar datos.
    - Conserva productos y pedidos históricos.
    - Desactiva únicamente los productos de ejemplo que no pertenecen al catálogo.
    - Las imágenes utilizadas existen en MiniStore_WEB/wwwroot/ministore/images.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Categorias TABLE
    (
        NombreCategoria varchar(100) NOT NULL,
        Descripcion varchar(500) NOT NULL
    );

    INSERT INTO @Categorias (NombreCategoria, Descripcion)
    VALUES
        ('Teléfonos', 'Smartphones y dispositivos móviles para comunicación, fotografía y entretenimiento.'),
        ('Computadoras', 'Equipos portátiles y de escritorio para estudio, trabajo y productividad.'),
        ('Relojes inteligentes', 'Dispositivos vestibles para actividad física, salud y notificaciones.'),
        ('Accesorios', 'Complementos tecnológicos para mejorar la experiencia de uso diario.');

    UPDATE C
       SET C.Descripcion = S.Descripcion,
           C.Estado = 1
      FROM dbo.tbCategoria C
      INNER JOIN @Categorias S
              ON S.NombreCategoria = C.NombreCategoria;

    INSERT INTO dbo.tbCategoria (NombreCategoria, Descripcion, Estado)
    SELECT S.NombreCategoria, S.Descripcion, 1
      FROM @Categorias S
     WHERE NOT EXISTS
           (
               SELECT 1
                 FROM dbo.tbCategoria C
                WHERE C.NombreCategoria = S.NombreCategoria
           );

    DECLARE @Telefonos int =
        (SELECT TOP (1) Consecutivo FROM dbo.tbCategoria WHERE NombreCategoria = 'Teléfonos');
    DECLARE @Computadoras int =
        (SELECT TOP (1) Consecutivo FROM dbo.tbCategoria WHERE NombreCategoria = 'Computadoras');
    DECLARE @Relojes int =
        (SELECT TOP (1) Consecutivo FROM dbo.tbCategoria WHERE NombreCategoria = 'Relojes inteligentes');
    DECLARE @Accesorios int =
        (SELECT TOP (1) Consecutivo FROM dbo.tbCategoria WHERE NombreCategoria = 'Accesorios');

    IF @Telefonos IS NULL OR @Computadoras IS NULL OR @Relojes IS NULL OR @Accesorios IS NULL
        THROW 51000, 'No fue posible preparar las categorías del catálogo tecnológico.', 1;

    DECLARE @Productos TABLE
    (
        Nombre varchar(150) NOT NULL,
        Descripcion varchar(500) NOT NULL,
        Precio decimal(10,2) NOT NULL,
        Stock int NOT NULL,
        Imagen varchar(255) NOT NULL,
        ConsecutivoCategoria int NOT NULL
    );

    INSERT INTO @Productos
        (Nombre, Descripcion, Precio, Stock, Imagen, ConsecutivoCategoria)
    VALUES
        ('Smartphone Nova X1 128 GB',
         'Pantalla OLED de 6.1 pulgadas, cámara dual, 128 GB de almacenamiento y reconocimiento facial.',
         289900.00, 18, 'product-item1.jpg', @Telefonos),

        ('Smartphone Vision 5G 256 GB',
         'Conectividad 5G, pantalla de 6.5 pulgadas, batería de larga duración y 256 GB de almacenamiento.',
         239900.00, 22, 'product-item2.jpg', @Telefonos),

        ('Smartphone Edge OLED 128 GB',
         'Diseño compacto, pantalla OLED de alto contraste, cámara nocturna y carga rápida.',
         329900.00, 14, 'product-item3.jpg', @Telefonos),

        ('Smartphone Pro Camera 256 GB',
         'Sistema de cámara triple, video 4K, acabado grafito y procesador de alto rendimiento.',
         449900.00, 10, 'product-item4.jpg', @Telefonos),

        ('Smartphone Ocean 5G 256 GB',
         'Pantalla de alta frecuencia, conectividad 5G, protección contra salpicaduras y carga inalámbrica.',
         379900.00, 12, 'product-item5.jpg', @Telefonos),

        ('Laptop NovaBook 14',
         'Laptop de 14 pulgadas con procesador de ocho núcleos, 16 GB de RAM y SSD de 512 GB.',
         549900.00, 9, 'product-laptop-silver.png', @Computadoras),

        ('Computadora CoreStation 24',
         'Equipo todo en uno de 24 pulgadas, 16 GB de RAM, SSD de 1 TB, teclado y mouse inalámbricos.',
         649900.00, 6, 'product-desktop-aio.png', @Computadoras),

        ('Smartwatch Rose Fit',
         'Reloj inteligente con monitoreo de actividad, frecuencia cardíaca y notificaciones.',
         89900.00, 20, 'product-item6.jpg', @Relojes),

        ('Smartwatch Active S',
         'Pantalla táctil, modos deportivos, control multimedia y resistencia al agua.',
         119900.00, 16, 'product-item7.jpg', @Relojes),

        ('Smartwatch Sport GPS',
         'GPS integrado, medición de entrenamientos, cronómetro y correa deportiva.',
         109900.00, 17, 'product-item8.jpg', @Relojes),

        ('Smartwatch Classic AMOLED',
         'Pantalla AMOLED, llamadas Bluetooth, seguimiento del sueño y diseño clásico.',
         139900.00, 11, 'product-item9.jpg', @Relojes),

        ('Smartwatch Midnight Pro',
         'Diseño negro minimalista, pantalla siempre activa y batería de hasta cinco días.',
         159900.00, 8, 'product-item10.jpg', @Relojes),

        ('Kit inalámbrico Connect Pro',
         'Incluye audífonos de diadema, mouse inalámbrico y batería portátil en acabado grafito.',
         74900.00, 25, 'product-accessory-kit.png', @Accesorios);

    UPDATE P
       SET P.Descripcion = S.Descripcion,
           P.Precio = S.Precio,
           P.Stock = S.Stock,
           P.Imagen = S.Imagen,
           P.ConsecutivoCategoria = S.ConsecutivoCategoria,
           P.Estado = 1
      FROM dbo.tbProducto P
      INNER JOIN @Productos S
              ON S.Nombre = P.Nombre
              OR S.Imagen = P.Imagen;

    INSERT INTO dbo.tbProducto
        (Nombre, Descripcion, Precio, Stock, Imagen, ConsecutivoCategoria, Estado)
    SELECT S.Nombre,
           S.Descripcion,
           S.Precio,
           S.Stock,
           S.Imagen,
           S.ConsecutivoCategoria,
           1
      FROM @Productos S
     WHERE NOT EXISTS
           (
               SELECT 1
                 FROM dbo.tbProducto P
                WHERE P.Nombre = S.Nombre
                   OR P.Imagen = S.Imagen
           );

    UPDATE dbo.tbProducto
       SET Estado = 0
     WHERE Imagen IN
           (
               'laptop_hp.jpg',
               'samsung_galaxy.jpg',
               'camisa_polo.jpg',
               'sabanas.jpg'
           );

    UPDATE dbo.tbCategoria
       SET Estado = 0
     WHERE NOT EXISTS
           (
               SELECT 1
                 FROM dbo.tbProducto P
                WHERE P.ConsecutivoCategoria = tbCategoria.Consecutivo
                  AND P.Estado = 1
           );

    COMMIT TRANSACTION;

    SELECT C.NombreCategoria AS Categoria,
           COUNT(*) AS ProductosActivos
      FROM dbo.tbProducto P
      INNER JOIN dbo.tbCategoria C
              ON C.Consecutivo = P.ConsecutivoCategoria
     WHERE P.Estado = 1
       AND C.Estado = 1
     GROUP BY C.NombreCategoria
     ORDER BY C.NombreCategoria;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
