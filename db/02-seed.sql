-- ===========================================================================
--  Pedidos · Datos iniciales (SQL Server 2022)
--  Equivalente al seed de EF Core (Pedidos.Infrastructure/Seeding/DbSeeder).
--  Los hashes son PBKDF2 (SHA256, 100.000 iteraciones) generados con
--  PasswordHasher<Usuario> de ASP.NET Core Identity.
--
--  Credenciales de prueba:
--      user@email.com  / 123456    -> rol User
--      admin@email.com / Admin123! -> rol Admin
-- ===========================================================================

USE [Pedidos];
GO

DECLARE @Usuarios TABLE
(
    [Email]        NVARCHAR(200),
    [PasswordHash] NVARCHAR(500),
    [Rol]          NVARCHAR(20),
    [Activo]       BIT
);

INSERT INTO @Usuarios ([Email], [PasswordHash], [Rol], [Activo])
VALUES
    (N'user@email.com',
     N'AQAAAAIAAYagAAAAELXbjZZ6Y6HgJ0q5Fu5oCueNaI+ziThOCjgVT2VxrnK3KVp1PgIIzv9Qzd6hPYBDVQ==',
     N'User',
     1),
    (N'admin@email.com',
     N'AQAAAAIAAYagAAAAENsz/YLe3jU+FEN9v4NOsVvEOaIGCUllCmWU8aJUjRybRsThrw9PxVnNJEbOB+hH9w==',
     N'Admin',
     1);

MERGE [dbo].[Usuarios] AS destino
USING @Usuarios AS origen
    ON destino.[Email] = origen.[Email]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Email], [PasswordHash], [Rol], [Activo])
    VALUES (origen.[Email], origen.[PasswordHash], origen.[Rol], origen.[Activo]);

PRINT 'Seed de usuarios aplicado correctamente.';
GO
