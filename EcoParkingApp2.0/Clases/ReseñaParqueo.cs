using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoParkingApp;

[Table("ReseñasParqueo")]
public class ReseñaParqueo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string IdParqueo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Usuario { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Puntuacion { get; set; }

    [MaxLength(500)]
    public string Comentario { get; set; } = string.Empty;

    [Required]
    public DateTime Fecha { get; set; }

    public ReseñaParqueo() { }

    public ReseñaParqueo(string idParqueo, string usuario, int puntuacion, string comentario)
    {
        IdParqueo = idParqueo;
        Usuario = usuario;
        Puntuacion = puntuacion;
        Comentario = comentario;
        Fecha = DateTime.Now;
    }

    public override string ToString()
    {
        return $"Parqueo: {IdParqueo} | Usuario: {Usuario} | Puntuación: {Puntuacion}/5 | Fecha: {Fecha:yyyy-MM-dd} | Comentario: {Comentario}";
    }

    public static async Task MostrarAsync()
    {
        try
        {
            await using var context = new EcoParkingContext();
            var reseñas = await context.ReseñasParqueo
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            Console.WriteLine("\n--- RESEÑAS DE PARQUEOS ---");

            if (reseñas.Count == 0)
            {
                Console.WriteLine("No hay reseñas disponibles.");
                return;
            }

            foreach (var r in reseñas)
            {
                Console.WriteLine($"⭐ {r.Puntuacion}/5 - {r.Usuario} ({r.Fecha:yyyy-MM-dd}):");
                Console.WriteLine($"   Parqueo: {r.IdParqueo}");
                if (!string.IsNullOrEmpty(r.Comentario))
                {
                    Console.WriteLine($"   Comentario: {r.Comentario}");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al cargar reseñas: {ex.Message}");
        }
    }

    public static async Task GuardarAsync(ReseñaParqueo reseña)
    {
        try
        {
            await using var context = new EcoParkingContext();
            context.ReseñasParqueo.Add(reseña);
            await context.SaveChangesAsync();
            Console.WriteLine(" Reseña guardada exitosamente!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al guardar reseña: {ex.Message}");
        }
    }

    public static async Task<decimal> ObtenerPuntuacionPromedioAsync(string idParqueo)
    {
        try
        {
            await using var context = new EcoParkingContext();
            var promedio = await context.ReseñasParqueo
                .Where(r => r.IdParqueo == idParqueo)
                .AverageAsync(r => (decimal?)r.Puntuacion) ?? 0;

            return Math.Round(promedio, 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al calcular puntuación promedio: {ex.Message}");
            return 0;
        }
    }

    public static async Task<List<ReseñaParqueo>> ObtenerPorParqueoAsync(string idParqueo)
    {
        try
        {
            await using var context = new EcoParkingContext();
            return await context.ReseñasParqueo
                .Where(r => r.IdParqueo == idParqueo)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al obtener reseñas: {ex.Message}");
            return new List<ReseñaParqueo>();
        }
    }

    public static async Task MostrarEstadisticasAsync()
    {
        try
        {
            await using var context = new EcoParkingContext();

            var totalReseñas = await context.ReseñasParqueo.CountAsync();
            var promedioGeneral = await context.ReseñasParqueo
                .AverageAsync(r => (decimal?)r.Puntuacion) ?? 0;

            var reseñasPorPuntuacion = await context.ReseñasParqueo
                .GroupBy(r => r.Puntuacion)
                .Select(g => new { Puntuacion = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Puntuacion)
                .ToListAsync();

            Console.WriteLine("\n--- ESTADÍSTICAS DE RESEÑAS ---");
            Console.WriteLine($"Total de reseñas: {totalReseñas}");
            Console.WriteLine($"Puntuación promedio: {Math.Round(promedioGeneral, 1)}/5");
            Console.WriteLine("\nDistribución por puntuación:");

            foreach (var item in reseñasPorPuntuacion)
            {
                var porcentaje = totalReseñas > 0 ? (item.Cantidad * 100.0 / totalReseñas) : 0;
                Console.WriteLine($"  {item.Puntuacion} estrellas: {item.Cantidad} ({porcentaje:F1}%)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al cargar estadísticas: {ex.Message}");
        }
    }

    public static async Task<bool> EliminarReseñaAsync(int idReseña)
    {
        try
        {
            await using var context = new EcoParkingContext();
            var reseña = await context.ReseñasParqueo.FindAsync(idReseña);

            if (reseña != null)
            {
                context.ReseñasParqueo.Remove(reseña);
                await context.SaveChangesAsync();
                Console.WriteLine(" Reseña eliminada exitosamente!");
                return true;
            }

            Console.WriteLine(" Reseña no encontrada.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al eliminar reseña: {ex.Message}");
            return false;
        }
    }
}