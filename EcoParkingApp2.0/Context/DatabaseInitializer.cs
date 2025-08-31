using Microsoft.EntityFrameworkCore;
using System;

namespace EcoParkingApp
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            try
            {
                using (var context = new EcoParkingContext())
                {
                    // Aplicar migraciones automáticamente
                    context.Database.Migrate();

                    Console.WriteLine("✅ Base de datos migrada exitosamente!");

                    // Verificar si ya existen administradores
                    if (!context.Administradores.Any())
                    {
                        // Agregar administrador por defecto
                        var admin = new Administrador("admin", "001", "admin123");
                        context.Administradores.Add(admin);
                        context.SaveChanges();
                        Console.WriteLine("✅ Administrador por defecto creado");
                    }

                    Console.WriteLine("📊 Estado de la base de datos:");
                    Console.WriteLine($"   Administradores: {context.Administradores.Count()}");
                    Console.WriteLine($"   Usuarios: {context.Usuarios.Count()}");
                    Console.WriteLine($"   Parqueos: {context.Parqueos.Count()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inicializando base de datos: {ex.Message}");
            }
        }
    }
}
