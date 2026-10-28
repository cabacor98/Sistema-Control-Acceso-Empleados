using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SGCAE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AreaOrganizacional = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaContratacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuntosAcceso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Habilitado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosAcceso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Credenciales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credenciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credenciales_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAcceso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CredencialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CodigoUtilizado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PuntoAccesoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoMovimiento = table.Column<int>(type: "int", nullable: false),
                    Resultado = table.Column<int>(type: "int", nullable: false),
                    MotivoRechazo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAcceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosAcceso_Credenciales_CredencialId",
                        column: x => x.CredencialId,
                        principalTable: "Credenciales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrosAcceso_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrosAcceso_PuntosAcceso_PuntoAccesoId",
                        column: x => x.PuntoAccesoId,
                        principalTable: "PuntosAcceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PuntosAcceso",
                columns: new[] { "Id", "Descripcion", "Habilitado", "Nombre" },
                values: new object[,]
                {
                    { new Guid("0aa9c3d4-7006-4d61-9e30-1d3a5b171a01"), "Ingreso principal de la sede (área general)", true, "Recepción" },
                    { new Guid("0aa9c3d4-7006-4d61-9e30-1d3a5b171a02"), "Portería de acceso vehicular", true, "Parqueadero" },
                    { new Guid("0aa9c3d4-7006-4d61-9e30-1d3a5b171a03"), "Zona de almacenamiento y despacho", true, "Bodega" },
                    { new Guid("0aa9c3d4-7006-4d61-9e30-1d3a5b171a04"), "Sala de servidores - acceso autorizado", true, "Área Restringida A" },
                    { new Guid("0aa9c3d4-7006-4d61-9e30-1d3a5b171a05"), "Zona en acondicionamiento", false, "Almacén Temporal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credenciales_EmpleadoId",
                table: "Credenciales",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "UX_Credenciales_Codigo",
                table: "Credenciales",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Empleados_Email",
                table: "Empleados",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuntosAcceso_Nombre",
                table: "PuntosAcceso",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAcceso_CredencialId",
                table: "RegistrosAcceso",
                column: "CredencialId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAcceso_EmpleadoId_FechaHora",
                table: "RegistrosAcceso",
                columns: new[] { "EmpleadoId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAcceso_FechaHora",
                table: "RegistrosAcceso",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAcceso_PuntoAccesoId_FechaHora",
                table: "RegistrosAcceso",
                columns: new[] { "PuntoAccesoId", "FechaHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAcceso");

            migrationBuilder.DropTable(
                name: "Credenciales");

            migrationBuilder.DropTable(
                name: "PuntosAcceso");

            migrationBuilder.DropTable(
                name: "Empleados");
        }
    }
}
