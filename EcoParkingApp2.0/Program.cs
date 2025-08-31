using EcoParking_Proyecto;

using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace EcoParkingApp;

class Program
{
    static List<Administrador> administradores = new List<Administrador>();
    static List<Usuario> usuariosRegistrados = new List<Usuario>();

    static async Task Main(string[] args)
    {
        Console.WriteLine("🌐 Inicializando EcoParking System...");

        try
        {
            // Inicializar base de datos con todas las tablas
            using var context = new EcoParkingContext();
            await context.Database.EnsureCreatedAsync();
            await context.InitializeDataAsync();

            Console.WriteLine("✅ Base de datos inicializada correctamente");

            // Cargar datos iniciales
            await CargarDatosInicialesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error inicializando sistema: {ex.Message}");
            return;
        }

        // Inicialización de espacios de parqueo
        List<EcoParking> parqueos = new()
        {
            new EcoParking("Guayaquil-Centro", "Auto", true, 1.50m, "GYE123"),
            new EcoParking("Guayaquil-Norte", "Moto", true, 1.00m, "GYN456"),
            new EcoParking("Samborondón", "Camioneta", true, 2.00m, "SAM789")
        };

        bool ejecutarPrograma = true;
        while (ejecutarPrograma)
        {
            string[] opcionesInicio = {
                "1. Iniciar como Usuario",
                "2. Iniciar como Administrador",
                "3. Ver reseñas de parqueos",
                "4. Ver estadísticas del sistema",
                "5. Salir del sistema"
            };

            MostrarMenuConMarco("MENÚ PRINCIPAL", opcionesInicio);
            string opcionInicio = Console.ReadLine() ?? "";

            switch (opcionInicio)
            {
                case "1":
                    await FlujoPersonas.RegistrarEntradaStaticAsync("Usuario Anónimo", "Usuario");
                    await EjecutarUsuarioAsync(parqueos);
                    break;

                case "2":
                    var admin = await ObtenerAdministradorAsync();
                    if (admin != null)
                    {
                        await FlujoPersonas.RegistrarEntradaStaticAsync(admin.Nombre, "Administrador");
                        await EjecutarAdministradorAsync(admin, parqueos);
                    }
                    break;

                case "3":
                    await ReseñaParqueo.MostrarAsync();
                    Console.WriteLine("\nPresiona ENTER para continuar...");
                    Console.ReadLine();
                    break;

                case "4":
                    await MostrarEstadisticasCompletasAsync();
                    break;

                case "5":
                    ejecutarPrograma = false;
                    Console.WriteLine("🙏 Gracias por usar EcoParking. ¡Hasta pronto!");
                    break;

                default:
                    Console.WriteLine("❌ Opción no válida.");
                    break;
            }
        }
    }

    static async Task CargarDatosInicialesAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            usuariosRegistrados = await context.Usuarios.ToListAsync();
            administradores = await context.Administradores.ToListAsync();

            Console.WriteLine($"📊 Datos cargados: {usuariosRegistrados.Count} usuarios, {administradores.Count} administradores");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error cargando datos: {ex.Message}");
        }
    }

    static async Task MostrarEstadisticasCompletasAsync()
    {
        try
        {
            Console.WriteLine("\n📈 ESTADÍSTICAS COMPLETAS DEL SISTEMA");
            Console.WriteLine("===================================");

            // Estadísticas de ganancias
            await GananciasEcoParking.MostrarResumenAsync();

            // Estadísticas de flujo de personas
            await FlujoPersonas.MostrarHistorialAsync();

            Console.WriteLine("\nPresiona ENTER para continuar...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error mostrando estadísticas: {ex.Message}");
        }
    }

    static async Task EjecutarUsuarioAsync(List<EcoParking> parqueos)
    {
        EcoParking? espacioSeleccionado = null;
        Usuario? usuario = null;
        DateTime horaSalidaProgramada = DateTime.MinValue;

        // Acceso de usuario
        string[] opcionesLogin = { "1. Registrarse", "2. Iniciar sesión" };
        MostrarMenuConMarco("ACCESO DE USUARIO", opcionesLogin);
        string loginOption = Console.ReadLine() ?? "";

        if (loginOption == "1")
        {
            usuario = await Usuario.RegistrarNuevoUsuarioAsync();
            if (usuario != null)
            {
                usuariosRegistrados.Add(usuario);
                Console.WriteLine($"✅ Usuario {usuario.Nombre} registrado exitosamente");
            }
        }
        else if (loginOption == "2")
        {
            usuario = await Usuario.IniciarSesionAsync();
        }
        else
        {
            Console.WriteLine("❌ Opción inválida.");
            return;
        }

        if (usuario == null)
        {
            Console.WriteLine("❌ Inicio de sesión fallido.");
            return;
        }

        usuario.MostrarDatos();

        // Selección de espacio
        while (espacioSeleccionado == null)
        {
            Console.WriteLine("\n=== OPCIONES DE PARQUEO DISPONIBLES ===");
            for (int i = 0; i < parqueos.Count; i++)
            {
                var p = parqueos[i];
                Console.WriteLine($"{i + 1}. 🅿️ Ubicación: {p.Ubicacion} | Tipo: {p.TipoVehiculo} | Disponible: {(p.Disponible ? "✅ Sí" : "❌ No")} | Tarifa: ${p.TarifaPorHora:F2}/hora");
            }

            Console.Write("Seleccione el número del parqueo: ");
            if (int.TryParse(Console.ReadLine(), out int seleccion) && seleccion >= 1 && seleccion <= parqueos.Count)
            {
                espacioSeleccionado = parqueos[seleccion - 1];
                Console.WriteLine($"✅ Has seleccionado el espacio en {espacioSeleccionado.Ubicacion}");
            }
            else
            {
                Console.WriteLine("❌ Selección inválida.");
            }
        }

        // Menú de usuario
        bool continuar = true;
        while (continuar)
        {
            string[] opcionesUsuario = {
                "1. Reservar espacio",
                "2. Consultar código de reserva",
                "3. Modificar código de reserva",
                "4. Realizar pago",
                "5. Ver estado de pago",
                "6. Ver mis estadísticas de fidelidad",
                "7. Volver al menú principal"
            };

            MostrarMenuConMarco("MENÚ DE USUARIO", opcionesUsuario);
            string opcion = Console.ReadLine() ?? "";

            switch (opcion)
            {
                case "1":
                    Console.Write("¿Cuántas horas vas a reservar?: ");
                    if (int.TryParse(Console.ReadLine(), out int horas) && horas > 0)
                    {
                        espacioSeleccionado.ReservarEspacio();
                        await espacioSeleccionado.GuardarEstadoEnBaseDeDatosAsync();

                        // Registrar en estadísticas
                        var estadistica = await EstadisticaVehicular.ObtenerPorTipoVehiculoAsync(espacioSeleccionado.TipoVehiculo);
                        estadistica.RegistrarUso();
                        await estadistica.GuardarEnBaseDeDatosAsync();

                        DateTime horaIngreso = DateTime.Now;
                        horaSalidaProgramada = horaIngreso.AddHours(horas);
                        Console.WriteLine($"✅ Reserva exitosa. Salida programada a las {horaSalidaProgramada:HH:mm}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Horas inválidas.");
                    }
                    break;

                case "2":
                    Console.WriteLine($"🔑 Código de reserva: {espacioSeleccionado.GetCodigoReserva()}");
                    break;

                case "3":
                    Console.Write("Nuevo código: ");
                    string? nuevoCodigo = Console.ReadLine();
                    if (!string.IsNullOrEmpty(nuevoCodigo))
                    {
                        espacioSeleccionado.SetCodigoReserva(nuevoCodigo);
                        await espacioSeleccionado.GuardarEstadoEnBaseDeDatosAsync();
                        Console.WriteLine("✅ Código actualizado correctamente");
                    }
                    else
                    {
                        Console.WriteLine("❌ Código inválido.");
                    }
                    break;

                case "4":
                    try
                    {
                        // Obtener método de pago y monto
                        Console.WriteLine("💳 Seleccione método de pago:");
                        Console.WriteLine("1. Tarjeta de crédito/débito");
                        Console.WriteLine("2. Efectivo");
                        Console.Write("Opción: ");

                        string metodoPago = Console.ReadLine() == "1" ? "Tarjeta" : "Efectivo";

                        Console.Write("¿Cuántas horas estuviste estacionado?: ");
                        if (int.TryParse(Console.ReadLine(), out int horasEstacionado) && horasEstacionado > 0)
                        {
                            decimal montoPagado = espacioSeleccionado.TarifaPorHora * horasEstacionado;

                            // ✅ Aplicar descuento por fidelidad
                            var fidelidad = await Fidelidad.ObtenerPorUsuarioAsync(usuario.Nombre, usuario.Correo);
                            decimal montoFinal = await fidelidad.VerificarYAplicarDescuentoAsync(montoPagado);

                            Console.WriteLine($"💰 Monto a pagar: ${montoFinal:F2}");
                            if (montoFinal < montoPagado)
                            {
                                Console.WriteLine($"🎁 Descuento aplicado: ${montoPagado - montoFinal:F2}");
                            }

                            Console.Write("Confirmar pago (s/n): ");

                            if (Console.ReadLine()?.ToLower() == "s")
                            {
                                // ✅ CORRECCIÓN: Marcar el pago como realizado
                                espacioSeleccionado.PagoRealizado = true;

                                await espacioSeleccionado.GuardarEstadoEnBaseDeDatosAsync();

                                // ✅ Registrar en ganancias
                                await GananciasEcoParking.RegistrarPagoStaticAsync(
                                    $"Pago parqueo - {espacioSeleccionado.Ubicacion}",
                                    montoFinal,
                                    metodoPago,
                                    espacioSeleccionado.Ubicacion,
                                    espacioSeleccionado.TipoVehiculo,
                                    usuario.Nombre
                                );

                                // ✅ Registrar en fidelidad
                                await fidelidad.RegistrarReservaAsync();

                                // ✅ ENVÍO DE CORREO ELECTRÓNICO
                                if (!string.IsNullOrEmpty(usuario.Correo))
                                {
                                    Console.WriteLine("📧 Enviando comprobante por correo...");
                                    var correo = new Correo(
                                        usuario.Correo,
                                        usuario.Nombre,
                                        metodoPago,
                                        (double)montoFinal
                                    );
                                    correo.EnviarComprobante();
                                }
                                else
                                {
                                    Console.WriteLine("⚠ No se puede enviar correo: usuario no tiene correo registrado");
                                }

                                Console.WriteLine("✅ Pago realizado exitosamente");

                                // Generar citación si aplica
                                if (DateTime.Now > horaSalidaProgramada)
                                {
                                    await CitacionParqueo.GenerarAsync(usuario, espacioSeleccionado, horaSalidaProgramada);
                                }

                                // Opción de dejar reseña
                                Console.Write("\n⭐ ¿Deseas dejar una reseña para el parqueo? (s/n): ");
                                if (Console.ReadLine()?.ToLower() == "s")
                                {
                                    Console.Write("Puntuación (1-5): ");
                                    if (int.TryParse(Console.ReadLine(), out int puntuacion) && puntuacion >= 1 && puntuacion <= 5)
                                    {
                                        Console.Write("Comentario: ");
                                        string? comentario = Console.ReadLine();

                                        var resena = new ReseñaParqueo
                                        {
                                            IdParqueo = espacioSeleccionado.Ubicacion,
                                            Usuario = usuario.Nombre,
                                            Puntuacion = puntuacion,
                                            Comentario = comentario ?? "",
                                            Fecha = DateTime.Now
                                        };

                                        await ReseñaParqueo.GuardarAsync(resena);
                                        Console.WriteLine("¡Gracias por tu reseña!");
                                    }
                                    else
                                    {
                                        Console.WriteLine("❌ Puntuación inválida.");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("❌ Pago cancelado.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ Horas inválidas.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error en el proceso de pago: {ex.Message}");
                    }
                    break;

                case "5":
                    Console.WriteLine($"📋 Estado de pago: {espacioSeleccionado.EstadoPago()}");
                    break;

                case "6":
                    var fidelidadUsuario = await Fidelidad.ObtenerPorUsuarioAsync(usuario.Nombre, usuario.Correo);
                    fidelidadUsuario.MostrarEstadisticas();
                    break;

                case "7":
                    continuar = false;
                    Console.WriteLine("↩ Volviendo al menú principal...");
                    break;

                default:
                    Console.WriteLine("❌ Opción inválida.");
                    break;
            }
        }
    }

    static async Task EjecutarAdministradorAsync(Administrador admin, List<EcoParking> parqueos)
    {
        Console.WriteLine($"\n👨‍💼 Bienvenido administrador: {admin.Nombre}");
        bool continuar = true;

        while (continuar)
        {
            string[] opcionesAdmin = {
                "1. Cambiar disponibilidad de parqueo",
                "2. Actualizar tarifa de parqueo",
                "3. Mostrar datos del administrador",
                "4. Ver estadísticas de uso vehicular",
                "5. Ver reporte de ganancias",
                "6. Ver historial de entradas",
                "7. Mostrar lista de usuarios registrados",
                "8. Ver citaciones por exceso de tiempo",
                "9. Ver programa de fidelidad",
                "10. Volver al menú principal"
            };

            MostrarMenuConMarco("MENÚ DE ADMINISTRADOR", opcionesAdmin);
            string opcion = Console.ReadLine() ?? "";

            switch (opcion)
            {
                case "1":
                    MostrarParqueos(parqueos);
                    Console.Write("Seleccione el parqueo a modificar: ");
                    if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= parqueos.Count)
                    {
                        Console.Write("Nuevo estado (true = disponible, false = reservado): ");
                        if (bool.TryParse(Console.ReadLine(), out bool nuevoEstado))
                        {
                            admin.CambiarDisponibilidad(parqueos[idx - 1], nuevoEstado);
                            await parqueos[idx - 1].GuardarEstadoEnBaseDeDatosAsync();
                            Console.WriteLine("✅ Disponibilidad actualizada");
                        }
                        else
                        {
                            Console.WriteLine("❌ Estado inválido.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ Selección inválida.");
                    }
                    break;

                case "2":
                    MostrarParqueos(parqueos);
                    Console.Write("Seleccione el parqueo a modificar: ");
                    if (int.TryParse(Console.ReadLine(), out int tIdx) && tIdx >= 1 && tIdx <= parqueos.Count)
                    {
                        Console.Write("Nueva tarifa por hora: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal nuevaTarifa) && nuevaTarifa > 0)
                        {
                            admin.ActualizarTarifa(parqueos[tIdx - 1], nuevaTarifa);
                            await parqueos[tIdx - 1].GuardarEstadoEnBaseDeDatosAsync();
                            Console.WriteLine("✅ Tarifa actualizada");
                        }
                        else
                        {
                            Console.WriteLine("❌ Tarifa inválida.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ Selección inválida.");
                    }
                    break;

                case "3":
                    admin.MostrarDatos();
                    break;

                case "4":
                    await MostrarEstadisticasVehicularesAsync();
                    break;

                case "5":
                    await GananciasEcoParking.MostrarResumenAsync();
                    break;

                case "6":
                    await FlujoPersonas.MostrarHistorialAsync();
                    break;

                case "7":
                    await MostrarUsuariosRegistradosAsync();
                    break;

                case "8":
                    await MostrarCitacionesAsync();
                    break;

                case "9":
                    await MostrarProgramaFidelidadAsync();
                    break;

                case "10":
                    continuar = false;
                    Console.WriteLine("↩ Volviendo al menú principal...");
                    break;

                default:
                    Console.WriteLine("❌ Opción no válida.");
                    break;
            }

            Console.WriteLine("\nPresiona ENTER para continuar...");
            Console.ReadLine();
        }
    }

    // =================== MÉTODOS AUXILIARES ===================

    static async Task MostrarUsuariosRegistradosAsync()
    {
        Console.WriteLine("\n--- USUARIOS REGISTRADOS ---");

        try
        {
            using var context = new EcoParkingContext();
            var usuarios = await context.Usuarios.ToListAsync();

            if (usuarios.Count == 0)
            {
                Console.WriteLine("No hay usuarios registrados.");
                return;
            }

            foreach (var u in usuarios)
            {
                Console.WriteLine($"👤 Nombre: {u.Nombre} | Cédula: {u.Cedula} | Correo: {u.Correo}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al cargar usuarios: {ex.Message}");
        }
    }

    static async Task<Administrador?> ObtenerAdministradorAsync()
    {
        string[] opcionesAdmin = { "1. Registrar nuevo administrador", "2. Iniciar sesión" };
        MostrarMenuConMarco("ACCESO ADMINISTRADOR", opcionesAdmin);
        string opcion = Console.ReadLine() ?? "";

        if (opcion == "1")
        {
            return await Administrador.RegistrarAdministradorEFAsync();
        }
        else if (opcion == "2")
        {
            return await Administrador.IniciarSesionEFAsync();
        }
        else
        {
            Console.WriteLine("❌ Opción inválida.");
            return null;
        }
    }

    static void MostrarParqueos(List<EcoParking> parqueos)
    {
        Console.WriteLine("\n--- LISTA DE PARQUEOS ---");
        for (int i = 0; i < parqueos.Count; i++)
        {
            var p = parqueos[i];
            Console.WriteLine($"{i + 1}. 🅿️ Ubicación: {p.Ubicacion} | Tipo: {p.TipoVehiculo} | Disponible: {(p.Disponible ? "✅ Sí" : "❌ No")} | Tarifa: ${p.TarifaPorHora:F2}/hora");
        }
    }

    static async Task MostrarCitacionesAsync()
    {
        Console.WriteLine("\n--- LISTA DE CITACIONES ---");

        try
        {
            using var context = new EcoParkingContext();
            var citaciones = await context.Citaciones.ToListAsync();

            if (citaciones.Count == 0)
            {
                Console.WriteLine("✅ No hay citaciones registradas.");
            }
            else
            {
                foreach (var c in citaciones)
                {
                    Console.WriteLine($"🚨 Usuario: {c.Usuario} | Vehículo: {c.VehiculoTipo} | Monto: ${c.MontoAPagar:F2} | Fecha: {c.HoraSalidaReal:yyyy-MM-dd HH:mm}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al cargar citaciones: {ex.Message}");
        }
    }

    static async Task MostrarEstadisticasVehicularesAsync()
    {
        try
        {
            var estadisticas = await EstadisticaVehicular.CargarDesdeBaseDeDatosAsync();

            Console.WriteLine("\n🚗 ESTADÍSTICAS VEHICULARES");
            Console.WriteLine("========================");

            foreach (var stat in estadisticas)
            {
                stat.MostrarResumen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error mostrando estadísticas: {ex.Message}");
        }
    }

    static async Task MostrarProgramaFidelidadAsync()
    {
        try
        {
            using var context = new EcoParkingContext();
            var fidelidades = await context.Fidelidad.ToListAsync();

            Console.WriteLine("\n⭐ PROGRAMA DE FIDELIDAD");
            Console.WriteLine("======================");

            if (!fidelidades.Any())
            {
                Console.WriteLine("No hay datos de fidelidad registrados.");
                return;
            }

            foreach (var f in fidelidades.OrderByDescending(x => x.ReservasCompletadas))
            {
                Console.WriteLine($"👤 {f.NombreUsuario}: {f.ReservasCompletadas} reservas | Descuento: ${f.TotalDescuentoAplicado:F2}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error mostrando fidelidad: {ex.Message}");
        }
    }

    static void MostrarMenuConMarco(string titulo, string[] opciones)
    {
        int anchoContenido = Math.Max(
            opciones.Max(o => o.Length),
            titulo.Length
        ) + 4;

        string bordeSuperior = "┌" + new string('─', anchoContenido + 2) + "┐";
        string bordeInferior = "└" + new string('─', anchoContenido + 2) + "┘";
        string bordeMedio = "├" + new string('─', anchoContenido + 2) + "┤";

        Console.WriteLine();
        Console.WriteLine(bordeSuperior);
        Console.WriteLine($"│ {titulo.PadRight(anchoContenido)} │");
        Console.WriteLine(bordeMedio);

        foreach (var opcion in opciones)
        {
            Console.WriteLine($"│ {opcion.PadRight(anchoContenido)} │");
        }

        Console.WriteLine(bordeInferior);
        Console.Write("→ Seleccione una opción: ");
    }
}