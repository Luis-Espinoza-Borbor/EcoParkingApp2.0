using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoParkingApp;

[Table("Administradores")] // ← PLURAL consistente
public class Administrador
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Identificacion { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Contraseña { get; set; } = string.Empty;

    // Constructor sin parámetros para EF Core
    public Administrador() { }

    // Constructor con parámetros
    public Administrador(string nombre, string identificacion, string contraseña)
    {
        Nombre = nombre;
        Identificacion = identificacion;
        Contraseña = contraseña;
    }

    public void MostrarDatos()
    {
        Console.WriteLine("\n--- DATOS DEL ADMINISTRADOR ---");
        Console.WriteLine($"Nombre: {Nombre}");
        Console.WriteLine($"ID: {Identificacion}");
    }

    public void CambiarDisponibilidad(EcoParking parqueo, bool nuevoEstado)
    {
        parqueo.Disponible = nuevoEstado;
        string estadoTexto = nuevoEstado ? "Disponible" : "Reservado";
        Console.WriteLine($" El espacio en {parqueo.Ubicacion} ahora está: {estadoTexto}.");
    }

    public void ActualizarTarifa(EcoParking parqueo, decimal nuevaTarifa)
    {
        if (nuevaTarifa <= 0)
        {
            Console.WriteLine(" Tarifa inválida. Debe ser mayor que cero.");
            return;
        }

        parqueo.TarifaPorHora = nuevaTarifa;
        Console.WriteLine($" Nueva tarifa en {parqueo.Ubicacion}: ${nuevaTarifa:F2} por hora.");
    }

    // Método de presentación de menú con marco visual
    public static void MostrarMenuConMarco(string titulo, string[] opciones)
    {
        int ancho = 50;
        string borde = new string('═', ancho);

        Console.WriteLine($"\n╔{borde}╗");
        Console.WriteLine($"║{titulo.PadLeft((ancho + titulo.Length) / 2).PadRight(ancho)}║");
        Console.WriteLine($"╠{borde}╣");

        for (int i = 0; i < opciones.Length; i++)
        {
            string texto = $" {i + 1}. {opciones[i]}";
            Console.WriteLine($"║{texto.PadRight(ancho)}║");
        }

        Console.WriteLine($"╚{borde}╝");
        Console.Write("Seleccione una opción: ");
    }

    // ========== MÉTODOS DE BASE DE DATOS CON EF CORE ==========

    public static async Task<Administrador?> IniciarSesionEFAsync()
    {
        Console.WriteLine("\n--- INICIO DE SESIÓN ADMINISTRADOR ---");
        Console.Write("Usuario: ");
        string usuarioIngresado = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Contraseña: ");
        string contraseñaIngresada = Console.ReadLine()?.Trim() ?? "";

        try
        {
            await using var context = new EcoParkingContext();
            var administrador = await context.Administradores // ← PLURAL
                .FirstOrDefaultAsync(a => a.Nombre == usuarioIngresado && a.Contraseña == contraseñaIngresada);

            if (administrador != null)
            {
                Console.WriteLine("✅ ¡Inicio de sesión exitoso!");
                return administrador;
            }
            else
            {
                Console.WriteLine("❌ Credenciales incorrectas.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error de base de datos: {ex.Message}");
            return null;
        }
    }

    public static async Task<Administrador?> RegistrarAdministradorEFAsync()
    {
        Console.WriteLine("\n=== REGISTRO DE NUEVO ADMINISTRADOR ===");

        Console.Write("Nombre: ");
        string nombre = Console.ReadLine()?.Trim() ?? "";

        Console.Write("ID/Número de identificación: ");
        string id = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Contraseña: ");
        string contraseña = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(contraseña))
        {
            Console.WriteLine("❌ Todos los campos son obligatorios.");
            return null;
        }

        try
        {
            await using var context = new EcoParkingContext();

            // Verificar si ya existe
            var existe = await context.Administradores // ← PLURAL
                .AnyAsync(a => a.Nombre == nombre || a.Identificacion == id);

            if (existe)
            {
                Console.WriteLine("❌ Ya existe un administrador con ese nombre o ID.");
                return null;
            }

            // Crear nuevo administrador
            var nuevoAdmin = new Administrador(nombre, id, contraseña);
            context.Administradores.Add(nuevoAdmin); // ← PLURAL
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Administrador registrado exitosamente!");
            return nuevoAdmin;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al registrar administrador: {ex.Message}");
            return null;
        }
    }

    public async Task GuardarEnEFAsync()
    {
        try
        {
            await using var context = new EcoParkingContext();

            if (Id == 0) // Nuevo registro
            {
                context.Administradores.Add(this); // ← PLURAL
            }
            else // Actualización
            {
                context.Administradores.Update(this); // ← PLURAL
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Administrador guardado en base de datos!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al guardar administrador: {ex.Message}");
        }
    }

    public static async Task<List<Administrador>> ObtenerTodosAsync()
    {
        try
        {
            await using var context = new EcoParkingContext();
            return await context.Administradores.ToListAsync(); // ← PLURAL
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al obtener administradores: {ex.Message}");
            return new List<Administrador>();
        }
    }

    public static async Task<bool> EliminarAdministradorAsync(int id)
    {
        try
        {
            await using var context = new EcoParkingContext();
            var admin = await context.Administradores.FindAsync(id); // ← PLURAL

            if (admin != null)
            {
                context.Administradores.Remove(admin); // ← PLURAL
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Administrador eliminado exitosamente!");
                return true;
            }

            Console.WriteLine("❌ Administrador no encontrado.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al eliminar administrador: {ex.Message}");
            return false;
        }
    }
}