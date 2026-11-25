using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalAppointment.API.Migrations
{
    /// <inheritdoc />
    public partial class MoveConsultationFeeToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Giữ lại giá bác sĩ cũ để không mất dữ liệu; ứng dụng không còn dùng cột này.
            migrationBuilder.Sql("ALTER TABLE [Doctors] ADD CONSTRAINT [DF_Doctors_LegacyConsultationFee] DEFAULT 0 FOR [ConsultationFee]");

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationFee",
                table: "Departments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 50000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationFee",
                table: "Departments");

            migrationBuilder.Sql("ALTER TABLE [Doctors] DROP CONSTRAINT [DF_Doctors_LegacyConsultationFee]");
        }
    }
}
