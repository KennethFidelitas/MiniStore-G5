# Estándar de programación de MiniStore

Este estándar formaliza las convenciones aplicadas en MiniStore y conserva la metodología utilizada en el proyecto de referencia `RepoJN`.

## Arquitectura

- La aplicación se divide en `MiniStore_WEB` (MVC/Razor) y `MiniStore_API` (Web API).
- Los controladores MVC consumen únicamente endpoints de la API mediante `IHttpClientFactory`.
- La API accede a SQL Server con Dapper y procedimientos almacenados; no se escriben consultas SQL en los controladores.
- Los servicios se registran mediante interfaces e inyección de dependencias.
- La sesión se administra en MVC y la autorización de la API se realiza con JWT.

## Nombres y organización

- Clases, propiedades y métodos usan `PascalCase`.
- Parámetros y variables locales usan `camelCase`.
- Las interfaces comienzan con `I`.
- Los controladores terminan en `Controller`; los modelos de entrada y respuesta terminan en `RequestModel` y `ResponseModel`.
- Los endpoints públicos de la API terminan en `API`, siguiendo el formato utilizado por el curso.
- Los procedimientos almacenados comienzan con `sp` y expresan una acción, por ejemplo `spGuardarEventoCalendario`.

## Métodos y validación

- Cada método realiza una sola operación del flujo y retorna temprano ante errores.
- Los modelos de entrada declaran validaciones con `DataAnnotations`.
- Los controladores MVC validan `ModelState` antes de consumir la API.
- La API obtiene la identidad del usuario desde los claims del JWT; no confía en identificadores enviados por el navegador.
- Las operaciones que modifican datos desde formularios Razor utilizan antiforgery tokens.

## Excepciones y mensajes

- Las excepciones no se ocultan con bloques `catch` vacíos.
- La API delega las excepciones no controladas al middleware configurado y las registra en `tbError` mediante `spRegistrarError`.
- Los mensajes dirigidos al usuario deben ser claros y no deben revelar cadenas de conexión, secretos ni trazas internas.

## Comentarios y formato

- Los comentarios explican decisiones o reglas de negocio; no repiten literalmente el código.
- Se utiliza una indentación de cuatro espacios y una clase pública por archivo.
- Los archivos se guardan en UTF-8 y los textos visibles se redactan en español.
- Antes de integrar cambios debe compilarse la solución completa sin errores.
