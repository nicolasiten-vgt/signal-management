using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateSignalProcessorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignalProcessors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RecomputeTrigger = table.Column<int>(type: "integer", nullable: false),
                    RecomputeIntervalSec = table.Column<int>(type: "integer", nullable: true),
                    ComputeGraph = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalProcessors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignalProcessors_Name",
                table: "SignalProcessors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalProcessors");
        }
    }
}
