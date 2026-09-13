using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGeneralSettingsRatesToPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeductionRatePerHour",
                table: "GeneralSettings",
                newName: "DeductionRatePercentage");

            migrationBuilder.RenameColumn(
                name: "AdditionRatePerHour",
                table: "GeneralSettings",
                newName: "AdditionRatePercentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeductionRatePercentage",
                table: "GeneralSettings",
                newName: "DeductionRatePerHour");

            migrationBuilder.RenameColumn(
                name: "AdditionRatePercentage",
                table: "GeneralSettings",
                newName: "AdditionRatePerHour");
        }
    }
}
