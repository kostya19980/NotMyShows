using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NotMyShows.Data.Series
{
    public partial class ProfileUpdate_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RaitingDate",
                table: "UserSeries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedDate",
                table: "UserSeries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaitingDate",
                table: "UserSeries");

            migrationBuilder.DropColumn(
                name: "StatusChangedDate",
                table: "UserSeries");
        }
    }
}
