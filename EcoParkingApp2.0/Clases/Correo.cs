using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace EcoParking_Proyecto
{
    [Serializable]
    public class Correo
    {
        public int Id { get; set; }
        public string Destinatario { get; set; }
        public string NombreUsuario { get; set; }
        public string MetodoPago { get; set; }
        public double MontoPagado { get; set; }
        public DateTime FechaPago { get; set; }

        public Correo(string destinatario, string nombreUsuario, string metodoPago, double montoPagado)
        {
            Destinatario = destinatario;
            NombreUsuario = nombreUsuario;
            MetodoPago = metodoPago;
            MontoPagado = montoPagado;
            FechaPago = DateTime.Now;
        }

        public void EnviarComprobante()
        {
            try
            {
                string remitente = "ecoparkingproyecto@gmail.com";
                string contrasenaApp = "fsverkgbytdouxxe";

                string asunto = "Comprobante de Pago - EcoParking";

                // ✅ CORREGIDO: Usar las propiedades correctamente
                string cuerpoHTML = $@"
                <html>
                <body style='font-family: Arial; padding: 20px; background-color: #f4f4f4;'>
                    <h2 style='color: #2e7d32;'>Comprobante de Pago - EcoParking</h2>
                    <p><strong>Nombre:</strong> {this.NombreUsuario}</p>
                    <p><strong>Método de pago:</strong> {this.MetodoPago}</p>
                    <p><strong>Monto pagado:</strong> ${this.MontoPagado:F2}</p>
                    <p><strong>Fecha de pago:</strong> {this.FechaPago:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Número de transacción:</strong> {DateTime.Now.Ticks.ToString().Substring(8)}</p>
                    <hr style='border: 1px solid #ccc;'/>
                    <p style='color: #555; font-style: italic;'>Gracias por preferir EcoParking. Este es un comprobante electrónico válido.</p>
                    <p style='color: #888; font-size: 12px;'>EcoParking System - Parqueo ecológico y seguro</p>
                </body>
                </html>";

                // Especifica explícitamente System.Net.Mail para evitar ambigüedad
                System.Net.Mail.MailMessage mensaje = new System.Net.Mail.MailMessage(
                    new System.Net.Mail.MailAddress(remitente),
                    new System.Net.Mail.MailAddress(this.Destinatario)
                )
                {
                    Subject = asunto,
                    Body = cuerpoHTML,
                    IsBodyHtml = true
                };

                System.Net.Mail.SmtpClient cliente = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(remitente, contrasenaApp),
                    EnableSsl = true,
                    Timeout = 10000
                };

                cliente.Send(mensaje);
                Console.WriteLine("✅ Comprobante enviado al correo del usuario.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar el correo: {ex.Message}");
                // Para debugging más detallado:
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Error interno: {ex.InnerException.Message}");
                }
            }
        }
    }
}