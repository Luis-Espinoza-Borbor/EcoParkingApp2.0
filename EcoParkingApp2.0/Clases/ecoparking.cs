using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EcoParkingApp
{
    [Table("Parqueos")]
    public class EcoParking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ubicacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TipoVehiculo { get; set; } = string.Empty;

        public bool Disponible { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TarifaPorHora { get; set; }

        public DateTime? HoraReserva { get; set; }

        [MaxLength(50)]
        public string CodigoReserva { get; set; } = string.Empty;

        public bool PagoRealizado { get; set; }

        // Constructor sin parámetros para EF Core
        public EcoParking() { }

        // Constructor con parámetros (SIN ID - lo genera la BD)
        public EcoParking(string ubicacion, string tipoVehiculo, bool disponible, decimal tarifaPorHora, string codigoReserva)
        {
            Ubicacion = ubicacion;
            TipoVehiculo = tipoVehiculo;
            Disponible = disponible;
            TarifaPorHora = tarifaPorHora;
            CodigoReserva = codigoReserva;
            PagoRealizado = false;
        }

        public string GetCodigoReserva()
        {
            Console.Write("Ingrese la ubicación para validar: ");
            string ubicacionIngresada = Console.ReadLine();

            if (ubicacionIngresada == this.Ubicacion)
            {
                return "**" + CodigoReserva?.Substring(CodigoReserva.Length - 3);
            }
            else
            {
                Console.WriteLine("Ubicación incorrecta. No se puede mostrar el código.");
                return "Acceso denegado";
            }
        }

        public void SetCodigoReserva(string nuevoCodigo)
        {
            Console.Write("Ingrese la ubicación para modificar el código: ");
            string ubicacionIngresada = Console.ReadLine();

            if (ubicacionIngresada == this.Ubicacion)
            {
                this.CodigoReserva = nuevoCodigo;
                Console.WriteLine("Código actualizado correctamente.");
            }
            else
            {
                Console.WriteLine("Ubicación incorrecta. No autorizado.");
            }
        }

        public void ReservarEspacio()
        {
            if (Disponible)
            {
                Disponible = false;
                HoraReserva = DateTime.Now;
                Console.WriteLine("Espacio reservado exitosamente.");
            }
            else
            {
                Console.WriteLine("Este espacio ya está reservado.");
            }
            GuardarEstadoEnArchivo();
        }

        public decimal MenuDePago()
        {
            if (Disponible)
            {
                Console.WriteLine("Este espacio aún no ha sido reservado. Por favor, reserva primero.");
                return 0;
            }

            if (PagoRealizado)
            {
                Console.WriteLine("El pago ya ha sido realizado para esta reserva.");
                return 0;
            }

            Console.WriteLine("\n=== MENÚ DE PAGO ECO PARKING ===");

            Console.Write("Nombre completo: ");
            string nombre = Console.ReadLine();

            Console.Write("Número de cédula: ");
            string cedula = Console.ReadLine();

            Console.Write("¿Cuántas horas desea pagar? ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal horas) || horas <= 0)
            {
                Console.WriteLine("Cantidad de horas inválida.");
                return 0;
            }

            Console.WriteLine("\nMétodo de pago:");
            Console.WriteLine("1. Tarjeta");
            Console.WriteLine("2. Efectivo");
            Console.Write("Opción: ");
            string metodoPago = Console.ReadLine();

            string metodo;
            decimal total = TarifaPorHora * horas;

            if (metodoPago == "1")
            {
                metodo = "tarjeta";

                Console.Write("\nNúmero de tarjeta: ");
                string numeroTarjeta = Console.ReadLine();

                Console.Write("Fecha de expiración (MM/AA): ");
                string fechaExp = Console.ReadLine();

                Console.Write("CVV: ");
                string cvv = Console.ReadLine();

                Console.WriteLine($"\nProcesando pago de ${total:F2} con tarjeta...");
            }
            else if (metodoPago == "2")
            {
                metodo = "efectivo";
                Console.WriteLine($"\nProcesando pago de ${total:F2} en efectivo...");
            }
            else
            {
                Console.WriteLine("Método de pago no válido.");
                return 0;
            }

            PagoRealizado = true;
            Console.WriteLine(" Pago realizado correctamente. ¡Gracias por usar EcoParking!");

            Console.Write("\nIngrese su correo electrónico para recibir el comprobante: ");
            string correoDestino = Console.ReadLine();

            GuardarEstadoEnArchivo();

            return total;
        }

        public string EstadoPago()
        {
            return PagoRealizado ? "Pagado" : "Pendiente";
        }

        public void GuardarEstadoEnArchivo()
        {
            try
            {
                string ruta = "parqueos.txt";
                string estado = Disponible ? "Disponible" : "Reservado";
                string datos = $"{DateTime.Now:yyyy-MM-dd HH:mm}|{Id}|{Ubicacion}|{TipoVehiculo}|{TarifaPorHora}|{estado}|{EstadoPago()}";
                File.AppendAllText(ruta, datos + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar en archivo: {ex.Message}");
            }
        }

        public async Task GuardarEstadoEnBaseDeDatosAsync()
        {
            try
            {
                using var context = new EcoParkingContext();

                // Verificar si ya existe en la base de datos por ubicación y tipo
                var existingParqueo = await context.Parqueos
                    .FirstOrDefaultAsync(p => p.Ubicacion == this.Ubicacion && p.TipoVehiculo == this.TipoVehiculo);

                if (existingParqueo != null)
                {
                    // Actualizar solo las propiedades que cambian
                    existingParqueo.Disponible = this.Disponible;
                    existingParqueo.CodigoReserva = this.CodigoReserva;
                    existingParqueo.PagoRealizado = this.PagoRealizado;
                    existingParqueo.HoraReserva = this.HoraReserva;
                    existingParqueo.TarifaPorHora = this.TarifaPorHora;

                    context.Parqueos.Update(existingParqueo);
                }
                else
                {
                    // Crear NUEVO objeto sin el Id para insertar
                    var nuevoParqueo = new EcoParking
                    {
                        Ubicacion = this.Ubicacion,
                        TipoVehiculo = this.TipoVehiculo,
                        Disponible = this.Disponible,
                        TarifaPorHora = this.TarifaPorHora,
                        CodigoReserva = this.CodigoReserva,
                        PagoRealizado = this.PagoRealizado,
                        HoraReserva = this.HoraReserva
                    };

                    context.Parqueos.Add(nuevoParqueo);
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Estado guardado en base de datos correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar estado en base de datos: {ex.Message}");

                // Mostrar más detalles del error interno
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Error interno: {ex.InnerException.Message}");
                }

                // Fallback: guardar en archivo si la BD falla
                Console.WriteLine("📁 Guardando en archivo como respaldo...");
                GuardarEstadoEnArchivo();
            }
        }

        // Método para debuggear
        public void MostrarDatosParaDebug()
        {
            Console.WriteLine($"DEBUG - Id: {Id}");
            Console.WriteLine($"DEBUG - Ubicacion: {Ubicacion}");
            Console.WriteLine($"DEBUG - TipoVehiculo: {TipoVehiculo}");
            Console.WriteLine($"DEBUG - Disponible: {Disponible}");
            Console.WriteLine($"DEBUG - TarifaPorHora: {TarifaPorHora}");
            Console.WriteLine($"DEBUG - CodigoReserva: {CodigoReserva}");
            Console.WriteLine($"DEBUG - PagoRealizado: {PagoRealizado}");
            Console.WriteLine($"DEBUG - HoraReserva: {HoraReserva}");
        }

        // Método para verificar y crear tabla
        public static async Task VerificarYCrearTablaAsync()
        {
            try
            {
                using var context = new EcoParkingContext();
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Tabla de parqueos verificada/creada correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al verificar/crear tabla: {ex.Message}");
            }
        }
    }
}