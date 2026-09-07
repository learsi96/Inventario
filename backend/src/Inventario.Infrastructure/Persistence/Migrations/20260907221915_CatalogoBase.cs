using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CategoriaPadreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EsSistema = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Categorias_CategoriaPadreId",
                        column: x => x.CategoriaPadreId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_CategoriaPadreId",
                table: "Categorias",
                column: "CategoriaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_TenantId_CategoriaPadreId",
                table: "Categorias",
                columns: new[] { "TenantId", "CategoriaPadreId" });

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_Codigo",
                table: "UnidadesMedida",
                column: "Codigo",
                unique: true);

            // Catálogo global de unidades de medida (GUID fijos para reproducibilidad).
            var creado = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            migrationBuilder.InsertData(
                table: "UnidadesMedida",
                columns: new[] { "Id", "Codigo", "Nombre", "Activa", "CreadoEn", "ActualizadoEn" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "PZA", "Pieza", true, creado, null },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "JGO", "Juego", true, creado, null },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "LT", "Litro", true, creado, null },
                    { new Guid("11111111-0000-0000-0000-000000000004"), "MT", "Metro", true, creado, null },
                    { new Guid("11111111-0000-0000-0000-000000000005"), "KG", "Kilogramo", true, creado, null },
                    { new Guid("11111111-0000-0000-0000-000000000006"), "CJA", "Caja", true, creado, null },
                });

            // Backfill: cada partner existente necesita su categoría de sistema "Otros".
            migrationBuilder.Sql(
                """
                INSERT INTO Categorias (Id, TenantId, Nombre, CategoriaPadreId, EsSistema, Activa, CreadoEn)
                SELECT NEWID(), t.Id, N'Otros', NULL, 1, 1, SYSDATETIMEOFFSET()
                FROM Tenants t
                WHERE NOT EXISTS (
                    SELECT 1 FROM Categorias c
                    WHERE c.TenantId = t.Id AND c.EsSistema = 1 AND c.CategoriaPadreId IS NULL
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");
        }
    }
}
