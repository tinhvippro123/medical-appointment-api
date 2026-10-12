using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalAppointment.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameRoleCodeToKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Roles",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                newName: "IX_Roles_Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Roles",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Key",
                table: "Roles",
                newName: "IX_Roles_Code");
        }
    }
}
