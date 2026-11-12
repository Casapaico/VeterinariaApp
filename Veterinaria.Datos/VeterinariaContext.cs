using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Veterinaria.Entidades;

namespace Veterinaria.Datos;

public partial class VeterinariaContext : DbContext
{
    public VeterinariaContext(DbContextOptions<VeterinariaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<Propietario> Propietarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasIndex(e => e.PropietarioId, "IX_Mascotas_PropietarioId");

            entity.Property(e => e.Especie).HasMaxLength(30);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.PesoKg).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Raza).HasMaxLength(50);
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Propietario).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.PropietarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Propietarios");
        });

        modelBuilder.Entity<Propietario>(entity =>
        {
            entity.HasIndex(e => e.Dni, "UQ_Propietarios_Dni").IsUnique();

            entity.Property(e => e.Apellidos).HasMaxLength(60);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(150);
            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FechaRegistro)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Propietarios_FechaRegistro");
            entity.Property(e => e.Nombres).HasMaxLength(60);
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
