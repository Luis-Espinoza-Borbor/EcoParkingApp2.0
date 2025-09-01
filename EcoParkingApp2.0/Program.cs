using EcoParking_Proyecto;
using EcoParkingApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "EcoParking System";

        await VerificacionesSilenciosasAsync();

        bool salir = false;

        while (!salir)
        {
            Console.Clear();
            Console.WriteLine("\n=== BIENVENIDO A ECOPARKING ===");
            Console.WriteLine("1. Administrador");
            Console.WriteLine("2. Usuario");
            Console.WriteLine("3. Salir del sistema");
            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine()?.Trim();

            switch (opcion)
            {
                case "1":
                    await MenuTipoAccesoAsync("Administrador");
                    break;
                case "2":
                    await MenuTipoAccesoAsync("Usuario");
                    break;
                case "3":
                    salir = true;
                    Console.WriteLine(" ¡Hasta pronto!");
                    break;
                default:
                    Console.WriteLine(" Opción inválida.");
                    await PresionarParaContinuar();
                    break;
            }
        }
    }

    private static async Task VerificacionesSilenciosasAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            await context.Database.EnsureCreatedAsync();

            var sqlCommands = new[]
            {
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Fidelidad' AND COLUMN_NAME = 'ReservasRealizadas') BEGIN ALTER TABLE Fidelidad ADD ReservasRealizadas INT NOT NULL DEFAULT 0 END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Fidelidad' AND COLUMN_NAME = 'NivelFidelidad') BEGIN ALTER TABLE Fidelidad ADD NivelFidelidad NVARCHAR(50) NOT NULL DEFAULT 'Bronce' END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Fidelidad' AND COLUMN_NAME = 'DescuentoAplicado') BEGIN ALTER TABLE Fidelidad ADD DescuentoAplicado DECIMAL(5,2) NOT NULL DEFAULT 0 END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Citaciones' AND COLUMN_NAME = 'Motivo') BEGIN ALTER TABLE Citaciones ADD Motivo NVARCHAR(200) NOT NULL DEFAULT 'Tiempo excedido' END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Citaciones' AND COLUMN_NAME = 'MontoMulta') BEGIN ALTER TABLE Citaciones ADD MontoMulta DECIMAL(10,2) NOT NULL DEFAULT 0 END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Citaciones' AND COLUMN_NAME = 'Pagada') BEGIN ALTER TABLE Citaciones ADD Pagada BIT NOT NULL DEFAULT 0 END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Citaciones' AND COLUMN_NAME = 'HoraInicioReserva') BEGIN ALTER TABLE Citaciones ADD HoraInicioReserva DATETIME2 NOT NULL DEFAULT GETDATE() END",
                @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Citaciones' AND COLUMN_NAME = 'MinutosExcedidos') BEGIN ALTER TABLE Citaciones ADD MinutosExcedidos INT NOT NULL DEFAULT 0 END"
            };

            foreach (var sql in sqlCommands)
            {
                try { await context.Database.ExecuteSqlRawAsync(sql); } catch { }
            }

            if (!await context.Administradores.AnyAsync())
            {
                var admin = new Administrador("Admin Principal", "admin123", "admin123");
                context.Administradores.Add(admin);
                await context.SaveChangesAsync();
            }

            if (!await context.Parqueos.AnyAsync())
            {
                var parqueos = new[]
                {
                    new EcoParking("Guayaquil-Centro", "Auto", 50, 1.50m, "GYE123"),
                    new EcoParking("Guayaquil-Norte", "Moto", 30, 1.00m, "GYN456"),
                    new EcoParking("Samborondón", "Camioneta", 20, 2.00m, "SAM789")
                };
                context.Parqueos.AddRange(parqueos);
                await context.SaveChangesAsync();
            }
        }
        catch { }
    }

    static async Task MenuTipoAccesoAsync(string tipoUsuario)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n=== {tipoUsuario.ToUpper()} ===");
            Console.WriteLine("1. Registrarse");
            Console.WriteLine("2. Iniciar sesión");
            Console.WriteLine("3. Volver al menú principal");
            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine()?.Trim();

            switch (opcion)
            {
                case "1":
                    if (tipoUsuario == "Administrador")
                    {
                        using var context = new EcoParkingContext();
                        if (await context.Administradores.AnyAsync())
                        {
                            Console.WriteLine(" Solo puede haber un administrador. Contacte al existente.");
                            await PresionarParaContinuar();
                        }
                        else
                        {
                            await RegistrarAdministradorAsync();
                        }
                    }
                    else
                    {
                        await Usuario.RegistrarNuevoUsuarioAsync();
                        await PresionarParaContinuar();
                    }
                    break;
                case "2":
                    if (tipoUsuario == "Administrador")
                    {
                        var admin = await Administrador.IniciarSesionEFAsync();
                        if (admin != null)
                            await MenuAdministradorAsync(admin);
                        else
                            await PresionarParaContinuar();
                    }
                    else
                    {
                        var usuario = await Usuario.IniciarSesionAsync();
                        if (usuario != null)
                            await MenuUsuarioAsync(usuario);
                        else
                            await PresionarParaContinuar();
                    }
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine(" Opción inválida.");
                    await PresionarParaContinuar();
                    break;
            }
        }
    }

    static async Task RegistrarAdministradorAsync()
    {
        Console.WriteLine("\n=== REGISTRO DE ADMINISTRADOR ===");
        Console.Write("Nombre completo: ");
        string nombre = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Identificación: ");
        string identificacion = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Contraseña: ");
        string contraseña = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(identificacion) || string.IsNullOrEmpty(contraseña))
        {
            Console.WriteLine(" Todos los campos son obligatorios.");
            await PresionarParaContinuar();
            return;
        }

        try
        {
            using var context = new EcoParkingContext();
            bool existe = await context.Administradores.AnyAsync(a => a.Identificacion == identificacion);
            if (existe)
            {
                Console.WriteLine(" Ya existe un administrador con esa identificación.");
                await PresionarParaContinuar();
                return;
            }

            var nuevoAdmin = new Administrador(nombre, identificacion, contraseña);
            context.Administradores.Add(nuevoAdmin);
            await context.SaveChangesAsync();
            Console.WriteLine(" Administrador registrado exitosamente!");
            await PresionarParaContinuar();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error al registrar administrador: {ex.Message}");
            await PresionarParaContinuar();
        }
    }

    private static async Task PresionarParaContinuar()
    {
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    public static void MostrarMenuConMarco(string titulo, string[] opciones)
    {
        int ancho = 50;
        string borde = new string('═', ancho);
        Console.WriteLine($"\n╔{borde}╗");
        Console.WriteLine($"║{titulo.PadLeft((ancho + titulo.Length) / 2).PadRight(ancho)}║");
        Console.WriteLine($"╠{borde}╣");
        for (int i = 0; i < opciones.Length; i++)
        {
            string texto = $" {i + 1}. {opciones[i]}";
            Console.WriteLine($"║{texto.PadRight(ancho)}║");
        }
        Console.WriteLine($"╚{borde}╝");
        Console.Write("Seleccione una opción: ");
    }

    static async Task MenuAdministradorAsync(Administrador admin)
    {
        while (true)
        {
            Console.Clear();
            string[] opciones = {
                "Ver parqueos", "Modificar cantidad disponible", "Actualizar tarifa",
                "Ver estadísticas vehiculares", "Ver flujo de personas", "Ver ganancias",
                "Ver reseñas", "Cerrar sesión"
            };
            MostrarMenuConMarco("MENÚ ADMINISTRADOR", opciones);
            string opcion = Console.ReadLine()?.Trim();

            switch (opcion)
            {
                case "1": await MostrarParqueosAsync(); await PresionarParaContinuar(); break;
                case "2": await ModificarCantidadAsync(admin); await PresionarParaContinuar(); break;
                case "3": await ActualizarTarifaAsync(admin); await PresionarParaContinuar(); break;
                case "4": await MostrarEstadisticasVehicularesAsync(); await PresionarParaContinuar(); break;
                case "5": await FlujoPersonas.MostrarHistorialAsync(); await PresionarParaContinuar(); break;
                case "6": await GananciasEcoParking.MostrarResumenAsync(); await PresionarParaContinuar(); break;
                case "7": await ReseñaParqueo.MostrarEstadisticasAsync(); await PresionarParaContinuar(); break;
                case "8": Console.WriteLine(" Cerrando sesión de administrador..."); return;
                default: Console.WriteLine(" Opción inválida."); await PresionarParaContinuar(); break;
            }
        }
    }

    static async Task MenuUsuarioAsync(Usuario usuario)
    {
        await FlujoPersonas.RegistrarEntradaStaticAsync(usuario.Nombre, "Usuario");
        while (true)
        {
            Console.Clear();
            string[] opciones = {
                "Ver parqueos disponibles", "Reservar espacio", "Realizar pago",
                "Dejar reseña", "Ver puntuación promedio", "Ver citaciones pendientes", "Cerrar sesión"
            };
            Usuario.MostrarMenuConMarco("MENÚ USUARIO", opciones);
            string opcion = Console.ReadLine()?.Trim();

            switch (opcion)
            {
                case "1": await MostrarParqueosDisponiblesAsync(); await PresionarParaContinuar(); break;
                case "2": await ReservarEspacioAsync(usuario); await PresionarParaContinuar(); break;
                case "3": await RealizarPagoAsync(usuario); await PresionarParaContinuar(); break;
                case "4": await DejarReseñaAsync(usuario); await PresionarParaContinuar(); break;
                case "5": await MostrarPromedioReseñasAsync(); await PresionarParaContinuar(); break;
                case "6": await VerCitacionesPendientesAsync(usuario); await PresionarParaContinuar(); break;
                case "7": Console.WriteLine(" Cerrando sesión de usuario..."); return;
                default: Console.WriteLine(" Opción inválida."); await PresionarParaContinuar(); break;
            }
        }
    }

    static async Task MostrarParqueosAsync()
    {
        using var context = new EcoParkingContext();
        var parqueos = await context.Parqueos.OrderBy(p => p.Ubicacion).ToListAsync();

        if (!parqueos.Any())
        {
            Console.WriteLine(" No hay parqueos en el system.");
            return;
        }

        Console.WriteLine("\n TODOS LOS PARQUEOS:");
        Console.WriteLine("==================================================");

        foreach (var p in parqueos)
        {
            string estado = p.CantidadDisponible > 0 ? " DISPONIBLE" : " AGOTADO";
            string icono = p.TipoVehiculo switch { "Auto" => "", "Moto" => "", "Camioneta" => "", _ => "" };

            Console.WriteLine($"{icono} {p.Ubicacion} ({p.TipoVehiculo})");
            Console.WriteLine($"   Espacios: {p.CantidadDisponible} | Tarifa: ${p.TarifaPorHora:F2}/hora");
            Console.WriteLine($"   Estado: {estado} | Código: {p.CodigoReserva}");

            if (p.HoraReserva != null && !p.PagoRealizado)
            {
                Console.WriteLine($"     RESERVA ACTIVA - Hora fin: {p.HoraFinReserva:HH:mm}");
            }
            Console.WriteLine("--------------------------------------------------");
        }
        Console.WriteLine($"Total de parqueos: {parqueos.Count}");
    }

    static async Task MostrarParqueosDisponiblesAsync()
    {
        using var context = new EcoParkingContext();
        var parqueos = await context.Parqueos
            .Where(p => p.CantidadDisponible > 0)
            .OrderBy(p => p.Ubicacion)
            .ToListAsync();

        if (!parqueos.Any())
        {
            Console.WriteLine(" No hay parqueos disponibles en el system.");
            return;
        }

        Console.WriteLine("\n PARQUEOS DISPONIBLES:");
        Console.WriteLine("==================================================");

        foreach (var p in parqueos)
        {
            string icono = p.TipoVehiculo switch { "Auto" => "", "Moto" => "", "Camioneta" => "", _ => "" };

            Console.WriteLine($"{icono} {p.Ubicacion} ({p.TipoVehiculo})");
            Console.WriteLine($"   Espacios: {p.CantidadDisponible} | Tarifa: ${p.TarifaPorHora:F2}/hora");
            Console.WriteLine($"   Código: {p.CodigoReserva}");
            Console.WriteLine("--------------------------------------------------");
        }
        Console.WriteLine($"Total de parqueos disponibles: {parqueos.Count}");
    }

    static async Task ReservarEspacioAsync(Usuario usuario)
    {
        using var context = new EcoParkingContext();
        var parqueos = await context.Parqueos
            .Where(p => p.CantidadDisponible > 0)
            .ToListAsync();

        if (!parqueos.Any())
        {
            Console.WriteLine(" No hay parqueos disponibles para reservar.");
            return;
        }

        Console.WriteLine("\nSeleccione parqueo para reservar:");
        for (int i = 0; i < parqueos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {parqueos[i].Ubicacion} ({parqueos[i].TipoVehiculo}) - {parqueos[i].CantidadDisponible} espacios - ${parqueos[i].TarifaPorHora:F2}/hora");
        }

        Console.Write("Opción: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > parqueos.Count)
        {
            Console.WriteLine(" Selección inválida.");
            return;
        }

        var seleccionado = parqueos[index - 1];

        Console.WriteLine("\n TIEMPO DE RESERVA");
        Console.WriteLine("1. Por horas");
        Console.WriteLine("2. Por minutos");
        Console.Write("Seleccione opción: ");
        string tiempoOp = Console.ReadLine()?.Trim() ?? "";

        TimeSpan tiempoReserva = TimeSpan.Zero;

        if (tiempoOp == "1")
        {
            Console.Write("Ingrese número de horas: ");
            if (int.TryParse(Console.ReadLine(), out int horas) && horas > 0)
            {
                tiempoReserva = TimeSpan.FromHours(horas);
            }
            else
            {
                Console.WriteLine(" Horas inválidas.");
                return;
            }
        }
        else if (tiempoOp == "2")
        {
            Console.Write("Ingrese número de minutos: ");
            if (int.TryParse(Console.ReadLine(), out int minutos) && minutos > 0)
            {
                tiempoReserva = TimeSpan.FromMinutes(minutos);
            }
            else
            {
                Console.WriteLine(" Minutos inválidos.");
                return;
            }
        }
        else
        {
            Console.WriteLine(" Opción inválida.");
            return;
        }

        seleccionado.ReservarEspacio(tiempoReserva);
        context.Parqueos.Update(seleccionado);
        await context.SaveChangesAsync();
    }

    static async Task RealizarPagoAsync(Usuario usuario)
    {
        using var context = new EcoParkingContext();

        var todosParqueos = await context.Parqueos.ToListAsync();
        var parqueos = todosParqueos.Where(p => p.HoraReserva != null && !p.PagoRealizado).ToList();

        if (!parqueos.Any())
        {
            Console.WriteLine(" No hay reservas activas pendientes de pago.");
            return;
        }

        Console.WriteLine("\nSeleccione reserva para pagar:");
        for (int i = 0; i < parqueos.Count; i++)
        {
            var parqueo = parqueos[i];
            var tiempoReservado = parqueo.HoraFinReserva.Value - parqueo.HoraReserva.Value;
            Console.WriteLine($"{i + 1}. {parqueo.Ubicacion} ({parqueo.TipoVehiculo}) - Tiempo reservado: {tiempoReservado.TotalMinutes}min - Hora fin: {parqueo.HoraFinReserva:HH:mm}");
        }

        Console.Write("Opción: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > parqueos.Count)
        {
            Console.WriteLine(" Selección inválida.");
            return;
        }

        var seleccionado = parqueos[index - 1];
        var (monto, metodoPago, infoTarjeta) = seleccionado.MenuDePago();

        if (monto > 0)
        {
            TimeSpan tiempoReservado = seleccionado.HoraFinReserva.Value - seleccionado.HoraReserva.Value;
            TimeSpan tiempoPagado = TimeSpan.FromHours((double)(monto / seleccionado.TarifaPorHora));

            var fidelidad = await Fidelidad.ObtenerPorUsuarioAsync(usuario.Nombre, usuario.Correo);
            decimal montoFinal = await fidelidad.VerificarYAplicarDescuentoAsync(monto);

            await GananciasEcoParking.RegistrarPagoStaticAsync(
                "Pago de parqueo",
                montoFinal * 1.12m,
                metodoPago.ToLower(),
                seleccionado.Ubicacion,
                seleccionado.TipoVehiculo,
                usuario.Nombre
            );

            var correo = new Correo(usuario.Correo, usuario.Nombre, metodoPago, (double)montoFinal);
            correo.EnviarComprobante();

            await fidelidad.RegistrarReservaAsync();

            await EstadisticaVehicular.ActualizarEstadisticasPorPagoAsync(seleccionado.TipoVehiculo, montoFinal);

            if (tiempoPagado < tiempoReservado)
            {
                Console.WriteLine($"\n  ATENCIÓN: Has pagado {tiempoPagado.TotalMinutes} minutos de {tiempoReservado.TotalMinutes} minutos reservados.");
                Console.WriteLine(" Se generará una citación por el tiempo excedido.");

                await CitacionParqueo.GenerarPorTiempoExcedidoAsync(
                    usuario,
                    seleccionado,
                    tiempoReservado,
                    tiempoPagado
                );

                await PresionarParaContinuar();
            }

            seleccionado.PagoRealizado = true;
            seleccionado.LiberarEspacio();

            using var contextUpdate = new EcoParkingContext();
            contextUpdate.Parqueos.Update(seleccionado);
            await contextUpdate.SaveChangesAsync();

            Console.WriteLine(" Pago registrado y fidelidad actualizada.");
        }
    }

    static async Task VerCitacionesPendientesAsync(Usuario usuario)
    {
        var citaciones = await CitacionParqueo.ObtenerCitacionesPendientesAsync(usuario.Nombre);

        if (!citaciones.Any())
        {
            Console.WriteLine(" No tienes citaciones pendientes.");
            return;
        }

        Console.WriteLine("\n  CITACIONES PENDIENTES DE PAGO:");
        Console.WriteLine("==========================================");

        decimal totalMultas = 0;

        foreach (var citacion in citaciones)
        {
            Console.WriteLine($" Citación #{citacion.Id}");
            Console.WriteLine($"   Vehículo: {citacion.VehiculoTipo}");
            Console.WriteLine($"   Código: {citacion.CodigoReserva}");
            Console.WriteLine($"   Tiempo excedido: {citacion.MinutosExcedidos} minutos");
            Console.WriteLine($"   Multa: ${citacion.MontoMulta:F2}");
            Console.WriteLine($"   Fecha: {citacion.HoraInicioReserva:dd/MM/yyyy}");
            Console.WriteLine($"   Motivo: {citacion.Motivo}");
            Console.WriteLine("   ------------------------------------");

            totalMultas += citacion.MontoMulta;
        }

        Console.WriteLine($" TOTAL MULTAS PENDIENTES: ${totalMultas:F2}");
        Console.WriteLine("==========================================");

        Console.Write("\n¿Deseas pagar alguna multa? (s/n): ");
        if (Console.ReadLine()?.Trim().ToLower() == "s")
        {
            Console.Write("Ingresa el número de citación a pagar: ");
            if (int.TryParse(Console.ReadLine(), out int idCitacion))
            {
                var citacion = citaciones.FirstOrDefault(c => c.Id == idCitacion);
                if (citacion != null)
                {
                    Console.WriteLine($"\n PAGANDO MULTA #{citacion.Id}");
                    Console.WriteLine($"Monto a pagar: ${citacion.MontoMulta:F2}");
                    Console.Write("¿Confirmar pago? (s/n): ");

                    if (Console.ReadLine()?.Trim().ToLower() == "s")
                    {
                        decimal montoConIva = citacion.MontoMulta * 1.12m;
                        await GananciasEcoParking.RegistrarPagoStaticAsync(
                            "Pago de multa",
                            montoConIva,
                            "multa",
                            citacion.VehiculoTipo + " Parking",
                            citacion.VehiculoTipo,
                            usuario.Nombre
                        );

                        await citacion.MarcarComoPagadaAsync();
                        Console.WriteLine(" Multa pagada correctamente. Factura enviada por correo.");
                    }
                    else
                    {
                        Console.WriteLine(" Pago cancelado.");
                    }
                }
                else
                {
                    Console.WriteLine(" Citación no encontrada.");
                }
            }
            else
            {
                Console.WriteLine(" Número de citación inválido.");
            }
        }
    }

    static async Task ModificarCantidadAsync(Administrador admin)
    {
        using var context = new EcoParkingContext();
        var parqueos = await context.Parqueos.ToListAsync();
        Console.WriteLine("\nSeleccione el parqueo a modificar:");
        for (int i = 0; i < parqueos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {parqueos[i].Ubicacion} ({parqueos[i].TipoVehiculo}) - {parqueos[i].CantidadDisponible} espacios");
        }
        Console.Write("Opción: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > parqueos.Count)
        {
            Console.WriteLine("❌ Selección inválida."); return;
        }
        var seleccionado = parqueos[index - 1];
        Console.Write($"Ingrese nueva cantidad de espacios para {seleccionado.Ubicacion}: ");
        if (!int.TryParse(Console.ReadLine(), out int nuevaCantidad) || nuevaCantidad < 0)
        {
            Console.WriteLine(" Cantidad inválida."); return;
        }
        admin.ModificarCantidadDisponible(seleccionado, nuevaCantidad);
        context.Parqueos.Update(seleccionado);
        await context.SaveChangesAsync();
    }

    static async Task ActualizarTarifaAsync(Administrador admin)
    {
        using var context = new EcoParkingContext();
        var parqueos = await context.Parqueos.ToListAsync();
        Console.WriteLine("\nSeleccione el parqueo para actualizar tarifa:");
        for (int i = 0; i < parqueos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {parqueos[i].Ubicacion} ({parqueos[i].TipoVehiculo}) - Tarifa actual: ${parqueos[i].TarifaPorHora:F2}");
        }
        Console.Write("Opción: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > parqueos.Count)
        {
            Console.WriteLine(" Selección inválida."); return;
        }
        var seleccionado = parqueos[index - 1];
        Console.Write($"Ingrese nueva tarifa para {seleccionado.Ubicacion}: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal nuevaTarifa) || nuevaTarifa <= 0)
        {
            Console.WriteLine(" Tarifa inválida."); return;
        }
        admin.ActualizarTarifa(seleccionado, nuevaTarifa);
        context.Parqueos.Update(seleccionado);
        await context.SaveChangesAsync();
    }

    static async Task DejarReseñaAsync(Usuario usuario)
    {
        Console.Write("\nIngrese ID del parqueo: ");
        string idParqueo = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Puntuación (1-5): ");
        if (!int.TryParse(Console.ReadLine(), out int puntuacion) || puntuacion < 1 || puntuacion > 5)
        {
            Console.WriteLine(" Puntuación inválida."); return;
        }
        Console.Write("Comentario (opcional): ");
        string comentario = Console.ReadLine()?.Trim() ?? "";
        var reseña = new ReseñaParqueo(idParqueo, usuario.Nombre, puntuacion, comentario);
        await ReseñaParqueo.GuardarAsync(reseña);
        Console.WriteLine(" Reseña registrada correctamente.");
    }

    static async Task MostrarPromedioReseñasAsync()
    {
        Console.Write("\nIngrese ID del parqueo para ver promedio de puntuación: ");
        string idParqueo = Console.ReadLine()?.Trim() ?? "";
        decimal promedio = await ReseñaParqueo.ObtenerPuntuacionPromedioAsync(idParqueo);
        Console.WriteLine($"⭐ Puntuación promedio para {idParqueo}: {promedio}/5");
    }

    static async Task MostrarEstadisticasVehicularesAsync()
    {
        try
        {
            var estadisticas = await EstadisticaVehicular.CargarDesdeBaseDeDatosAsync();

            if (!estadisticas.Any())
            {
                Console.WriteLine(" No hay estadísticas vehiculares disponibles.");
                return;
            }

            Console.WriteLine("\n ESTADÍSTICAS VEHICULARES COMPLETAS");
            Console.WriteLine("==========================================");

            foreach (var estadistica in estadisticas)
            {
                estadistica.MostrarResumen();
            }

            int totalUsos = estadisticas.Sum(e => e.CantidadUsos);
            decimal totalRecaudado = estadisticas.Sum(e => e.TotalRecaudado);

            Console.WriteLine("\n TOTALES GENERALES:");
            Console.WriteLine($" Total de usos: {totalUsos}");
            Console.WriteLine($" Total recaudado: ${totalRecaudado:F2}");
            Console.WriteLine("==========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error mostrando estadísticas: {ex.Message}");
        }
    }
}