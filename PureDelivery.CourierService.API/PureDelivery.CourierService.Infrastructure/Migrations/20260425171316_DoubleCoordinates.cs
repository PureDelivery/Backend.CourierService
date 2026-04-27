using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureDelivery.CourierService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DoubleCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RestaurantLongitude",
                table: "AvailableOrders",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<double>(
                name: "RestaurantLatitude",
                table: "AvailableOrders",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<double>(
                name: "DeliveryLongitude",
                table: "AvailableOrders",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<double>(
                name: "DeliveryLatitude",
                table: "AvailableOrders",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RestaurantLongitude",
                table: "AvailableOrders",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "RestaurantLatitude",
                table: "AvailableOrders",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "DeliveryLongitude",
                table: "AvailableOrders",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "DeliveryLatitude",
                table: "AvailableOrders",
                type: "float(18)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
