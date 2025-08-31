using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EcoParkingApp;

public class EcoParkingContext : DbContext
{
    public DbSet<Administrador> Administradores { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<EcoParking> Parqueos { get; set; }
    public DbSet<GananciasEcoParking> Ganancias { get; set; }
    public DbSet<FlujoPersonas> FlujoPersonas { get; set; }
    public DbSet<EstadisticaVehicular> EstadisticasVehiculares { get; set; }
    public DbSet<Fidelidad> Fidelidad { get; set; }
    public DbSet<ReseñaParqueo> ReseñasParqueo { get; set; }
    public DbSet<CitacionParqueo> Citaciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-JJ0AADA\\SQLEXPRESS;Database=EcoParkingDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().ToTable("Administradores");
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<EcoParking>().ToTable("Parqueos");
        modelBuilder.Entity<GananciasEcoParking>().ToTable("Ganancias");
        modelBuilder.Entity<FlujoPersonas>().ToTable("FlujoPersonas");
        modelBuilder.Entity<EstadisticaVehicular>().ToTable("EstadisticasVehiculares");
        modelBuilder.Entity<Fidelidad>().ToTable("Fidelidad");
        modelBuilder.Entity<ReseñaParqueo>().ToTable("ReseñasParqueo");
        modelBuilder.Entity<CitacionParqueo>().ToTable("Citaciones");

        // Configuración de EcoParking actualizada con todas las propiedades
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.Ubicacion).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.TipoVehiculo).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.CodigoReserva).HasMaxLength(50);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.CantidadDisponible).IsRequired();
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.TarifaPorHora).HasColumnType("decimal(10,2)");
        // Asegurar que las propiedades DateTime? sean opcionales en la base de datos
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.HoraReserva).IsRequired(false);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.HoraFinReserva).IsRequired(false);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.PagoRealizado).IsRequired();

        // Configuración de EstadisticaVehicular actualizada
        modelBuilder.Entity<EstadisticaVehicular>()
            .Property(e => e.TipoVehiculo).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<EstadisticaVehicular>()
            .Property(e => e.CantidadUsos).IsRequired();
        modelBuilder.Entity<EstadisticaVehicular>()
            .Property(e => e.TotalRecaudado).HasColumnType("decimal(10,2)");
        modelBuilder.Entity<EstadisticaVehicular>()
            .Property(e => e.FechaRegistro).IsRequired();
        modelBuilder.Entity<EstadisticaVehicular>()
            .Property(e => e.FechaUltimoUso).IsRequired(false);

        // Configuración de otras entidades (sin cambios estructurales)
        modelBuilder.Entity<Administrador>()
            .Property(a => a.Nombre).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Administrador>()
            .Property(a => a.Identificacion).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Administrador>()
            .Property(a => a.Contraseña).IsRequired().HasMaxLength(255);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Cedula).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Correo).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Telefono).HasMaxLength(20);

        modelBuilder.Entity<GananciasEcoParking>()
            .Property(g => g.Concepto).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<GananciasEcoParking>()
            .Property(g => g.MetodoPago).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<GananciasEcoParking>()
            .Property(g => g.UbicacionParqueo).HasMaxLength(100);
        modelBuilder.Entity<GananciasEcoParking>()
            .Property(g => g.TipoVehiculo).HasMaxLength(50);
        modelBuilder.Entity<GananciasEcoParking>()
            .Property(g => g.Usuario).HasMaxLength(100);
        modelBuilder.Entity<GananciasEcoParking>()
            .Property(g => g.Monto).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.NombrePersona).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.TipoAcceso).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.HoraEntrada).HasMaxLength(10);
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.DiaSemana).HasMaxLength(20);

        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.NombreUsuario).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.CorreoUsuario).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.ReservasRealizadas).IsRequired();
        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.NivelFidelidad).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.DescuentoAplicado).HasColumnType("decimal(5,2)");

        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.IdParqueo).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.Usuario).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.Comentario).HasMaxLength(500);
        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.Puntuacion).IsRequired();

        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.Usuario).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.Cedula).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.Correo).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.VehiculoTipo).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.CodigoReserva).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.Motivo).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<CitacionParqueo>()
            .Property(c => c.MontoMulta).HasColumnType("decimal(10,2)");
    }
}
