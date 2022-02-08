using Microsoft.EntityFrameworkCore.Migrations;

namespace NotMyShows.Data.Series
{
    public partial class CommentsUpdate_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Episode_EpisodeId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Episode_Series_SeriesId",
                table: "Episode");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEpisodes_Episode_EpisodeId",
                table: "UserEpisodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Episode",
                table: "Episode");

            migrationBuilder.RenameTable(
                name: "Episode",
                newName: "Episodes");

            migrationBuilder.RenameIndex(
                name: "IX_Episode_SeriesId",
                table: "Episodes",
                newName: "IX_Episodes_SeriesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Episodes",
                table: "Episodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Episodes_EpisodeId",
                table: "Comments",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_Series_SeriesId",
                table: "Episodes",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEpisodes_Episodes_EpisodeId",
                table: "UserEpisodes",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Episodes_EpisodeId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_Series_SeriesId",
                table: "Episodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEpisodes_Episodes_EpisodeId",
                table: "UserEpisodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Episodes",
                table: "Episodes");

            migrationBuilder.RenameTable(
                name: "Episodes",
                newName: "Episode");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_SeriesId",
                table: "Episode",
                newName: "IX_Episode_SeriesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Episode",
                table: "Episode",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Episode_EpisodeId",
                table: "Comments",
                column: "EpisodeId",
                principalTable: "Episode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Episode_Series_SeriesId",
                table: "Episode",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEpisodes_Episode_EpisodeId",
                table: "UserEpisodes",
                column: "EpisodeId",
                principalTable: "Episode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
