using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriasFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CampoModificado = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ValorNuevo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriasFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriasFactura_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditoriasFactura_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasFactura_FacturaId",
                table: "AuditoriasFactura",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasFactura_UsuarioId",
                table: "AuditoriasFactura",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriasFactura");
        }
    }
}
