using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace EcoParkingApp;

[Table("Fidelidad")]
public class Fidelidad
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string CorreoUsuario { get; set; } = string.Empty;

    // CAMBIAR EL NOMBRE DE LA PROPIEDAD PARA QUE COINCIDA CON EL CONTEXT
    public int ReservasRealizadas { get; set; } // ← Cambiado de ReservasCompletadas

    // AÑADIR PROPIEDADES FALTANTES
    [MaxLength(50)]
    public string NivelFidelidad { get; set; } = "Bronce"; // ← Nueva propiedad

    [Column(TypeName = "decimal(5,2)")]
    public decimal DescuentoAplicado { get; set; } // ← Nueva propiedad

    // MANTENER PROPIEDADES EXISTENTES
    public DateTime UltimaFechaBeneficio { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaUltimaReserva { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalDescuentoAplicado { get; set; }

    // Constructor para EF Core
    public Fidelidad() { }

    // Constructor para nuevo usuario
    public Fidelidad(string nombreUsuario, string correoUsuario)
    {
        NombreUsuario = nombreUsuario;
        CorreoUsuario = correoUsuario;
        ReservasRealizadas = 0; // ← Cambiado
        NivelFidelidad = "Bronce"; // ← Nueva
        DescuentoAplicado = 0; // ← Nueva
        UltimaFechaBeneficio = DateTime.MinValue;
        FechaRegistro = DateTime.Now;
        TotalDescuentoAplicado = 0;
    }

    // Registra cada reserva completada
    public async Task RegistrarReservaAsync()
    {
        ReservasRealizadas++; // ← Cambiado
        FechaUltimaReserva = DateTime.Now;
        await GuardarEnBaseDeDatosAsync();
    }

    // Verifica y aplica descuento si corresponde
    public async Task<decimal> VerificarYAplicarDescuentoAsync(decimal montoOriginal)
    {
        const decimal descuento = 0.20m; // 20%

        // Contar reservas desde el último beneficio
        int reservasDesdeUltimoBeneficio = await ObtenerReservasDesdeUltimoBeneficioAsync();

        if (reservasDesdeUltimoBeneficio >= 10)
        {
            // Aplica descuento
            decimal montoConDescuento = montoOriginal * (1 - descuento);
            decimal descuentoAplicado = montoOriginal - montoConDescuento;

            // Actualizar propiedades
            this.DescuentoAplicado = descuentoAplicado; // ← Nueva
            this.NivelFidelidad = CalcularNivelFidelidad(); // ← Nueva
            UltimaFechaBeneficio = DateTime.Now;
            TotalDescuentoAplicado += descuentoAplicado;

            await GuardarEnBaseDeDatosAsync();
            await EnviarNotificacionAsync(montoOriginal, montoConDescuento, descuentoAplicado);

            return montoConDescuento;
        }

        return montoOriginal;
    }

    // Nuevo método para calcular nivel de fidelidad
    private string CalcularNivelFidelidad()
    {
        return ReservasRealizadas switch
        {
            >= 50 => "Oro",
            >= 25 => "Plata",
            >= 10 => "Bronce",
            _ => "Nuevo"
        };
    }

    // Obtiene reservas desde el último beneficio
    private async Task<int> ObtenerReservasDesdeUltimoBeneficioAsync()
    {
        using var context = new EcoParkingContext();

        if (UltimaFechaBeneficio == DateTime.MinValue)
        {
            // Si nunca ha tenido beneficio, contar todas las reservas
            return ReservasRealizadas; // ← Cambiado
        }
        else
        {
            // Contar reservas desde el último beneficio
            return await context.Fidelidad
                .Where(f => f.NombreUsuario == this.NombreUsuario &&
                           f.FechaUltimaReserva > this.UltimaFechaBeneficio)
                .SumAsync(f => f.ReservasRealizadas); // ← Cambiado
        }
    }

    // Envía notificación de descuento
    private async Task EnviarNotificacionAsync(decimal montoOriginal, decimal montoConDescuento, decimal descuentoAplicado)
    {
        // Mensaje en consola
        Console.WriteLine($"\n🎉 ¡Felicidades {NombreUsuario}!");
        Console.WriteLine($"✅ Has alcanzado 10 reservas. Se ha aplicado un 20% de descuento.");
        Console.WriteLine($"💰 Total original: ${montoOriginal:F2} → Total con descuento: ${montoConDescuento:F2}");
        Console.WriteLine($"🎁 Descuento aplicado: ${descuentoAplicado:F2}\n");

        // Envío de correo electrónico
        try
        {
            string remitente = "ecoparkingproyecto@gmail.com";
            string contrasenaApp = "fsverkgbytdouxxe";

            string asunto = "🎁 Descuento de fidelidad aplicado - EcoParking";
            string cuerpo = $@"
            <html>
            <body style='font-family: Arial; padding: 20px; background-color: #f4f8fb;'>
                <h2 style='color: #2e7d32;'>¡Felicidades {NombreUsuario}!</h2>
                <p>Gracias por tu lealtad. Has completado <strong>10 reservas</strong> en EcoParking.</p>
                
                <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <h3 style='color: #1b5e20;'>💳 Descuento aplicado:</h3>
                    <p><strong>Total original:</strong> ${montoOriginal:F2}</p>
                    <p><strong>Descuento (20%):</strong> -${descuentoAplicado:F2}</p>
                    <p><strong>Nuevo total:</strong> ${montoConDescuento:F2}</p>
                </div>
                
                <p>¡Sigue disfrutando de los beneficios de ser un cliente frecuente!</p>
                <hr style='border: 1px solid #ccc;'/>
                <p style='color: #666; font-size: 12px;'>EcoParking System - Programa de fidelidad</p>
            </body>
            </html>";

            var mensaje = new MailMessage(
                new MailAddress(remitente),
                new MailAddress(this.CorreoUsuario)
            )
            {
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            using var cliente = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(remitente, contrasenaApp),
                EnableSsl = true
            };

            await cliente.SendMailAsync(mensaje);
            Console.WriteLine("📧 Correo de descuento enviado al usuario.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error enviando correo de descuento: {ex.Message}");
        }
    }

    // Guarda en base de datos
    public async Task GuardarEnBaseDeDatosAsync()
    {
        try
        {
            using var context = new EcoParkingContext();

            var existingFidelidad = await context.Fidelidad
                .FirstOrDefaultAsync(f => f.NombreUsuario == this.NombreUsuario);

            if (existingFidelidad != null)
            {
                // Actualizar existente
                existingFidelidad.ReservasRealizadas = this.ReservasRealizadas; // ← Cambiado
                existingFidelidad.NivelFidelidad = this.NivelFidelidad; // ← Nueva
                existingFidelidad.DescuentoAplicado = this.DescuentoAplicado; // ← Nueva
                existingFidelidad.UltimaFechaBeneficio = this.UltimaFechaBeneficio;
                existingFidelidad.FechaUltimaReserva = this.FechaUltimaReserva;
                existingFidelidad.TotalDescuentoAplicado = this.TotalDescuentoAplicado;
                context.Fidelidad.Update(existingFidelidad);
            }
            else
            {
                // Crear nuevo
                context.Fidelidad.Add(this);
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error guardando fidelidad: {ex.Message}");
        }
    }

    // Carga desde base de datos
    public static async Task<Fidelidad> ObtenerPorUsuarioAsync(string nombreUsuario, string correoUsuario = "")
    {
        try
        {
            using var context = new EcoParkingContext();
            var fidelidad = await context.Fidelidad
                .FirstOrDefaultAsync(f => f.NombreUsuario == nombreUsuario);

            return fidelidad ?? new Fidelidad(nombreUsuario, correoUsuario);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo fidelidad: {ex.Message}");
            return new Fidelidad(nombreUsuario, correoUsuario);
        }
    }

    // Obtiene el historial de descuentos de un usuario
    public static async Task<List<Fidelidad>> ObtenerHistorialUsuarioAsync(string nombreUsuario)
    {
        try
        {
            using var context = new EcoParkingContext();
            return await context.Fidelidad
                .Where(f => f.NombreUsuario == nombreUsuario)
                .OrderByDescending(f => f.FechaUltimaReserva)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo historial: {ex.Message}");
            return new List<Fidelidad>();
        }
    }

    // Muestra estadísticas de fidelidad
    public void MostrarEstadisticas()
    {
        Console.WriteLine("\n⭐ Programa de Fidelidad");
        Console.WriteLine($"👤 Usuario: {NombreUsuario}");
        Console.WriteLine($"📊 Reservas realizadas: {ReservasRealizadas}"); // ← Cambiado
        Console.WriteLine($"🏆 Nivel: {NivelFidelidad}"); // ← Nueva
        Console.WriteLine($"🎁 Descuento aplicado: ${DescuentoAplicado:F2}"); // ← Nueva
        Console.WriteLine($"💰 Descuento total aplicado: ${TotalDescuentoAplicado:F2}");
        Console.WriteLine($"📅 Último beneficio: {(UltimaFechaBeneficio == DateTime.MinValue ? "Nunca" : UltimaFechaBeneficio.ToString("dd/MM/yyyy"))}");
        Console.WriteLine($"🔢 Próximo descuento en: {10 - (ReservasRealizadas % 10)} reservas\n"); // ← Cambiado
    }
}