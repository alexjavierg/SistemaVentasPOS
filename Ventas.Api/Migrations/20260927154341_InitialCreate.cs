using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ventas.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rol = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Modelos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreCorto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Foto = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrecioPorDocena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoPorDocena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Talla = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoQR = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    EtiquetasPendientes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modelos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modelos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CajasTurnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoFinal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Estado = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajasTurnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CajasTurnos_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CajasTurnos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventariosSucursal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ModeloId = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    CantidadDocenas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventariosSucursal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventariosSucursal_Modelos_ModeloId",
                        column: x => x.ModeloId,
                        principalTable: "Modelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventariosSucursal_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    ModeloId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CantidadDocenas = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Modelos_ModeloId",
                        column: x => x.ModeloId,
                        principalTable: "Modelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    CajaTurnoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Serie = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correlativo = table.Column<int>(type: "int", nullable: false),
                    TipoComprobante = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClienteDocumento = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClienteNombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Recargo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedioPago = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotivoAnulacion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_CajasTurnos_CajaTurnoId",
                        column: x => x.CajaTurnoId,
                        principalTable: "CajasTurnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetallesVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    ModeloId = table.Column<int>(type: "int", nullable: false),
                    CantidadDocenas = table.Column<int>(type: "int", nullable: false),
                    PrecioPorDocena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoHistoricoPorDocena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesVentas_Modelos_ModeloId",
                        column: x => x.ModeloId,
                        principalTable: "Modelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVentas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "BODYS MANGA CERO" },
                    { 2, "BODYS MANGA CORTA" },
                    { 3, "BODYS MANGA LARGA" },
                    { 4, "POLOS" }
                });

            migrationBuilder.InsertData(
                table: "Sucursales",
                columns: new[] { "Id", "Direccion", "Nombre", "Telefono" },
                values: new object[] { 1, "Lima", "Sede Principal", null });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "BloqueadoHasta", "IntentosFallidos", "PasswordHash", "Rol", "Username" },
                values: new object[] { 1, null, 0, "$2a$11$PxJVDpSwx9mjYGSxORIPeOl2VP5kIf/CLkkNOeSBLFF0FtxRP28om", "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "Modelos",
                columns: new[] { "Id", "CategoriaId", "CodigoQR", "CostoPorDocena", "EtiquetasPendientes", "Foto", "Nombre", "NombreCorto", "PrecioPorDocena", "Talla" },
                values: new object[,]
                {
                    { 1, 4, "", 0m, 0, null, "Polo Manga Corta Cuello Cuadrado", null, 10.00m, "Estándar" },
                    { 2, 2, "", 0m, 0, null, "Body Manga Corta Cuello Cuadrado", null, 14.00m, "Estándar" },
                    { 3, 2, "", 0m, 0, null, "Body Manga Corta Espalda Descubierta", null, 12.50m, "Estándar" },
                    { 4, 3, "", 0m, 0, null, "Body Manga Larga Cuello Cuadrado", null, 14.00m, "Estándar" },
                    { 5, 1, "", 0m, 0, null, "Body Manga Cero Escote Espejo", null, 10.50m, "Estándar" },
                    { 6, 4, "", 0m, 0, null, "Polo Manga Corta Cuello Corazón", null, 10.00m, "Estándar" },
                    { 7, 1, "", 0m, 0, null, "Body Manga Cero Tirita", null, 10.00m, "Estándar" },
                    { 8, 4, "", 0m, 0, null, "Polo Lazo", null, 11.50m, "Estándar" },
                    { 9, 4, "", 0m, 0, null, "Polo Manga Corta Cuello Redondo", null, 9.50m, "Estándar" },
                    { 10, 2, "", 0m, 0, null, "Body Manga Corta Cuello Corazón", null, 12.50m, "Estándar" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CajasTurnos_SucursalId",
                table: "CajasTurnos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_CajasTurnos_UsuarioId",
                table: "CajasTurnos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentas_ModeloId",
                table: "DetallesVentas",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentas_VentaId",
                table: "DetallesVentas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosSucursal_ModeloId",
                table: "InventariosSucursal",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosSucursal_SucursalId",
                table: "InventariosSucursal",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelos_CategoriaId",
                table: "Modelos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Modelos_Nombre",
                table: "Modelos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ModeloId",
                table: "MovimientosInventario",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_SucursalId",
                table: "MovimientosInventario",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_UsuarioId",
                table: "MovimientosInventario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CajaTurnoId",
                table: "Ventas",
                column: "CajaTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_SucursalId",
                table: "Ventas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioId",
                table: "Ventas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesVentas");

            migrationBuilder.DropTable(
                name: "InventariosSucursal");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Modelos");

            migrationBuilder.DropTable(
                name: "CajasTurnos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
