-- =============================================================
-- VeterinariaDB: clínica veterinaria (Propietarios 1:N Mascotas)
-- Laboratorio Semana 13 - API en capas con DTOs y AutoMapper
-- Script re-ejecutable: elimina y vuelve a crear la base de datos.
-- =============================================================
USE master;
GO

IF DB_ID('VeterinariaDB') IS NOT NULL
BEGIN
    ALTER DATABASE VeterinariaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VeterinariaDB;
END
GO

CREATE DATABASE VeterinariaDB;
GO

USE VeterinariaDB;
GO

CREATE TABLE Propietarios (
    PropietarioId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Propietarios PRIMARY KEY,
    Dni            CHAR(8)       NOT NULL CONSTRAINT UQ_Propietarios_Dni UNIQUE,
    Nombres        NVARCHAR(60)  NOT NULL,
    Apellidos      NVARCHAR(60)  NOT NULL,
    Telefono       VARCHAR(15)   NOT NULL,
    Correo         NVARCHAR(100) NULL,
    Direccion      NVARCHAR(150) NULL,
    FechaRegistro  DATETIME2(0)  NOT NULL CONSTRAINT DF_Propietarios_FechaRegistro DEFAULT (SYSDATETIME())
);
GO

CREATE TABLE Mascotas (
    MascotaId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mascotas PRIMARY KEY,
    Nombre          NVARCHAR(50)  NOT NULL,
    Especie         NVARCHAR(30)  NOT NULL,
    Raza            NVARCHAR(50)  NULL,
    Sexo            CHAR(1)       NOT NULL CONSTRAINT CK_Mascotas_Sexo CHECK (Sexo IN ('M', 'H')),
    FechaNacimiento DATE          NULL,
    PesoKg          DECIMAL(5,2)  NOT NULL CONSTRAINT CK_Mascotas_Peso CHECK (PesoKg > 0),
    PropietarioId   INT           NOT NULL
        CONSTRAINT FK_Mascotas_Propietarios REFERENCES Propietarios (PropietarioId)
);
GO

CREATE INDEX IX_Mascotas_PropietarioId ON Mascotas (PropietarioId);
GO

-- Datos de ejemplo (inventados)
INSERT INTO Propietarios (Dni, Nombres, Apellidos, Telefono, Correo, Direccion, FechaRegistro) VALUES
('45127893', N'Lucía',     N'Ramírez Torres',  '987654321', 'lucia.ramirez@example.com',  N'Av. Los Álamos 123, Arequipa',  '2025-03-14 10:20:00'),
('70214566', N'Carlos',    N'Huamán Quispe',   '956321478', 'carlos.huaman@example.com',  N'Calle Mercaderes 450, Arequipa','2025-05-02 16:05:00'),
('41896320', N'Mariana',   N'Flores Benavides','912345678', 'mariana.flores@example.com', N'Urb. La Campiña B-7, Arequipa', '2025-07-21 09:45:00'),
('72458190', N'Jorge',     N'Paredes Salas',   '934567812', NULL,                         N'Jr. Ayacucho 210, Arequipa',    '2025-09-10 11:30:00'),
('46673021', N'Valeria',   N'Chávez Medina',   '978123456', 'valeria.chavez@example.com', NULL,                             '2026-01-18 15:10:00'),
('73320914', N'Andrés',    N'Gutiérrez Rojas', '945612378', 'andres.gutierrez@example.com', N'Av. Ejército 812, Arequipa',  '2026-04-03 08:55:00');
GO

-- Lucía tiene 5 mascotas: el máximo permitido por la regla de negocio.
-- Andrés no tiene mascotas: es el que se puede eliminar sin conflicto.
INSERT INTO Mascotas (Nombre, Especie, Raza, Sexo, FechaNacimiento, PesoKg, PropietarioId) VALUES
(N'Toby',    N'Perro', N'Labrador',          'M', '2019-06-12', 28.40, 1),
(N'Misha',   N'Gato',  N'Siamés',            'H', '2021-02-03',  4.10, 1),
(N'Rocky',   N'Perro', N'Bulldog francés',   'M', '2022-11-20', 12.30, 1),
(N'Kiwi',    N'Ave',   N'Periquito',         'H', '2024-04-15',  0.05, 1),
(N'Pelusa',  N'Conejo',N'Cabeza de león',    'H', '2023-08-01',  1.60, 1),
(N'Max',     N'Perro', N'Pastor alemán',     'M', '2020-01-25', 34.00, 2),
(N'Luna',    N'Gato',  N'Mestizo',           'H', '2022-07-09',  3.80, 2),
(N'Coco',    N'Perro', N'Shih Tzu',          'M', '2018-12-30',  6.20, 3),
(N'Nala',    N'Gato',  N'Persa',             'H', '2023-03-17',  3.50, 3),
(N'Bruno',   N'Perro', N'Mestizo',           'M', NULL,         18.75, 4),
(N'Simba',   N'Gato',  N'Mestizo',           'M', '2025-05-05',  2.90, 5),
(N'Lola',    N'Perro', N'Beagle',            'H', '2021-09-14', 11.10, 5);
GO

SELECT 'Propietarios' AS Tabla, COUNT(*) AS Filas FROM Propietarios
UNION ALL
SELECT 'Mascotas', COUNT(*) FROM Mascotas;
GO
