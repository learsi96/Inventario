using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Conteos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conteos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Folio = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConciliadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MovimientoAjusteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conteos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conteos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conteos_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConteoDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConteoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticuloId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadSistema = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadContada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CreadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteoDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConteoDetalles_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConteoDetalles_Conteos_ConteoId",
                        column: x => x.ConteoId,
                        principalTable: "Conteos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConteoDetalles_ArticuloId",
                table: "ConteoDetalles",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteoDetalles_ConteoId_ArticuloId",
                table: "ConteoDetalles",
                columns: new[] { "ConteoId", "ArticuloId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conteos_CategoriaId",
                table: "Conteos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Conteos_SucursalId",
                table: "Conteos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Conteos_TenantId_Folio",
                table: "Conteos",
                columns: new[] { "TenantId", "Folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conteos_TenantId_SucursalId_Estado",
                table: "Conteos",
                columns: new[] { "TenantId", "SucursalId", "Estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConteoDetalles");

            migrationBuilder.DropTable(
                name: "Conteos");
        }
    }
}
