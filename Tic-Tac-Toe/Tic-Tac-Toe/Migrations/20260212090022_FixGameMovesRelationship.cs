using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tic_Tac_Toe.Migrations
{
    /// <inheritdoc />
    public partial class FixGameMovesRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаляем ограничение, если оно существует
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_constraint 
                        WHERE conname = 'FK_Moves_Games_GameDtoId'
                    ) THEN
                        ALTER TABLE ""Moves"" DROP CONSTRAINT ""FK_Moves_Games_GameDtoId"";
                    END IF;
                END $$;
            ");

            // Удаляем индекс, если он существует
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Moves_GameDtoId"";
            ");

            // Удаляем столбец, если он существует
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Moves' 
                        AND column_name = 'GameDtoId'
                    ) THEN
                        ALTER TABLE ""Moves"" DROP COLUMN ""GameDtoId"";
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameDtoId",
                table: "Moves",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_GameDtoId",
                table: "Moves",
                column: "GameDtoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Moves_Games_GameDtoId",
                table: "Moves",
                column: "GameDtoId",
                principalTable: "Games",
                principalColumn: "id");
        }
    }
}
