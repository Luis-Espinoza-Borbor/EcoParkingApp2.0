using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

[Table("Ganancias")]
public class GananciasEcoParking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Concepto { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    [Required]
    [MaxLength(50)]
    public string MetodoPago { get; set; } = string.Empty;

    [MaxLength(100)]
    public string UbicacionParqueo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string TipoVehiculo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Usuario { get; set; } = string.Empty;

    // Constructor para EF Core
    public GananciasEcoParking() { }

    // Constructor para nuevos registros
    public GananciasEcoParking(string concepto, decimal monto, string metodoPago,
                             string ubicacion = "", string tipoVehiculo = "", string usuario = "")
    {
        Concepto = concepto;
        Monto = monto;
        MetodoPago = metodoPago;
        UbicacionParqueo = ubicacion;
        TipoVehiculo = tipoVehiculo;
        Usuario = usuario;
        Fecha = DateTime.Now;
    }

    // Registra un nuevo pago
    public async Task RegistrarPagoAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            context.Ganancias.Add(this);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error registrando ganancia: {ex.Message}");
        }
    }

    // Total de la semana actual
    public static async Task<decimal> TotalSemanaAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            DateTime hace7Dias = DateTime.Now.AddDays(-7);
            return await context.Ganancias
                .Where(g => g.Fecha >= hace7Dias)
                .SumAsync(g => g.Monto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error calculando total semanal: {ex.Message}");
            return 0;
        }
    }

    // Total del mes actual
    public static async Task<decimal> TotalMesAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await context.Ganancias
                .Where(g => g.Fecha >= inicioMes)
                .SumAsync(g => g.Monto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error calculando total mensual: {ex.Message}");
            return 0;
        }
    }

    // Total del año actual
    public static async Task<decimal> TotalAnualAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            DateTime inicioAño = new DateTime(DateTime.Now.Year, 1, 1);
            return await context.Ganancias
                .Where(g => g.Fecha >= inicioAño)
                .SumAsync(g => g.Monto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error calculando total anual: {ex.Message}");
            return 0;
        }
    }

    // Total histórico
    public static async Task<decimal> TotalHistoricoAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            return await context.Ganancias
                .SumAsync(g => g.Monto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error calculando total histórico: {ex.Message}");
            return 0;
        }
    }

    // Muestra resumen de ganancias
    public static async Task MostrarResumenAsync()
    {
        try
        {
            decimal totalSemana = await TotalSemanaAsync();
            decimal totalMes = await TotalMesAsync();
            decimal totalAnual = await TotalAnualAsync();
            decimal totalHistorico = await TotalHistoricoAsync();

            Console.WriteLine("\n💰 Reporte de Ganancias:");
            Console.WriteLine("========================");
            Console.WriteLine($"📅 Semana actual: ${totalSemana:F2}");
            Console.WriteLine($"🗓️  Mes actual: ${totalMes:F2}");
            Console.WriteLine($"📊 Año actual: ${totalAnual:F2}");
            Console.WriteLine($"🏆 Total histórico: ${totalHistorico:F2}");

            // Estadísticas adicionales
            await MostrarEstadisticasDetalladasAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error mostrando resumen: {ex.Message}");
        }
    }

    // Estadísticas detalladas
    private static async Task MostrarEstadisticasDetalladasAsync()
    {
        try
        {
            using var context = new EcoParkingContext();

            // Por método de pago
            var porMetodoPago = await context.Ganancias
                .GroupBy(g => g.MetodoPago)
                .Select(g => new { Metodo = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            Console.WriteLine("\n💳 Distribución por método de pago:");
            foreach (var item in porMetodoPago)
            {
                Console.WriteLine($"   {item.Metodo}: ${item.Total:F2}");
            }

            // Por ubicación
            var porUbicacion = await context.Ganancias
                .Where(g => !string.IsNullOrEmpty(g.UbicacionParqueo))
                .GroupBy(g => g.UbicacionParqueo)
                .Select(g => new { Ubicacion = g.Key, Total = g.Sum(x => x.Monto) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            if (porUbicacion.Any())
            {
                Console.WriteLine("\n📍 Top 5 ubicaciones por ganancias:");
                foreach (var item in porUbicacion)
                {
                    Console.WriteLine($"   {item.Ubicacion}: ${item.Total:F2}");
                }
            }

            // Promedio diario
            var promedioDiario = await context.Ganancias
                .Where(g => g.Fecha >= DateTime.Now.AddDays(-30))
                .AverageAsync(g => (decimal?)g.Monto) ?? 0;

            Console.WriteLine($"\n📈 Promedio diario (últimos 30 días): ${promedioDiario:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error mostrando estadísticas detalladas: {ex.Message}");
        }
    }

    // Obtiene reporte por rango de fechas
    public static async Task<List<GananciasEcoParking>> ObtenerReportePorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            using var context = new EcoParkingContext();
            return await context.Ganancias
                .Where(g => g.Fecha >= fechaInicio && g.Fecha <= fechaFin)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo reporte: {ex.Message}");
            return new List<GananciasEcoParking>();
        }
    }

    // Método estático para registrar pago fácilmente
    public static async Task RegistrarPagoStaticAsync(string concepto, decimal monto, string metodoPago,
                                                    string ubicacion = "", string tipoVehiculo = "", string usuario = "")
    {
        var ganancia = new GananciasEcoParking(concepto, monto, metodoPago, ubicacion, tipoVehiculo, usuario);
        await ganancia.RegistrarPagoAsync();
    }

    // Genera reporte CSV
    public static async Task<string> GenerarReporteCSVAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var reporte = await ObtenerReportePorFechaAsync(fechaInicio, fechaFin);

            if (!reporte.Any())
                return "No hay datos para el período seleccionado";

            string csv = "Fecha,Concepto,Monto,MetodoPago,Ubicacion,TipoVehiculo,Usuario\n";

            foreach (var item in reporte)
            {
                csv += $"{item.Fecha:yyyy-MM-dd HH:mm},{item.Concepto},{item.Monto},{item.MetodoPago}," +
                       $"{item.UbicacionParqueo},{item.TipoVehiculo},{item.Usuario}\n";
            }

            // Guardar archivo
            string nombreArchivo = $"reporte_ganancias_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            await System.IO.File.WriteAllTextAsync(nombreArchivo, csv);

            return $"✅ Reporte generado: {nombreArchivo}";
        }
        catch (Exception ex)
        {
            return $"❌ Error generando reporte: {ex.Message}";
        }
    }

    // Limpia registros antiguos (más de 2 años)
    public static async Task LimpiarRegistrosAntiguosAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            var fechaLimite = DateTime.Now.AddYears(-2);
            var registrosAntiguos = await context.Ganancias
                .Where(g => g.Fecha < fechaLimite)
                .ToListAsync();

            if (registrosAntiguos.Any())
            {
                context.Ganancias.RemoveRange(registrosAntiguos);
                await context.SaveChangesAsync();
                Console.WriteLine($"🗑️  Eliminados {registrosAntiguos.Count} registros antiguos de ganancias");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error limpiando registros: {ex.Message}");
        }
    }
}