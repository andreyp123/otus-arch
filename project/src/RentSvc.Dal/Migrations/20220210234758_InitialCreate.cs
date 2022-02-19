using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RentSvc.Dal.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rent_svc");

            migrationBuilder.CreateTable(
                name: "rents",
                schema: "rent_svc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rent_id = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    car_id = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    state = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    start_mileage = table.Column<int>(type: "integer", nullable: true),
                    mileage = table.Column<int>(type: "integer", nullable: true),
                    price_per_km = table.Column<decimal>(type: "numeric", nullable: true),
                    price_per_hour = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requests",
                schema: "rent_svc",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requests", x => new { x.id, x.name, x.date });
                });

            migrationBuilder.CreateIndex(
                name: "ix_rents_rent_id",
                schema: "rent_svc",
                table: "rents",
                column: "rent_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rents",
                schema: "rent_svc");

            migrationBuilder.DropTable(
                name: "requests",
                schema: "rent_svc");
        }
    }
}
