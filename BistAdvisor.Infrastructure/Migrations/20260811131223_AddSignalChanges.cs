using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BistAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignalChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignalChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    PreviousSignalType = table.Column<int>(type: "int", nullable: false),
                    NewSignalType = table.Column<int>(type: "int", nullable: false),
                    PreviousScore = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    NewScore = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    PreviousConfidenceRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NewConfidenceRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ChangeTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlgorithmVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignalChanges_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignalChanges_StockId",
                table: "SignalChanges",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalChanges");
        }
    }
}
