using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class UserPrivilage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivileged",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivileged",
                table: "Users");
        }
    }
}
