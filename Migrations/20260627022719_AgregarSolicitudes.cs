using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEntidad = table.Column<int>(type: "int", nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    DatosActuales = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CambioPropuesto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    SolicitanteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevisadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComentarioRevision = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_AspNetUsers_RevisadoPorId",
                        column: x => x.RevisadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Solicitudes_AspNetUsers_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_RevisadoPorId",
                table: "Solicitudes",
                column: "RevisadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_SolicitanteId",
                table: "Solicitudes",
                column: "SolicitanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Solicitudes");
        }
    }
}
