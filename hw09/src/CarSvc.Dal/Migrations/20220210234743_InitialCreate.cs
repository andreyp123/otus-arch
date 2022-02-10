using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarSvc.Dal.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "car_svc");

            migrationBuilder.CreateTable(
                name: "cars",
                schema: "car_svc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    car_id = table.Column<string>(type: "text", nullable: false),
                    brand = table.Column<string>(type: "text", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    release_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    body_style = table.Column<string>(type: "text", nullable: false),
                    doors_count = table.Column<int>(type: "integer", nullable: false),
                    transmission = table.Column<string>(type: "text", nullable: false),
                    fuel_type = table.Column<string>(type: "text", nullable: false),
                    price_per_km = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_hour = table.Column<decimal>(type: "numeric", nullable: false),
                    drive_state = table.Column<string>(type: "text", nullable: false),
                    mileage = table.Column<int>(type: "integer", nullable: false),
                    location_lat = table.Column<decimal>(type: "numeric", nullable: false),
                    location_lon = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_fuel = table.Column<decimal>(type: "numeric", nullable: false),
                    alert = table.Column<string>(type: "text", nullable: true),
                    api_key_hash = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cars", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "car_rents",
                schema: "car_svc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    car_id = table.Column<int>(type: "integer", nullable: false),
                    rent_id = table.Column<string>(type: "text", nullable: false),
                    rent_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rent_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_car_rents", x => x.id);
                    table.ForeignKey(
                        name: "fk_car_rents_cars_car_id",
                        column: x => x.car_id,
                        principalSchema: "car_svc",
                        principalTable: "cars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_car_rents_car_id",
                schema: "car_svc",
                table: "car_rents",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "ix_cars_car_id",
                schema: "car_svc",
                table: "cars",
                column: "car_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "car_rents",
                schema: "car_svc");

            migrationBuilder.DropTable(
                name: "cars",
                schema: "car_svc");
        }
    }
}
