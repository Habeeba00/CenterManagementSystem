using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CenterManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_Attendance2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                table: "InstructorAttendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "InstructorAttendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAttendances_SessionId",
                table: "InstructorAttendances",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorAttendances_Sessions_SessionId",
                table: "InstructorAttendances",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorAttendances_Sessions_SessionId",
                table: "InstructorAttendances");

            migrationBuilder.DropIndex(
                name: "IX_InstructorAttendances_SessionId",
                table: "InstructorAttendances");

            migrationBuilder.DropColumn(
                name: "IsLate",
                table: "InstructorAttendances");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "InstructorAttendances");
        }
    }
}
