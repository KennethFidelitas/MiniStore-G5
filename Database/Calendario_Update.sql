USE [Ministore_DB]
GO

IF OBJECT_ID(N'[dbo].[tbEventoCalendario]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[tbEventoCalendario](
        [Consecutivo] [int] IDENTITY(1,1) NOT NULL,
        [Titulo] [varchar](150) NOT NULL,
        [Descripcion] [varchar](500) NULL,
        [FechaInicio] [datetime] NOT NULL,
        [FechaFin] [datetime] NOT NULL,
        [ConsecutivoUsuario] [int] NOT NULL,
        [TipoEvento] [varchar](30) NOT NULL
            CONSTRAINT [DF_tbEventoCalendario_TipoEvento] DEFAULT 'Personal',
        [ConsecutivoPedido] [int] NULL,
        CONSTRAINT [PK_tbEventoCalendario] PRIMARY KEY CLUSTERED ([Consecutivo] ASC),
        CONSTRAINT [CK_tbEventoCalendario_Fechas] CHECK ([FechaFin] >= [FechaInicio]),
        CONSTRAINT [FK_tbEventoCalendario_tbUsuario] FOREIGN KEY([ConsecutivoUsuario])
            REFERENCES [dbo].[tbUsuario] ([Consecutivo]),
        CONSTRAINT [FK_tbEventoCalendario_tbPedido] FOREIGN KEY([ConsecutivoPedido])
            REFERENCES [dbo].[tbPedido] ([Consecutivo])
    )
END
GO

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
