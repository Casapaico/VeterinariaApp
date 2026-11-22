-- =============================================================
-- Usuarios para la autenticación JWT (Laboratorio Semana 14)
-- Ejecutar DESPUÉS de VeterinariaDB.sql. Re-ejecutable.
-- =============================================================
USE VeterinariaDB;
GO

IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DROP TABLE Usuarios;
GO

CREATE TABLE Usuarios (
    UsuarioId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
    Correo        NVARCHAR(100) NOT NULL CONSTRAINT UQ_Usuarios_Correo UNIQUE,
    ClaveHash     VARCHAR(100)  NOT NULL,      -- hash BCrypt (60 caracteres), nunca la clave
    Rol           VARCHAR(20)   NOT NULL
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Asistente', 'Recepcionista', 'Veterinario')),
    FechaRegistro DATETIME2(0)  NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (SYSDATETIME())
);
GO

-- Un usuario de prueba por rol. Clave de los tres: Vet2026*
-- Los hashes se obtuvieron registrándolos por POST /api/auth/registro (rol Asistente)
-- y luego se cambió el rol con UPDATE: la API nunca deja elegir el rol.
INSERT INTO Usuarios (Correo, ClaveHash, Rol) VALUES
('asistente@vetarequipa.com',   '$2a$11$sXtyl/uqnMo5D9mlmelEKO6LdwgwggEOfcKB119REJ8ePSAro9ku.', 'Asistente'),
('recepcion@vetarequipa.com',   '$2a$11$7/f4EJhS2N9A6sJKCgpjJe0St142yyCik/ISv8Hfet/Z21ZDhoise', 'Recepcionista'),
('veterinario@vetarequipa.com', '$2a$11$dwl8zqwbjGFfti7YZT4Hh.W4oj5NGGxiDyasxRJJgQ2yBpSEeJWP.', 'Veterinario');
GO

-- Para dar otro rol a una cuenta registrada por la API:
-- UPDATE Usuarios SET Rol = 'Recepcionista' WHERE Correo = 'nuevo@vetarequipa.com';

SELECT UsuarioId, Correo, Rol FROM Usuarios;
GO
