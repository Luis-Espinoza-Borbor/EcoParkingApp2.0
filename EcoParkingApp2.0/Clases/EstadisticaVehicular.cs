using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

[Table("EstadisticasVehiculares")]
public class EstadisticaVehicular
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TipoVehiculo { get; set; } = string.Empty;

    public int CantidadUsos { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalRecaudado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaUltimoUso { get; set; }

    // Constructor para EF Core
    public EstadisticaVehicular() { }

    // Constructor para crear nuevas estadísticas
    public EstadisticaVehicular(string tipoVehiculo)
    {
        TipoVehiculo = tipoVehiculo;
        CantidadUsos = 0;
        TotalRecaudado = 0;
        FechaRegistro = DateTime.Now;
    }

    public void RegistrarUso(decimal monto = 0)
    {
        CantidadUsos++;
        TotalRecaudado += monto;
        FechaUltimoUso = DateTime.Now;
    }

    public int ObtenerCantidadPorSemana()
    {
        using var context = new EcoParkingContext();
        DateTime hace7Dias = DateTime.Now.AddDays(-7);
        return context.EstadisticasVehiculares
            .Where(e => e.TipoVehiculo == this.TipoVehiculo && e.FechaUltimoUso >= hace7Dias)
            .Sum(e => e.CantidadUsos);
    }

    public int ObtenerCantidadPorMes()
    {
        using var context = new EcoParkingContext();
        DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        return context.EstadisticasVehiculares
            .Where(e => e.TipoVehiculo == this.TipoVehiculo && e.FechaUltimoUso >= inicioMes)
            .Sum(e => e.CantidadUsos);
    }

    public void MostrarResumen()
    {
        using var context = new EcoParkingContext();

        var estadisticas = context.EstadisticasVehiculares
            .Where(e => e.TipoVehiculo == this.TipoVehiculo)
            .ToList();

        int totalUsos = estadisticas.Sum(e => e.CantidadUsos);
        decimal totalRecaudado = estadisticas.Sum(e => e.TotalRecaudado);

        Console.WriteLine("\n📊 Estadísticas de uso vehicular:");
        Console.WriteLine($"🚗 Tipo de vehículo: {TipoVehiculo}");
        Console.WriteLine($"📅 Últimos 7 días: {ObtenerCantidadPorSemana()} usos");
        Console.WriteLine($"📆 Este mes: {ObtenerCantidadPorMes()} usos");
        Console.WriteLine($"📈 Total histórico: {totalUsos} usos");
        Console.WriteLine($"💰 Total recaudado: ${totalRecaudado:F2}\n");
    }

    public async Task GuardarEnBaseDeDatosAsync()
    {
        try
        {
            using var context = new EcoParkingContext();

            // Buscar si ya existe estadística para este tipo de vehículo
            var existingStat = await context.EstadisticasVehiculares
                .FirstOrDefaultAsync(e => e.TipoVehiculo == this.TipoVehiculo);

            if (existingStat != null)
            {
                // Actualizar estadística existente
                existingStat.CantidadUsos = this.CantidadUsos;
                existingStat.TotalRecaudado = this.TotalRecaudado;
                existingStat.FechaUltimoUso = this.FechaUltimoUso;
                context.EstadisticasVehiculares.Update(existingStat);
            }
            else
            {
                // Crear nueva estadística
                context.EstadisticasVehiculares.Add(this);
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error guardando estadísticas: {ex.Message}");
        }
    }

    public static async Task<List<EstadisticaVehicular>> CargarDesdeBaseDeDatosAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            return await context.EstadisticasVehiculares.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error cargando estadísticas: {ex.Message}");
            return new List<EstadisticaVehicular>();
        }
    }

    public static async Task<EstadisticaVehicular> ObtenerPorTipoVehiculoAsync(string tipoVehiculo)
    {
        try
        {
            using var context = new EcoParkingContext();
            var estadistica = await context.EstadisticasVehiculares
                .FirstOrDefaultAsync(e => e.TipoVehiculo == tipoVehiculo);

            return estadistica ?? new EstadisticaVehicular(tipoVehiculo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo estadística: {ex.Message}");
            return new EstadisticaVehicular(tipoVehiculo);
        }
    }
}