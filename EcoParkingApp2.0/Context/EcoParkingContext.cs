using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EcoParkingApp;

public class EcoParkingContext : DbContext
{
    // DbSets para todas las entidades - CORREGIDOS
    public DbSet<Administrador> Administradores { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<EcoParking> Parqueos { get; set; } // ← CAMBIADO: EcoParkings → Parqueos
    public DbSet<GananciasEcoParking> Ganancias { get; set; }
    public DbSet<FlujoPersonas> FlujoPersonas { get; set; }
    public DbSet<EstadisticaVehicular> EstadisticasVehiculares { get; set; }
    public DbSet<Fidelidad> Fidelidad { get; set; }
    public DbSet<ReseñaParqueo> ReseñasParqueo { get; set; }
    public DbSet<CitacionParqueo> Citaciones { get; set; }

    // RESPETO TU CONFIGURACIÓN ACTUAL DE CONEXIÓN
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-JJ0AADA\\SQLEXPRESS;Database=EcoParkingDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar nombre de tablas en PLURAL
        modelBuilder.Entity<Administrador>().ToTable("Administradores");
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<EcoParking>().ToTable("Parqueos"); // ← CAMBIADO: EcoParkings → Parqueos
        modelBuilder.Entity<GananciasEcoParking>().ToTable("Ganancias");
        modelBuilder.Entity<FlujoPersonas>().ToTable("FlujoPersonas");
        modelBuilder.Entity<EstadisticaVehicular>().ToTable("EstadisticasVehiculares");
        modelBuilder.Entity<Fidelidad>().ToTable("Fidelidad");
        modelBuilder.Entity<ReseñaParqueo>().ToTable("ReseñasParqueo");
        modelBuilder.Entity<CitacionParqueo>().ToTable("Citaciones");

        // Configuración de Administrador
        modelBuilder.Entity<Administrador>()
            .Property(a => a.Nombre).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Administrador>()
            .Property(a => a.Identificacion).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Administrador>()
            .Property(a => a.Contraseña).IsRequired().HasMaxLength(255);

        // Configuración de Usuario - CORREGIDO
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Cedula).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Correo).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Telefono).HasMaxLength(20);

        // Configuración de EcoParking
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.Ubicacion).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.TipoVehiculo).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<EcoParking>()
            .Property(p => p.CodigoReserva).HasMaxLength(10);

        // Configuración de GananciasEcoParking - CORREGIDO
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

        // Configuración de FlujoPersonas - CORREGIDO
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.NombrePersona).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.TipoAcceso).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.HoraEntrada).HasMaxLength(10);
        modelBuilder.Entity<FlujoPersonas>()
            .Property(f => f.DiaSemana).HasMaxLength(20);

        // Configuración de EstadisticaVehicular
        modelBuilder.Entity<EstadisticaVehicular>()
            .Property(e => e.TipoVehiculo).IsRequired().HasMaxLength(50);

        // Configuración de Fidelidad
        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.NombreUsuario).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Fidelidad>()
            .Property(f => f.CorreoUsuario).IsRequired().HasMaxLength(100);

        // Configuración de ReseñaParqueo
        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.IdParqueo).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.Usuario).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<ReseñaParqueo>()
            .Property(r => r.Comentario).HasMaxLength(500);

        // Configuración de CitacionParqueo - CORREGIDO
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
    }

    public async Task InitializeDataAsync()
    {
        await Database.EnsureCreatedAsync();

        // Datos iniciales para administrador si no existen
        if (!Administradores.Any())
        {
            Administradores.Add(new Administrador("Admin", "1234567890", "admin123"));
            await SaveChangesAsync();
        }

        // Datos iniciales para estadísticas vehiculares si no existen
        if (!EstadisticasVehiculares.Any())
        {
            var tiposVehiculos = new[] { "Auto", "Moto", "Camioneta", "Bicicleta" };
            foreach (var tipo in tiposVehiculos)
            {
                EstadisticasVehiculares.Add(new EstadisticaVehicular(tipo));
            }
            await SaveChangesAsync();
        }

        // Datos iniciales de parqueos si no existen - CORREGIDO
        if (!Parqueos.Any()) // ← CAMBIADO: EcoParkings → Parqueos
        {
            var parqueos = new List<EcoParking>
            {
                new EcoParking("Guayaquil-Centro", "Auto", true, 1.50m, "GYE123"),
                new EcoParking("Guayaquil-Norte", "Moto", true, 1.00m, "GYN456"),
                new EcoParking("Samborondón", "Camioneta", true, 2.00m, "SAM789")
            };

            Parqueos.AddRange(parqueos); // ← CAMBIADO: EcoParkings → Parqueos
            await SaveChangesAsync();
        }
    }
}