using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
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

        [Required]
        public int CantidadDisponible { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TarifaPorHora { get; set; }

        public DateTime? HoraReserva { get; set; }
        public DateTime? HoraFinReserva { get; set; }

        [MaxLength(50)]
        public string CodigoReserva { get; set; } = string.Empty;

        public bool PagoRealizado { get; set; }

        // PROPIEDADES [NotMapped] PARA USO EN MEMORIA
        [NotMapped]
        public bool PuedeReservar => CantidadDisponible > 0;

        [NotMapped]
        public bool ReservaActiva => HoraReserva != null && !PagoRealizado;

        [NotMapped]
        public string EstadoDisponibilidad => CantidadDisponible > 0 ? $"{CantidadDisponible} espacios disponibles" : "Sin disponibilidad";

        [NotMapped]
        public string EstadoPago => PagoRealizado ? "Pagado" : "Pendiente";

        public EcoParking() { }

        public EcoParking(string ubicacion, string tipoVehiculo, int cantidadDisponible, decimal tarifaPorHora, string codigoReserva)
        {
            Ubicacion = ubicacion;
            TipoVehiculo = tipoVehiculo;
            CantidadDisponible = cantidadDisponible;
            TarifaPorHora = tarifaPorHora;
            CodigoReserva = codigoReserva;
            PagoRealizado = false;
            HoraReserva = null;
            HoraFinReserva = null;
        }

        public void ReservarEspacio(TimeSpan tiempoReserva)
        {
            if (CantidadDisponible > 0)
            {
                CantidadDisponible--;
                HoraReserva = DateTime.Now;
                HoraFinReserva = DateTime.Now.Add(tiempoReserva);
                GetCodigoReserva();
                Console.WriteLine($"✅ Espacio reservado exitosamente por {tiempoReserva.TotalHours} horas.");
                Console.WriteLine($"⏰ Hora de fin: {HoraFinReserva:HH:mm}");
            }
            else
            {
                Console.WriteLine("❌ No hay espacios disponibles en esta ubicación.");
            }
            GuardarEstadoEnArchivo();
        }

        public void LiberarEspacio()
        {
            CantidadDisponible++;
            HoraReserva = null;
            HoraFinReserva = null;
            PagoRealizado = false;
            Console.WriteLine("✅ Espacio liberado correctamente.");
        }

        public void GuardarEstadoEnArchivo()
        {
            try
            {
                string ruta = "parqueos.txt";
                string datos = $"{DateTime.Now:yyyy-MM-dd HH:mm}|{Id}|{Ubicacion}|{TipoVehiculo}|{TarifaPorHora}|{CantidadDisponible}|{EstadoPago}";
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
                var existente = await context.Parqueos
                    .FirstOrDefaultAsync(p => p.Ubicacion == Ubicacion && p.TipoVehiculo == TipoVehiculo);

                if (existente != null)
                {
                    existente.CantidadDisponible = CantidadDisponible;
                    existente.CodigoReserva = CodigoReserva;
                    existente.PagoRealizado = PagoRealizado;
                    existente.HoraReserva = HoraReserva;
                    existente.HoraFinReserva = HoraFinReserva;
                    existente.TarifaPorHora = TarifaPorHora;

                    context.Parqueos.Update(existente);
                }
                else
                {
                    context.Parqueos.Add(this);
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Estado guardado en base de datos correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar estado en base de datos: {ex.Message}");
                GuardarEstadoEnArchivo();
            }
        }

        // MÉTODO MEJORADO PARA MENÚ DE PAGO
        public (decimal monto, string metodoPago, string infoTarjeta) MenuDePago()
        {
            if (HoraReserva == null)
            {
                Console.WriteLine("❌ No hay una reserva activa para este parqueo.");
                return (0, "", "");
            }

            // Pedir tipo de pago
            Console.WriteLine("\n💳 SELECCIÓN DE MÉTODO DE PAGO");
            Console.WriteLine("1. Efectivo");
            Console.WriteLine("2. Tarjeta de crédito/débito");
            Console.Write("Seleccione método de pago: ");
            string metodoPagoOp = Console.ReadLine()?.Trim() ?? "";

            string metodoPago = "";
            string infoTarjeta = "";

            if (metodoPagoOp == "1")
            {
                metodoPago = "Efectivo";
            }
            else if (metodoPagoOp == "2")
            {
                metodoPago = "Tarjeta";
                Console.Write("💳 Número de tarjeta: ");
                string numeroTarjeta = Console.ReadLine()?.Trim() ?? "";
                Console.Write("📅 Fecha de vencimiento (MM/YY): ");
                string fechaVencimiento = Console.ReadLine()?.Trim() ?? "";
                Console.Write("🔒 CVV: ");
                string cvv = Console.ReadLine()?.Trim() ?? "";

                infoTarjeta = $"Tarjeta: {numeroTarjeta}, Venc: {fechaVencimiento}, CVV: {cvv}";
            }
            else
            {
                Console.WriteLine("❌ Método de pago inválido.");
                return (0, "", "");
            }

            // Pedir tiempo de estacionamiento
            Console.WriteLine("\n⏰ TIEMPO DE ESTACIONAMIENTO");
            Console.WriteLine("1. Por horas");
            Console.WriteLine("2. Por minutos");
            Console.Write("Seleccione opción: ");
            string tiempoOp = Console.ReadLine()?.Trim() ?? "";

            decimal total = 0;

            if (tiempoOp == "1")
            {
                Console.Write("Ingrese número de horas: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal horas) && horas > 0)
                {
                    total = horas * TarifaPorHora;
                }
                else
                {
                    Console.WriteLine("❌ Horas inválidas.");
                    return (0, "", "");
                }
            }
            else if (tiempoOp == "2")
            {
                Console.Write("Ingrese número de minutos: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal minutos) && minutos > 0)
                {
                    decimal tarifaPorMinuto = TarifaPorHora / 60m;
                    total = minutos * tarifaPorMinuto;
                }
                else
                {
                    Console.WriteLine("❌ Minutos inválidos.");
                    return (0, "", "");
                }
            }
            else
            {
                Console.WriteLine("❌ Opción inválida.");
                return (0, "", "");
            }

            Console.WriteLine($"\n💵 DETALLE DE PAGO:");
            Console.WriteLine($"📍 Ubicación: {Ubicacion}");
            Console.WriteLine($"🚗 Tipo vehículo: {TipoVehiculo}");
            Console.WriteLine($"💰 Tarifa: ${TarifaPorHora:F2}/hora");
            Console.WriteLine($"💳 Método: {metodoPago}");
            Console.WriteLine($"💵 Total a pagar: ${total:F2}");

            Console.Write("\n¿Confirmar pago? (s/n): ");
            string confirmacion = Console.ReadLine()?.Trim().ToLower() ?? "";

            return confirmacion == "s" ? (total, metodoPago, infoTarjeta) : (0, "", "");
        }

        // MÉTODO PARA OBTENER CÓDIGO DE RESERVA
        public string GetCodigoReserva()
        {
            if (string.IsNullOrEmpty(CodigoReserva))
            {
                CodigoReserva = $"RES-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
            }
            return CodigoReserva;
        }
    }
}