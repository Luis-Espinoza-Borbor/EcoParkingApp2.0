using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoParkingApp;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Cedula { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    public Usuario() { }

    public Usuario(string nombre, string cedula, string correo, string telefono)
    {
        Nombre = nombre;
        Cedula = cedula;
        Correo = correo;
        Telefono = telefono;
    }

    public void MostrarDatos()
    {
        Console.WriteLine("\n--- DATOS DEL USUARIO ---");
        Console.WriteLine($"Nombre: {Nombre}");
        Console.WriteLine($"Cédula: {Cedula}");
        Console.WriteLine($"Teléfono: {Telefono}");
        Console.WriteLine($"Correo: {Correo}");
    }

    public static async Task<Usuario?> RegistrarNuevoUsuarioAsync()
    {
        Console.WriteLine("\n=== REGISTRO DE USUARIO ===");

        Console.Write("Nombre completo: ");
        string nombre = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Cédula: ");
        string cedula = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Correo electrónico: ");
        string correo = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Número de teléfono: ");
        string telefono = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(cedula) || string.IsNullOrEmpty(correo))
        {
            Console.WriteLine(" Nombre, cédula y correo son obligatorios.");
            return null;
        }

        try
        {
            await using var context = new EcoParkingContext();

            bool existe = await context.Usuarios
                .AnyAsync(u => u.Cedula == cedula || u.Correo == correo);

            if (existe)
            {
                Console.WriteLine(" Ya existe un usuario con esa cédula o correo.");
                return null;
            }

            var nuevoUsuario = new Usuario(nombre, cedula, correo, telefono);
            context.Usuarios.Add(nuevoUsuario);
            await context.SaveChangesAsync();

            Console.WriteLine(" Usuario registrado exitosamente!");
            return nuevoUsuario;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al registrar usuario: {ex.Message}");
            return null;
        }
    }

    public static async Task<Usuario?> IniciarSesionAsync()
    {
        Console.WriteLine("\n--- INICIO DE SESIÓN ---");
        Console.Write("Cédula: ");
        string cedula = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Correo: ");
        string correo = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrEmpty(cedula) || string.IsNullOrEmpty(correo))
        {
            Console.WriteLine(" Cédula y correo son obligatorios.");
            return null;
        }

        try
        {
            await using var context = new EcoParkingContext();

            var usuario = await context.Usuarios
                .FirstOrDefaultAsync(u => u.Cedula == cedula && u.Correo == correo);

            if (usuario != null)
            {
                Console.WriteLine(" ¡Inicio de sesión exitoso!");
                return usuario;
            }
            else
            {
                Console.WriteLine(" Credenciales incorrectas.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error de base de datos: {ex.Message}");
            return null;
        }
    }

    public async Task GuardarEnBaseDeDatosAsync()
    {
        try
        {
            await using var context = new EcoParkingContext();

            if (Id == 0)
            {
                context.Usuarios.Add(this);
            }
            else
            {
                context.Usuarios.Update(this);
            }

            await context.SaveChangesAsync();
            Console.WriteLine(" Usuario guardado en base de datos!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al guardar usuario: {ex.Message}");
        }
    }

    public static async Task<bool> ExisteUsuarioAsync(string cedula)
    {
        try
        {
            await using var context = new EcoParkingContext();
            return await context.Usuarios.AnyAsync(u => u.Cedula == cedula);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al verificar usuario: {ex.Message}");
            return false;
        }
    }

    public static async Task<List<Usuario>> ObtenerTodosUsuariosAsync()
    {
        try
        {
            await using var context = new EcoParkingContext();
            return await context.Usuarios.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al obtener usuarios: {ex.Message}");
            return new List<Usuario>();
        }
    }

    [Obsolete("Usar métodos de base de datos en su lugar")]
    public void GuardarEnArchivo()
    {
        try
        {
            string ruta = "usuarios.txt";
            string datos = $"{Nombre}|{Cedula}|{Correo}|{Telefono}";
            File.AppendAllText(ruta, datos + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al guardar en archivo: {ex.Message}");
        }
    }

    [Obsolete("Usar métodos de base de datos en su lugar")]
    public static bool ExisteUsuario(string cedula)
    {
        try
        {
            string ruta = "usuarios.txt";
            if (!File.Exists(ruta)) return false;

            return File.ReadAllLines(ruta)
                .Any(linea => linea.Split('|').Length > 1 && linea.Split('|')[1] == cedula);
        }
        catch
        {
            return false;
        }
    }

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
}