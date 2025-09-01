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

    public GananciasEcoParking() { }

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
            Console.WriteLine($" Error registrando ganancia: {ex.Message}");
        }
    }

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
            Console.WriteLine($" Error calculando total semanal: {ex.Message}");
            return 0;
        }
    }

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
            Console.WriteLine($" Error calculando total mensual: {ex.Message}");
            return 0;
        }
    }

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
            Console.WriteLine($" Error calculando total anual: {ex.Message}");
            return 0;
        }
    }

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
            Console.WriteLine($" Error calculando total histórico: {ex.Message}");
            return 0;
        }
    }

    public static async Task MostrarResumenAsync()
    {
        try
        {
            decimal totalSemana = await TotalSemanaAsync();
            decimal totalMes = await TotalMesAsync();
            decimal totalAnual = await TotalAnualAsync();
            decimal totalHistorico = await TotalHistoricoAsync();

            Console.WriteLine("\n Reporte de Ganancias:");
            Console.WriteLine("========================");
            Console.WriteLine($" Semana actual: ${totalSemana:F2}");
            Console.WriteLine($" Mes actual: ${totalMes:F2}");
            Console.WriteLine($" Año actual: ${totalAnual:F2}");
            Console.WriteLine($" Total histórico: ${totalHistorico:F2}");

            await MostrarEstadisticasDetalladasAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error mostrando resumen: {ex.Message}");
        }
    }

    private static async Task MostrarEstadisticasDetalladasAsync()
    {
        try
        {
            using var context = new EcoParkingContext();

            var porMetodoPago = await context.Ganancias
                .GroupBy(g => g.MetodoPago)
                .Select(g => new { Metodo = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            Console.WriteLine("\n Distribución por método de pago:");
            foreach (var item in porMetodoPago)
            {
                Console.WriteLine($"   {item.Metodo}: ${item.Total:F2}");
            }

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

            var promedioDiario = await context.Ganancias
                .Where(g => g.Fecha >= DateTime.Now.AddDays(-30))
                .AverageAsync(g => (decimal?)g.Monto) ?? 0;

            Console.WriteLine($"\n Promedio diario (últimos 30 días): ${promedioDiario:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error mostrando estadísticas detalladas: {ex.Message}");
        }
    }

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
            Console.WriteLine($" Error obteniendo reporte: {ex.Message}");
            return new List<GananciasEcoParking>();
        }
    }

    public static async Task RegistrarPagoStaticAsync(string concepto, decimal monto, string metodoPago,
                                                    string ubicacion = "", string tipoVehiculo = "", string usuario = "")
    {
        var ganancia = new GananciasEcoParking(concepto, monto, metodoPago, ubicacion, tipoVehiculo, usuario);
        await ganancia.RegistrarPagoAsync();
    }

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

            string nombreArchivo = $"reporte_ganancias_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            await System.IO.File.WriteAllTextAsync(nombreArchivo, csv);

            return $" Reporte generado: {nombreArchivo}";
        }
        catch (Exception ex)
        {
            return $" Error generando reporte: {ex.Message}";
        }
    }

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
                Console.WriteLine($"  Eliminados {registrosAntiguos.Count} registros antiguos de ganancias");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error limpiando registros: {ex.Message}");
        }
    }
}