using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AirlineCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WindowMinutes = table.Column<int>(type: "integer", nullable: false),
                    Threshold = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ActualValue = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "metric_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AirlineCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WindowMinutes = table.Column<int>(type: "integer", nullable: false),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    TotalVolume = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    LatencyP95Ms = table.Column<int>(type: "integer", nullable: false),
                    LatencyP99Ms = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_snapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alerts_AirlineCode",
                table: "alerts",
                column: "AirlineCode");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_Code",
                table: "alerts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alerts_FiredAt",
                table: "alerts",
                column: "FiredAt");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_Status",
                table: "alerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_metric_snapshots_AirlineCode_WindowMinutes",
                table: "metric_snapshots",
                columns: new[] { "AirlineCode", "WindowMinutes" });

            migrationBuilder.CreateIndex(
                name: "IX_metric_snapshots_Code",
                table: "metric_snapshots",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "metric_snapshots");
        }
    }
}
