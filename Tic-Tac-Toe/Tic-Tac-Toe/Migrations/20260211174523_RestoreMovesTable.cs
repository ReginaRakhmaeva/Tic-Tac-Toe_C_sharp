using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tic_Tac_Toe.Migrations
{
    /// <inheritdoc />
    public partial class RestoreMovesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "move_history",
                table: "Games");

            migrationBuilder.CreateTable(
                name: "Moves",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    row = table.Column<int>(type: "integer", nullable: false),
                    col = table.Column<int>(type: "integer", nullable: false),
                    player = table.Column<int>(type: "integer", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moves", x => x.id);
                    table.ForeignKey(
                        name: "FK_Moves_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_game_id",
                table: "Moves",
                column: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Moves");

            migrationBuilder.AddColumn<string>(
                name: "move_history",
                table: "Games",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
