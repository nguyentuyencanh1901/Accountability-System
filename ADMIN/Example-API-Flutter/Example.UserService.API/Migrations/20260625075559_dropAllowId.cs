using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.UserService.API.Migrations
{
    /// <inheritdoc />
    public partial class dropAllowId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowTrial",
                table: "ExamPeriods");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowTrial",
                table: "ExamPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
