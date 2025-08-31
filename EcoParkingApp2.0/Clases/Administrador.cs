using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoParkingApp
{
    [Table("Administradores")]
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

        public Administrador() { }

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

        public void ModificarCantidadDisponible(EcoParking parqueo, int nuevaCantidad)
        {
            if (nuevaCantidad < 0)
            {
                Console.WriteLine("❌ La cantidad no puede ser negativa.");
                return;
            }

            parqueo.CantidadDisponible = nuevaCantidad;
            Console.WriteLine($"✅ Cantidad de espacios en {parqueo.Ubicacion} actualizada a: {nuevaCantidad}");
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

        // NUEVO MÉTODO PARA INICIAR SESIÓN
        public static async Task<Administrador?> IniciarSesionEFAsync()
        {
            Console.WriteLine("\n--- INICIO DE SESIÓN ADMINISTRADOR ---");
            Console.Write("Identificación: ");
            string identificacion = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Contraseña: ");
            string contraseña = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(identificacion) || string.IsNullOrEmpty(contraseña))
            {
                Console.WriteLine("❌ Identificación y contraseña son obligatorios.");
                return null;
            }

            try
            {
                using var context = new EcoParkingContext();
                var admin = await context.Administradores
                    .FirstOrDefaultAsync(a => a.Identificacion == identificacion && a.Contraseña == contraseña);

                if (admin != null)
                {
                    Console.WriteLine("✅ ¡Inicio de sesión exitoso!");
                    return admin;
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
    }
}