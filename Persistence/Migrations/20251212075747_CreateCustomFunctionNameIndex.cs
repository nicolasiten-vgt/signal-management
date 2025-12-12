using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateCustomFunctionNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CustomFunctions_Name",
                table: "CustomFunctions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomFunctions_Name",
                table: "CustomFunctions");
        }
    }
}
