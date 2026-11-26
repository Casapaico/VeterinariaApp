-- =============================================================
-- VeterinariaDB para PostgreSQL (Neon) - Laboratorio Semana 15
-- Mismo modelo que db/VeterinariaDB.sql + db/Usuarios.sql (SQL Server),
-- adaptado a PostgreSQL. Re-ejecutable: borra y vuelve a crear las tablas.
--   psql "$NEON_URL" -f db/postgres/VeterinariaDB-postgres.sql
--
-- Cambios respecto a SQL Server:
--   IDENTITY(1,1)        -> GENERATED ALWAYS AS IDENTITY
--   NVARCHAR / VARCHAR   -> VARCHAR (PostgreSQL guarda todo en UTF-8)
--   DATETIME2(0)         -> TIMESTAMP(0)
--   SYSDATETIME()        -> LOCALTIMESTAMP(0)
--   Nombres entre comillas dobles: EF Core los busca respetando mayúsculas
-- =============================================================

DROP TABLE IF EXISTS "Mascotas";
DROP TABLE IF EXISTS "Propietarios";
DROP TABLE IF EXISTS "Usuarios";

CREATE TABLE "Propietarios" (
    "PropietarioId" INT GENERATED ALWAYS AS IDENTITY CONSTRAINT "PK_Propietarios" PRIMARY KEY,
    "Dni"           CHAR(8)      NOT NULL CONSTRAINT "UQ_Propietarios_Dni" UNIQUE,
    "Nombres"       VARCHAR(60)  NOT NULL,
    "Apellidos"     VARCHAR(60)  NOT NULL,
    "Telefono"      VARCHAR(15)  NOT NULL,
    "Correo"        VARCHAR(100) NULL,
    "Direccion"     VARCHAR(150) NULL,
    "FechaRegistro" TIMESTAMP(0) NOT NULL DEFAULT LOCALTIMESTAMP(0)
);

CREATE TABLE "Mascotas" (
    "MascotaId"       INT GENERATED ALWAYS AS IDENTITY CONSTRAINT "PK_Mascotas" PRIMARY KEY,
    "Nombre"          VARCHAR(50)  NOT NULL,
    "Especie"         VARCHAR(30)  NOT NULL,
    "Raza"            VARCHAR(50)  NULL,
    "Sexo"            CHAR(1)      NOT NULL CONSTRAINT "CK_Mascotas_Sexo" CHECK ("Sexo" IN ('M', 'H')),
    "FechaNacimiento" DATE         NULL,
    "PesoKg"          NUMERIC(5,2) NOT NULL CONSTRAINT "CK_Mascotas_Peso" CHECK ("PesoKg" > 0),
    "PropietarioId"   INT          NOT NULL
        CONSTRAINT "FK_Mascotas_Propietarios" REFERENCES "Propietarios" ("PropietarioId")
);

CREATE INDEX "IX_Mascotas_PropietarioId" ON "Mascotas" ("PropietarioId");

CREATE TABLE "Usuarios" (
    "UsuarioId"     INT GENERATED ALWAYS AS IDENTITY CONSTRAINT "PK_Usuarios" PRIMARY KEY,
    "Correo"        VARCHAR(100) NOT NULL CONSTRAINT "UQ_Usuarios_Correo" UNIQUE,
    "ClaveHash"     VARCHAR(100) NOT NULL,      -- hash BCrypt, nunca la clave
    "Rol"           VARCHAR(20)  NOT NULL
        CONSTRAINT "CK_Usuarios_Rol" CHECK ("Rol" IN ('Asistente', 'Recepcionista', 'Veterinario')),
    "FechaRegistro" TIMESTAMP(0) NOT NULL DEFAULT LOCALTIMESTAMP(0)
);

-- Datos de ejemplo (inventados), los mismos que en SQL Server
INSERT INTO "Propietarios" ("Dni", "Nombres", "Apellidos", "Telefono", "Correo", "Direccion", "FechaRegistro") VALUES
('45127893', 'Lucía',   'Ramírez Torres',   '987654321', 'lucia.ramirez@example.com',    'Av. Los Álamos 123, Arequipa',   '2025-03-14 10:20:00'),
('70214566', 'Carlos',  'Huamán Quispe',    '956321478', 'carlos.huaman@example.com',    'Calle Mercaderes 450, Arequipa', '2025-05-02 16:05:00'),
('41896320', 'Mariana', 'Flores Benavides', '912345678', 'mariana.flores@example.com',   'Urb. La Campiña B-7, Arequipa',  '2025-07-21 09:45:00'),
('72458190', 'Jorge',   'Paredes Salas',    '934567812', NULL,                           'Jr. Ayacucho 210, Arequipa',     '2025-09-10 11:30:00'),
('46673021', 'Valeria', 'Chávez Medina',    '978123456', 'valeria.chavez@example.com',   NULL,                             '2026-01-18 15:10:00'),
('73320914', 'Andrés',  'Gutiérrez Rojas',  '945612378', 'andres.gutierrez@example.com', 'Av. Ejército 812, Arequipa',     '2026-04-03 08:55:00');

-- Lucía tiene 5 mascotas (el máximo); Andrés no tiene mascotas (se puede eliminar)
INSERT INTO "Mascotas" ("Nombre", "Especie", "Raza", "Sexo", "FechaNacimiento", "PesoKg", "PropietarioId") VALUES
('Toby',   'Perro',  'Labrador',        'M', '2019-06-12', 28.40, 1),
('Misha',  'Gato',   'Siamés',          'H', '2021-02-03',  4.10, 1),
('Rocky',  'Perro',  'Bulldog francés', 'M', '2022-11-20', 12.30, 1),
('Kiwi',   'Ave',    'Periquito',       'H', '2024-04-15',  0.05, 1),
('Pelusa', 'Conejo', 'Cabeza de león',  'H', '2023-08-01',  1.60, 1),
('Max',    'Perro',  'Pastor alemán',   'M', '2020-01-25', 34.00, 2),
('Luna',   'Gato',   'Mestizo',         'H', '2022-07-09',  3.80, 2),
('Coco',   'Perro',  'Shih Tzu',        'M', '2018-12-30',  6.20, 3),
('Nala',   'Gato',   'Persa',           'H', '2023-03-17',  3.50, 3),
('Bruno',  'Perro',  'Mestizo',         'M', NULL,         18.75, 4),
('Simba',  'Gato',   'Mestizo',         'M', '2025-05-05',  2.90, 5),
('Lola',   'Perro',  'Beagle',          'H', '2021-09-14', 11.10, 5);

-- Un usuario de prueba por rol. Clave de los tres: Vet2026*
INSERT INTO "Usuarios" ("Correo", "ClaveHash", "Rol") VALUES
('asistente@vetarequipa.com',   '$2a$11$sXtyl/uqnMo5D9mlmelEKO6LdwgwggEOfcKB119REJ8ePSAro9ku.', 'Asistente'),
('recepcion@vetarequipa.com',   '$2a$11$7/f4EJhS2N9A6sJKCgpjJe0St142yyCik/ISv8Hfet/Z21ZDhoise', 'Recepcionista'),
('veterinario@vetarequipa.com', '$2a$11$dwl8zqwbjGFfti7YZT4Hh.W4oj5NGGxiDyasxRJJgQ2yBpSEeJWP.', 'Veterinario');

SELECT 'Propietarios' AS "Tabla", COUNT(*) AS "Filas" FROM "Propietarios"
UNION ALL SELECT 'Mascotas', COUNT(*) FROM "Mascotas"
UNION ALL SELECT 'Usuarios', COUNT(*) FROM "Usuarios";
