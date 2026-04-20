using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureDelivery.CourierService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Couriers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    CurrentLatitude = table.Column<double>(type: "float", nullable: true),
                    CurrentLongitude = table.Column<double>(type: "float", nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastLocationUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RatingSum = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    RatingCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalDeliveries = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Couriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourierRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatedByCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourierRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourierRatings_Couriers_CourierId",
                        column: x => x.CourierId,
                        principalTable: "Couriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "CourierRatings_CourierId",
                table: "CourierRatings",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "CourierRatings_Unique",
                table: "CourierRatings",
                columns: new[] { "CourierId", "OrderId", "RatedByCustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Couriers_Email",
                table: "Couriers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Couriers_Online_Available",
                table: "Couriers",
                columns: new[] { "IsOnline", "IsAvailable", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "Couriers_UserId",
                table: "Couriers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourierRatings");

            migrationBuilder.DropTable(
                name: "Couriers");
        }
    }
}
