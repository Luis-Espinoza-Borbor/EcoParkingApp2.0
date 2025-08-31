using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EcoParking_Proyecto
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync()
        {
            try
            {
                using var context = new EcoParkingContext();

                // SOLUCIÓN: Usar EnsureCreatedAsync en lugar de EnsureDeletedAsync
                // Esto crea las tablas si no existen, pero no elimina las existentes
                await context.Database.EnsureCreatedAsync();

                Console.WriteLine("✅ Base de datos verificada.");

                // Crear administrador por defecto solo si no existe
                await CrearAdministradorPorDefectoAsync(context);

                // Crear los parqueos específicos solo si no existen
                await CrearParqueosEspecificosAsync(context);

                // Verificar y agregar columnas faltantes
                await VerificarColumnasFaltantesAsync(context);

                Console.WriteLine("✅ Inicialización completada correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en inicialización: {ex.Message}");
                Console.WriteLine($"📋 Detalles: {ex.InnerException?.Message}");
            }
        }

        private static async Task CrearAdministradorPorDefectoAsync(EcoParkingContext context)
        {
            try
            {
                // Verificar si ya existe un administrador
                var adminExistente = await context.Administradores
                    .FirstOrDefaultAsync(a => a.Identificacion == "admin123");

                if (adminExistente == null)
                {
                    var admin = new Administrador("Admin Principal", "admin123", "admin123");
                    context.Administradores.Add(admin);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Administrador por defecto creado:");
                    Console.WriteLine($"   👤 Usuario: admin123");
                    Console.WriteLine($"   🔑 Contraseña: admin123");
                }
                else
                {
                    Console.WriteLine("ℹ️  Administrador ya existe en la base de datos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando administrador: {ex.Message}");
            }
        }

        private static async Task CrearParqueosEspecificosAsync(EcoParkingContext context)
        {
            try
            {
                // Verificar si ya existen parqueos
                if (!await context.Parqueos.AnyAsync())
                {
                    var parqueos = new[]
                    {
                        new EcoParking("Guayaquil-Centro", "Auto", 50, 1.50m, "GYE123"),
                        new EcoParking("Guayaquil-Norte", "Moto", 30, 1.00m, "GYN456"),
                        new EcoParking("Samborondón", "Camioneta", 20, 2.00m, "SAM789")
                    };

                    context.Parqueos.AddRange(parqueos);
                    await context.SaveChangesAsync();

                    Console.WriteLine("✅ Parqueos específicos creados:");
                    foreach (var parqueo in parqueos)
                    {
                        Console.WriteLine($"   📍 {parqueo.Ubicacion} - {parqueo.TipoVehiculo} - ${parqueo.TarifaPorHora}/hora - {parqueo.CantidadDisponible} espacios");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️  Ya existen parqueos en la base de datos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando parqueos: {ex.Message}");
            }
        }

        private static async Task VerificarColumnasFaltantesAsync(EcoParkingContext context)
        {
            try
            {
                // Verificar y agregar columnas faltantes en Fidelidad
                var sqlCommands = new[]
                {
                    // Verificar si la columna existe antes de agregarla
                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                      WHERE TABLE_NAME = 'Fidelidad' AND COLUMN_NAME = 'ReservasRealizadas')
                      BEGIN
                          ALTER TABLE Fidelidad ADD ReservasRealizadas INT NOT NULL DEFAULT 0;
                          PRINT 'Columna ReservasRealizadas agregada.';
                      END",

                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                      WHERE TABLE_NAME = 'Fidelidad' AND COLUMN_NAME = 'NivelFidelidad')
                      BEGIN
                          ALTER TABLE Fidelidad ADD NivelFidelidad NVARCHAR(50) NOT NULL DEFAULT 'Bronce';
                          PRINT 'Columna NivelFidelidad agregada.';
                      END",

                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                      WHERE TABLE_NAME = 'Fidelidad' AND COLUMN_NAME = 'DescuentoAplicado')
                      BEGIN
                          ALTER TABLE Fidelidad ADD DescuentoAplicado DECIMAL(5,2) NOT NULL DEFAULT 0;
                          PRINT 'Columna DescuentoAplicado agregada.';
                      END"
                };

                foreach (var sql in sqlCommands)
                {
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(sql);
                    }
                    catch (Exception sqlEx)
                    {
                        Console.WriteLine($"⚠️  Error ejecutando comando SQL: {sqlEx.Message}");
                    }
                }

                Console.WriteLine("✅ Columnas verificadas y actualizadas.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error verificando columnas: {ex.Message}");
            }
        }

        // Método separado solo para verificar estructura (sin crear datos)
        public static async Task VerificarEstructuraAsync()
        {
            try
            {
                using var context = new EcoParkingContext();
                await VerificarColumnasFaltantesAsync(context);
                Console.WriteLine("✅ Estructura de base de datos verificada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error verificando estructura: {ex.Message}");
            }
        }
    }
}