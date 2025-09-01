using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

[Table("FlujoPersonas")]
public class FlujoPersonas
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombrePersona { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TipoAcceso { get; set; } = string.Empty;

    public DateTime FechaEntrada { get; set; }

    [MaxLength(10)]
    public string HoraEntrada { get; set; } = string.Empty;

    [MaxLength(20)]
    public string DiaSemana { get; set; } = string.Empty;

    public FlujoPersonas() { }

    public FlujoPersonas(string nombre, string tipoAcceso)
    {
        NombrePersona = nombre;
        TipoAcceso = tipoAcceso;
        FechaEntrada = DateTime.Now;
        HoraEntrada = DateTime.Now.ToString("HH:mm:ss");
        DiaSemana = DateTime.Now.DayOfWeek.ToString();
    }

    public async Task RegistrarEntradaAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            context.FlujoPersonas.Add(this);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error registrando entrada: {ex.Message}");
        }
    }

    public static async Task MostrarHistorialAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            var entradas = await context.FlujoPersonas
                .OrderByDescending(f => f.FechaEntrada)
                .ToListAsync();

            Console.WriteLine("\n Historial de entradas a la aplicación:");
            Console.WriteLine("===========================================");

            if (!entradas.Any())
            {
                Console.WriteLine(" No se han registrado entradas aún.");
                return;
            }

            foreach (var entrada in entradas)
            {
                Console.WriteLine($" {entrada.FechaEntrada:yyyy-MM-dd HH:mm:ss} | {entrada.TipoAcceso} | {entrada.NombrePersona}");
            }

            await MostrarEstadisticasAsync(entradas);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error mostrando historial: {ex.Message}");
        }
    }

    private static async Task MostrarEstadisticasAsync(List<FlujoPersonas> entradas)
    {
        var hoy = DateTime.Today;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        int totalEntradas = entradas.Count;
        int hoyEntradas = entradas.Count(e => e.FechaEntrada.Date == hoy);
        int semanaEntradas = entradas.Count(e => e.FechaEntrada >= inicioSemana);
        int mesEntradas = entradas.Count(e => e.FechaEntrada >= inicioMes);

        var porTipo = entradas.GroupBy(e => e.TipoAcceso)
            .Select(g => new { Tipo = g.Key, Count = g.Count() });

        Console.WriteLine("\n Estadísticas de flujo:");
        Console.WriteLine($" Total histórico: {totalEntradas} entradas");
        Console.WriteLine($" Hoy: {hoyEntradas} entradas");
        Console.WriteLine($" Esta semana: {semanaEntradas} entradas");
        Console.WriteLine($" Este mes: {mesEntradas} entradas");

        Console.WriteLine("\n Distribución por tipo:");
        foreach (var grupo in porTipo)
        {
            double porcentaje = (double)grupo.Count / totalEntradas * 100;
            Console.WriteLine($"   {grupo.Tipo}: {grupo.Count} ({porcentaje:F1}%)");
        }

        var horasPico = entradas.GroupBy(e => e.FechaEntrada.Hour)
            .Select(g => new { Hora = g.Key, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .Take(3);

        Console.WriteLine("\n Horas pico de acceso:");
        foreach (var hora in horasPico)
        {
            Console.WriteLine($"   {hora.Hora:00}:00 - {hora.Hora + 1:00}:00: {hora.Cantidad} accesos");
        }
    }

    public static async Task<int> CantidadPorDiaAsync(DateTime dia)
    {
        try
        {
            using var context = new EcoParkingContext();
            return await context.FlujoPersonas
                .Where(f => f.FechaEntrada.Date == dia.Date)
                .CountAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error contando entradas: {ex.Message}");
            return 0;
        }
    }

    public static async Task<List<FlujoPersonas>> ObtenerReportePorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            using var context = new EcoParkingContext();
            return await context.FlujoPersonas
                .Where(f => f.FechaEntrada >= fechaInicio && f.FechaEntrada <= fechaFin)
                .OrderBy(f => f.FechaEntrada)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error obteniendo reporte: {ex.Message}");
            return new List<FlujoPersonas>();
        }
    }

    public static async Task MostrarEstadisticasPorDiaAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            var estadisticas = await context.FlujoPersonas
                .GroupBy(f => f.DiaSemana)
                .Select(g => new { Dia = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            Console.WriteLine("\n Accesos por día de la semana:");
            foreach (var stat in estadisticas)
            {
                Console.WriteLine($"   {stat.Dia}: {stat.Count} accesos");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error mostrando estadísticas por día: {ex.Message}");
        }
    }

    public static async Task RegistrarEntradaStaticAsync(string nombre, string tipoAcceso)
    {
        var flujo = new FlujoPersonas(nombre, tipoAcceso);
        await flujo.RegistrarEntradaAsync();
    }

    public static async Task LimpiarRegistrosAntiguosAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            var fechaLimite = DateTime.Now.AddYears(-1);
            var registrosAntiguos = await context.FlujoPersonas
                .Where(f => f.FechaEntrada < fechaLimite)
                .ToListAsync();

            if (registrosAntiguos.Any())
            {
                context.FlujoPersonas.RemoveRange(registrosAntiguos);
                await context.SaveChangesAsync();
                Console.WriteLine($"  Eliminados {registrosAntiguos.Count} registros antiguos");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error limpiando registros: {ex.Message}");
        }
    }
}