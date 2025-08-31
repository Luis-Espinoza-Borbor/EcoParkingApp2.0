using System;
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
        public DateTime HoraSalidaProg { get; set; }

        [Required]
        public DateTime HoraSalidaReal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoAPagar { get; set; }

        public CitacionParqueo() { }

        public static async Task GenerarAsync(Usuario usuario, EcoParking parqueo, DateTime horaSalidaProgramada)
        {
            DateTime ahora = DateTime.Now;
            if (ahora <= horaSalidaProgramada)
                return;

            TimeSpan exceso = ahora - horaSalidaProgramada;
            decimal tarifaMinuto = parqueo.TarifaPorHora / 60.0m;
            decimal multa = (decimal)exceso.TotalMinutes * tarifaMinuto * 2.0m;

            var citacion = new CitacionParqueo
            {
                Usuario = usuario.Nombre,
                Cedula = usuario.Cedula,
                Correo = usuario.Correo,
                VehiculoTipo = parqueo.TipoVehiculo,
                CodigoReserva = parqueo.GetCodigoReserva(),
                HoraSalidaProg = horaSalidaProgramada,
                HoraSalidaReal = ahora,
                MontoAPagar = multa
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
                Console.WriteLine("✅ Citación guardada en base de datos!");
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
                    Subject = "Citación por exceso de tiempo – EcoParking",
                    Body = $"Hola {Usuario},\n\n" +
                           $"Tu vehículo ({VehiculoTipo}) con reserva {CodigoReserva} excedió el tiempo autorizado.\n" +
                           $"Hora salida programada: {HoraSalidaProg:yyyy-MM-dd HH:mm}\n" +
                           $"Hora salida real: {HoraSalidaReal:yyyy-MM-dd HH:mm}\n" +
                           $"Minutos excedidos: {(HoraSalidaReal - HoraSalidaProg).TotalMinutes:F0}\n" +
                           $"Monto a pagar por citación: ${MontoAPagar:F2}\n\n" +
                           "Por favor, realice el pago a la brevedad.\n\n" +
                           "Saludos,\nEcoParking"
                };

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("ecoparkingproyecto@gmail.com", "fsverkgbytdouxxe"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(msg);
                Console.WriteLine("✅ Correo de citación enviado!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo: {ex.Message}");
            }
        }
    }
}