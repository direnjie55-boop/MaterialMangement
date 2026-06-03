using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaterialMangement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Specification = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Materials",
                columns: new[] { "Id", "Category", "CreatedAt", "Name", "Quantity", "Remark", "Specification", "Unit", "UnitPrice", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "建材", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "水泥", 100m, "普通硅酸盐水泥", "P.O 42.5", "吨", 450.00m, null },
                    { 2, "建材", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "钢筋", 50m, "热轧带肋钢筋", "HRB400 Φ12", "吨", 4200.00m, null },
                    { 3, "建材", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "砂石", 200m, "河砂", "中砂", "立方米", 85.00m, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
