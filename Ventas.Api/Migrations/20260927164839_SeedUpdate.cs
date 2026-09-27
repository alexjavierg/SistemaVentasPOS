using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ventas.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InventariosSucursal",
                columns: new[] { "Id", "CantidadDocenas", "ModeloId", "SucursalId" },
                values: new object[,]
                {
                    { 1, 10, 1, 1 },
                    { 2, 3, 2, 1 },
                    { 3, 0, 3, 1 },
                    { 4, 5, 4, 1 },
                    { 5, 15, 5, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Modelos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nombre",
                value: "Polo Manga Corta Cuello CorazÃ³n");

            migrationBuilder.UpdateData(
                table: "Modelos",
                keyColumn: "Id",
                keyValue: 10,
                column: "Nombre",
                value: "Body Manga Corta Cuello CorazÃ³n");

            migrationBuilder.UpdateData(
                table: "Sucursales",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nombre",
                value: "MUJER BONITA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InventariosSucursal",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InventariosSucursal",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InventariosSucursal",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InventariosSucursal",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "InventariosSucursal",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Modelos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nombre",
                value: "Polo Manga Corta Cuello Corazón");

            migrationBuilder.UpdateData(
                table: "Modelos",
                keyColumn: "Id",
                keyValue: 10,
                column: "Nombre",
                value: "Body Manga Corta Cuello Corazón");

            migrationBuilder.UpdateData(
                table: "Sucursales",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nombre",
                value: "Sede Principal");
        }
    }
}
