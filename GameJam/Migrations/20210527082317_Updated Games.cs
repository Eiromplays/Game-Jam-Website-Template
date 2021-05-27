using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GameJam.Migrations
{
    public partial class UpdatedGames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Publisher",
                table: "Games",
                newName: "PublisherUserId");

            migrationBuilder.RenameColumn(
                name: "Picture",
                table: "Games",
                newName: "DownloadLink");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Games",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<List<string>>(
                name: "Images",
                table: "Games",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Videos",
                table: "Games",
                type: "text[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Images",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Videos",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "PublisherUserId",
                table: "Games",
                newName: "Publisher");

            migrationBuilder.RenameColumn(
                name: "DownloadLink",
                table: "Games",
                newName: "Picture");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Games",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
