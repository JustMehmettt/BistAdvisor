using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BistAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicatorResultsUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndicatorResults_StockId",
                table: "IndicatorResults");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorResults_StockId_Interval_BarTime",
                table: "IndicatorResults",
                columns: new[] { "StockId", "Interval", "BarTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndicatorResults_StockId_Interval_BarTime",
                table: "IndicatorResults");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorResults_StockId",
                table: "IndicatorResults",
                column: "StockId");
        }
    }
}
