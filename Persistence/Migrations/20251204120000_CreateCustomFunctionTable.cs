using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateCustomFunctionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomFunctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    InputParameters = table.Column<string>(type: "jsonb", nullable: true),
                    OutputParameters = table.Column<string>(type: "jsonb", nullable: true),
                    SourceCode = table.Column<string>(type: "text", nullable: false),
                    Dependencies = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFunctions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFunctions");
        }
    }
}
