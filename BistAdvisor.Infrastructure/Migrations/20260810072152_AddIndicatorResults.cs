using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BistAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicatorResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndicatorResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    BarTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    RsiValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MacdValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MacdSignalValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MacdHistogramValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Ema20 = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Ema50 = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BollingerUpper = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BollingerMiddle = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BollingerLower = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    StochasticK = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    StochasticD = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AverageVolume20 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorResults_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorResults_StockId",
                table: "IndicatorResults",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndicatorResults");
        }
    }
}
