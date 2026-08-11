using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BistAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignalSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignalSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    BarTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    RsiScore = table.Column<int>(type: "int", nullable: true),
                    MacdScore = table.Column<int>(type: "int", nullable: true),
                    EmaScore = table.Column<int>(type: "int", nullable: true),
                    BollingerScore = table.Column<int>(type: "int", nullable: true),
                    StochasticScore = table.Column<int>(type: "int", nullable: true),
                    TotalScore = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ConfidenceRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SignalType = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlgorithmVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignalSnapshots_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignalSnapshots_StockId",
                table: "SignalSnapshots",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalSnapshots");
        }
    }
}
