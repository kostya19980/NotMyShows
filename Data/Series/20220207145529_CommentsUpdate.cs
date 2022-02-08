using Microsoft.EntityFrameworkCore.Migrations;

namespace NotMyShows.Data.Series
{
    public partial class CommentsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Series_SeriesId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "SeriesId",
                table: "Comments",
                newName: "EpisodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_SeriesId",
                table: "Comments",
                newName: "IX_Comments_EpisodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Episode_EpisodeId",
                table: "Comments",
                column: "EpisodeId",
                principalTable: "Episode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Episode_EpisodeId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "EpisodeId",
                table: "Comments",
                newName: "SeriesId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_EpisodeId",
                table: "Comments",
                newName: "IX_Comments_SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Series_SeriesId",
                table: "Comments",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
