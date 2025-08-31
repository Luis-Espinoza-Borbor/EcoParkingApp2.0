using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoParkingApp2._0.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administradores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VehiculoTipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodigoReserva = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoraSalidaProg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraSalidaReal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoAPagar = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadisticasVehiculares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoVehiculo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CantidadUsos = table.Column<int>(type: "int", nullable: false),
                    TotalRecaudado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoUso = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadisticasVehiculares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fidelidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CorreoUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReservasCompletadas = table.Column<int>(type: "int", nullable: false),
                    UltimaFechaBeneficio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimaReserva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalDescuentoAplicado = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fidelidad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlujoPersonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePersona = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoAcceso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaEntrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraEntrada = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlujoPersonas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ganancias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Concepto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UbicacionParqueo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoVehiculo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ganancias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parqueos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ubicacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoVehiculo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    TarifaPorHora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    HoraReserva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CodigoReserva = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PagoRealizado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parqueos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReseñasParqueo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdParqueo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Puntuacion = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReseñasParqueo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administradores");

            migrationBuilder.DropTable(
                name: "Citaciones");

            migrationBuilder.DropTable(
                name: "EstadisticasVehiculares");

            migrationBuilder.DropTable(
                name: "Fidelidad");

            migrationBuilder.DropTable(
                name: "FlujoPersonas");

            migrationBuilder.DropTable(
                name: "Ganancias");

            migrationBuilder.DropTable(
                name: "Parqueos");

            migrationBuilder.DropTable(
                name: "ReseñasParqueo");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
