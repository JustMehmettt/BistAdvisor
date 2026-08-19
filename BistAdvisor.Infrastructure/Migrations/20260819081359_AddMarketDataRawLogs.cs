using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BistAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketDataRawLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "IndicatorResults",
                newName: "CalculatedAt");

            migrationBuilder.CreateTable(
                name: "MarketDataRawLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestSymbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketDataRawLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketDataRawLogs_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketDataRawLogs_StockId",
                table: "MarketDataRawLogs",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketDataRawLogs");

            migrationBuilder.RenameColumn(
                name: "CalculatedAt",
                table: "IndicatorResults",
                newName: "CreatedAt");
        }
    }
}
