using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Orders.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Stock",
                columns: new[] { "Sku", "Disponible" },
                values: new object[,]
                {
                    { "SKU001", 10 },
                    { "SKU002", 5 },
                    { "SKU003", 20 },
                    { "SKU004", 40 },
                    { "SKU005", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "Sku",
                keyValue: "SKU001");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "Sku",
                keyValue: "SKU002");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "Sku",
                keyValue: "SKU003");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "Sku",
                keyValue: "SKU004");

            migrationBuilder.DeleteData(
                table: "Stock",
                keyColumn: "Sku",
                keyValue: "SKU005");
        }
    }
}
