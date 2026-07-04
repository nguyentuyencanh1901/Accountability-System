using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.UserService.API.Migrations
{
    /// <inheritdoc />
    public partial class ExamSessionViolationCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViolationCount",
                table: "ExamSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViolationCount",
                table: "ExamSessions");
        }
    }
}
