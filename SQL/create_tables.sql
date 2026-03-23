-- ============================================================
--  BookRadar - Script de creacion de base de datos
-- ============================================================

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BookRadarDB')
BEGIN
    CREATE DATABASE BookRadarDB;
END
GO

USE BookRadarDB;
GO

-- ============================================================
--  Tabla principal
-- ============================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialBusquedas' AND xtype='U')
BEGIN
    CREATE TABLE HistorialBusquedas (
        Id              INT             IDENTITY(1,1)   NOT NULL,
        Autor           NVARCHAR(200)   NOT NULL,
        Titulo          NVARCHAR(500)   NOT NULL,
        AnioPublicacion INT             NULL,
        Editorial       NVARCHAR(300)   NULL,
        FechaConsulta   DATETIME        NOT NULL CONSTRAINT DF_FechaConsulta DEFAULT GETDATE(),

        CONSTRAINT PK_HistorialBusquedas PRIMARY KEY CLUSTERED (Id ASC)
    );

    -- Indice para acelerar busquedas por autor y fecha
    CREATE NONCLUSTERED INDEX IX_HistorialBusquedas_Autor_Fecha
        ON HistorialBusquedas (Autor, FechaConsulta DESC);

    PRINT 'Tabla HistorialBusquedas creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla HistorialBusquedas ya existe.';
END
GO

-- ============================================================
--  Stored Procedure: insertar un libro en el historial
--  Incluye logica anti-duplicado: no inserta si el mismo autor
--  fue buscado hace menos de 1 minuto
-- ============================================================
CREATE OR ALTER PROCEDURE SP_InsertarLibro
    @Autor           NVARCHAR(200),
    @Titulo          NVARCHAR(500),
    @AnioPublicacion INT          = NULL,
    @Editorial       NVARCHAR(300) = NULL,
    @Insertado       BIT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar si existe una busqueda del mismo autor en el ultimo minuto
    DECLARE @UltimaConsulta DATETIME;

    SELECT TOP 1 @UltimaConsulta = FechaConsulta
    FROM HistorialBusquedas
    WHERE Autor = @Autor
    ORDER BY FechaConsulta DESC;

    IF @UltimaConsulta IS NOT NULL AND DATEDIFF(SECOND, @UltimaConsulta, GETDATE()) < 60
    BEGIN
        -- Busqueda duplicada en menos de 1 minuto, no se inserta
        SET @Insertado = 0;
        RETURN;
    END

    -- Insertar el registro
    INSERT INTO HistorialBusquedas (Autor, Titulo, AnioPublicacion, Editorial, FechaConsulta)
    VALUES (@Autor, @Titulo, @AnioPublicacion, @Editorial, GETDATE());

    SET @Insertado = 1;
END
GO

-- ============================================================
--  Stored Procedure: obtener todo el historial ordenado
-- ============================================================
CREATE OR ALTER PROCEDURE SP_ObtenerHistorial
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Autor, Titulo, AnioPublicacion, Editorial, FechaConsulta
    FROM HistorialBusquedas
    ORDER BY FechaConsulta DESC;
END
GO

PRINT 'Script ejecutado correctamente. Base de datos BookRadarDB lista.';
GO
