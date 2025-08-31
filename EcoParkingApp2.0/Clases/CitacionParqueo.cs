using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EcoParkingApp
{
    [Table("Citaciones")]
    public class CitacionParqueo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string VehiculoTipo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CodigoReserva { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Motivo { get; set; } = "Tiempo de estacionamiento excedido";

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoMulta { get; set; }

        public bool Pagada { get; set; } = false;

        [Required]
        public DateTime HoraInicioReserva { get; set; }

        [Required]
        public DateTime HoraFinProgramada { get; set; }

        [Required]
        public DateTime HoraFinReal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TarifaPorHora { get; set; }

        public int MinutosExcedidos { get; set; }

        public CitacionParqueo() { }

        // MÉTODO: Generar citación por tiempo excedido
        public static async Task GenerarPorTiempoExcedidoAsync(Usuario usuario, EcoParking parqueo,
                                                            TimeSpan tiempoReservado, TimeSpan tiempoPagado)
        {
            if (tiempoPagado >= tiempoReservado)
                return; // No generar citación si pagó todo el tiempo

            // Calcular tiempo excedido
            TimeSpan tiempoExcedido = tiempoReservado - tiempoPagado;
            int minutosExcedidos = (int)tiempoExcedido.TotalMinutes;

            if (minutosExcedidos <= 0)
                return;

            // Calcular multa (40% del valor total del parqueo)
            decimal valorTotalParqueo = (decimal)tiempoReservado.TotalHours * parqueo.TarifaPorHora;
            decimal multa = valorTotalParqueo * 0.40m;

            var citacion = new CitacionParqueo
            {
                Usuario = usuario.Nombre,
                Cedula = usuario.Cedula,
                Correo = usuario.Correo,
                VehiculoTipo = parqueo.TipoVehiculo,
                CodigoReserva = parqueo.CodigoReserva,
                Motivo = $"Tiempo de estacionamiento excedido: {minutosExcedidos} minutos",
                MontoMulta = multa,
                HoraInicioReserva = parqueo.HoraReserva ?? DateTime.Now,
                HoraFinProgramada = (parqueo.HoraReserva ?? DateTime.Now).Add(tiempoReservado),
                HoraFinReal = (parqueo.HoraReserva ?? DateTime.Now).Add(tiempoPagado),
                TarifaPorHora = parqueo.TarifaPorHora,
                MinutosExcedidos = minutosExcedidos,
                Pagada = false
            };

            await citacion.GuardarAsync();
            await citacion.EnviarCorreoAsync();
        }

        public async Task GuardarAsync()
        {
            try
            {
                using var context = new EcoParkingContext();
                context.Citaciones.Add(this);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Citación por tiempo excedido guardada en base de datos!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar citación: {ex.Message}");
            }
        }

        public async Task EnviarCorreoAsync()
        {
            try
            {
                var fromAddress = new MailAddress("no-reply@ecoparking.com", "EcoParking");
                var toAddress = new MailAddress(this.Correo, this.Usuario);

                var msg = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "⚠️ Citación por tiempo excedido – EcoParking",
                    Body = $@"Hola {Usuario},

Has recibido una citación por exceder el tiempo de estacionamiento.

📋 DETALLES DE LA CITACIÓN:
🔸 Vehículo: {VehiculoTipo}
🔸 Código de reserva: {CodigoReserva}
🔸 Hora inicio: {HoraInicioReserva:HH:mm}
🔸 Hora fin programada: {HoraFinProgramada:HH:mm}
🔸 Hora fin real: {HoraFinReal:HH:mm}
🔸 Minutos excedidos: {MinutosExcedidos}
🔸 Tarifa por hora: ${TarifaPorHora:F2}

💰 MULTA APLICADA:
🔸 Motivo: {Motivo}
🔸 Multa (40% del valor total): ${MontoMulta:F2}

💡 Por favor, realiza el pago de la multa a la brevedad para evitar cargos adicionales.

Saludos,
Sistema de Gestión EcoParking"
                };

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("ecoparkingproyecto@gmail.com", "fsverkgbytdouxxe"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(msg);
                Console.WriteLine("✅ Correo de citación enviado al usuario!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo: {ex.Message}");
            }
        }

        // MÉTODO PARA ENVIAR FACTURA DE PAGO DE MULTA
        public async Task EnviarFacturaPagoAsync()
        {
            try
            {
                var fromAddress = new MailAddress("no-reply@ecoparking.com", "EcoParking");
                var toAddress = new MailAddress(this.Correo, this.Usuario);

                decimal iva = MontoMulta * 0.12m;
                decimal total = MontoMulta + iva;

                var msg = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "📄 Factura de pago de multa – EcoParking",
                    Body = $@"
FACTURA DE PAGO DE MULTA - ECOPARKING
==========================================

📋 INFORMACIÓN DE LA FACTURA:
🔸 Número de factura: MULTA-{this.Id:0000}
🔸 Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}
🔸 Estado: PAGADA

👤 INFORMACIÓN DEL CLIENTE:
🔸 Nombre: {this.Usuario}
🔸 Cédula: {this.Cedula}
🔸 Email: {this.Correo}

📊 DETALLES DE LA MULTA:
🔸 Vehículo: {this.VehiculoTipo}
🔸 Código de reserva: {this.CodigoReserva}
🔸 Motivo: {this.Motivo}
🔸 Minutos excedidos: {this.MinutosExcedidos}
🔸 Fecha de la infracción: {this.HoraInicioReserva:dd/MM/yyyy}

💵 DETALLES DE PAGO:
🔸 Subtotal: ${this.MontoMulta:F2}
🔸 IVA (12%): ${iva:F2}
🔸 TOTAL PAGADO: ${total:F2}

📝 INFORMACIÓN ADICIONAL:
🔸 Método de pago: Sistema EcoParking
🔸 Número de transacción: TRANS-{DateTime.Now:yyyyMMddHHmmss}
🔸 Fecha de pago: {DateTime.Now:dd/MM/yyyy HH:mm}

Gracias por realizar el pago oportuno de su multa.

Este documento es una factura electrónica y tiene validez fiscal.

Saludos,
Sistema de Gestión EcoParking
=========================================="
                };

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("ecoparkingproyecto@gmail.com", "fsverkgbytdouxxe"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(msg);
                Console.WriteLine("✅ Factura de pago enviada al correo del usuario.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar factura: {ex.Message}");
            }
        }

        // MÉTODO PARA MARCAR COMO PAGADA Y ENVIAR FACTURA
        public async Task MarcarComoPagadaAsync()
        {
            try
            {
                using var context = new EcoParkingContext();
                var citacion = await context.Citaciones.FindAsync(this.Id);
                if (citacion != null)
                {
                    citacion.Pagada = true;
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Citación marcada como pagada.");

                    // Enviar factura por correo
                    await EnviarFacturaPagoAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error marcando citación como pagada: {ex.Message}");
            }
        }

        // MÉTODO PARA OBTENER CITACIONES PENDIENTES DE UN USUARIO
        public static async Task<List<CitacionParqueo>> ObtenerCitacionesPendientesAsync(string usuario)
        {
            try
            {
                using var context = new EcoParkingContext();
                return await context.Citaciones
                    .Where(c => c.Usuario == usuario && !c.Pagada)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo citaciones: {ex.Message}");
                return new List<CitacionParqueo>();
            }
        }
    }
}