using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.UserService.API.Migrations
{
    /// <inheritdoc />
    public partial class dropExemSetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ExamSets_ExamSetId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ExamSetId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ExamSetId",
                table: "Questions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ExamSetId",
                table: "Questions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamSetId",
                table: "Questions",
                column: "ExamSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ExamSets_ExamSetId",
                table: "Questions",
                column: "ExamSetId",
                principalTable: "ExamSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
