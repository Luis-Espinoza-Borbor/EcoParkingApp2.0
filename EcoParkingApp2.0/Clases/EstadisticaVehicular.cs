using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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

    // Métodos estáticos para evitar problemas de conexión a BD
    public static int ObtenerCantidadPorSemana(string tipoVehiculo)
    {
        try
        {
            using var context = new EcoParkingContext();
            DateTime hace7Dias = DateTime.Now.AddDays(-7);
            return context.EstadisticasVehiculares
                .Where(e => e.TipoVehiculo == tipoVehiculo && e.FechaUltimoUso >= hace7Dias)
                .Sum(e => e.CantidadUsos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo estadísticas semanales: {ex.Message}");
            return 0;
        }
    }

    public static int ObtenerCantidadPorMes(string tipoVehiculo)
    {
        try
        {
            using var context = new EcoParkingContext();
            DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return context.EstadisticasVehiculares
                .Where(e => e.TipoVehiculo == tipoVehiculo && e.FechaUltimoUso >= inicioMes)
                .Sum(e => e.CantidadUsos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error obteniendo estadísticas mensuales: {ex.Message}");
            return 0;
        }
    }

    public void MostrarResumen()
    {
        try
        {
            using var context = new EcoParkingContext();

            var estadisticas = context.EstadisticasVehiculares
                .Where(e => e.TipoVehiculo == this.TipoVehiculo)
                .ToList();

            int totalUsos = estadisticas.Sum(e => e.CantidadUsos);
            decimal totalRecaudado = estadisticas.Sum(e => e.TotalRecaudado);

            Console.WriteLine("\n📊 Estadísticas de uso vehicular:");
            Console.WriteLine($"🚗 Tipo de vehículo: {TipoVehiculo}");
            Console.WriteLine($"📅 Últimos 7 días: {ObtenerCantidadPorSemana(TipoVehiculo)} usos");
            Console.WriteLine($"📆 Este mes: {ObtenerCantidadPorMes(TipoVehiculo)} usos");
            Console.WriteLine($"📈 Total histórico: {totalUsos} usos");
            Console.WriteLine($"💰 Total recaudado: ${totalRecaudado:F2}");
            Console.WriteLine($"🕐 Último uso: {(FechaUltimoUso.HasValue ? FechaUltimoUso.Value.ToString("yyyy-MM-dd HH:mm") : "Nunca")}");
            Console.WriteLine("==========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error mostrando resumen: {ex.Message}");
        }
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
            var estadisticas = await context.EstadisticasVehiculares.ToListAsync();

            // Si no hay estadísticas, crear unas por defecto
            if (!estadisticas.Any())
            {
                estadisticas = new List<EstadisticaVehicular>
                {
                    new EstadisticaVehicular("Auto"),
                    new EstadisticaVehicular("Moto"),
                    new EstadisticaVehicular("Camioneta")
                };

                context.EstadisticasVehiculares.AddRange(estadisticas);
                await context.SaveChangesAsync();
            }

            return estadisticas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error cargando estadísticas: {ex.Message}");

            // Retornar estadísticas por defecto si hay error
            return new List<EstadisticaVehicular>
            {
                new EstadisticaVehicular("Auto"),
                new EstadisticaVehicular("Moto"),
                new EstadisticaVehicular("Camioneta")
            };
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

    // Método para actualizar estadísticas cuando se hace un pago
    public static async Task ActualizarEstadisticasPorPagoAsync(string tipoVehiculo, decimal monto)
    {
        try
        {
            var estadistica = await ObtenerPorTipoVehiculoAsync(tipoVehiculo);
            estadistica.RegistrarUso(monto);
            await estadistica.GuardarEnBaseDeDatosAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error actualizando estadísticas por pago: {ex.Message}");
        }
    }
}