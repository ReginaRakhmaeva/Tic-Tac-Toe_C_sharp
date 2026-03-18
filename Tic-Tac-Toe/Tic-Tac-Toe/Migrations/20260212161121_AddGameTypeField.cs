using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tic_Tac_Toe.Migrations
{
    /// <inheritdoc />
    public partial class AddGameTypeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "game_type",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Обновляем существующие игры: если Player2Id не null, то это TwoPlayer игра
            migrationBuilder.Sql(@"
                UPDATE ""Games""
                SET game_type = 1
                WHERE player2_id IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "game_type",
                table: "Games");
        }
    }
}
