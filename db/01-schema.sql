-- ===========================================================================
--  Pedidos · Esquema de base de datos (SQL Server 2022)
--  Equivalente a la migración inicial de EF Core (src/Pedidos.Infrastructure/
--  Persistence/Migrations). Puede aplicarse manualmente con:
--      sqlcmd -S localhost -U sa -P <password> -i db/01-schema.sql
-- ===========================================================================

IF DB_ID('Pedidos') IS NULL
BEGIN
    CREATE DATABASE [Pedidos];
END
GO

USE [Pedidos];
GO

-- ---------------------------------------------------------------------------
-- Pedidos
-- ---------------------------------------------------------------------------
IF OBJECT_ID('dbo.Pedidos', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Pedidos]
    (
        [Id]                INT            NOT NULL IDENTITY(1, 1),
        [NumeroPedido]      NVARCHAR(50)   NOT NULL,
        [Cliente]           NVARCHAR(150)  NOT NULL,
        [Fecha]             DATETIME2      NOT NULL,
        [Total]             DECIMAL(10, 2) NOT NULL,
        [Estado]            NVARCHAR(20)   NOT NULL,
        [EsEliminado]       BIT            NOT NULL CONSTRAINT [DF_Pedidos_EsEliminado] DEFAULT (0),
        [FechaEliminacion]  DATETIME2      NULL,
        CONSTRAINT [PK_Pedidos] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- Unicidad del número de pedido: incluye los pedidos eliminados lógicamente.
    CREATE UNIQUE INDEX [IX_Pedidos_NumeroPedido] ON [dbo].[Pedidos] ([NumeroPedido] ASC);
END
GO

-- ---------------------------------------------------------------------------
-- Usuarios
-- ---------------------------------------------------------------------------
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Usuarios]
    (
        [Id]            INT           NOT NULL IDENTITY(1, 1),
        [Email]         NVARCHAR(200) NOT NULL,
        [PasswordHash]  NVARCHAR(500) NOT NULL,
        [Rol]           NVARCHAR(20)  NOT NULL,
        [Activo]        BIT           NOT NULL CONSTRAINT [DF_Usuarios_Activo] DEFAULT (1),
        CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [dbo].[Usuarios] ([Email] ASC);
END
GO

PRINT 'Esquema Pedidos aplicado correctamente.';
GO
