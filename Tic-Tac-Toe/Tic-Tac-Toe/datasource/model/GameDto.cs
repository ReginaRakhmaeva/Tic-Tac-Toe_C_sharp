using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tic_Tac_Toe.datasource.model;

[Table("Games")]
public class GameDto
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("game_type")]
    public int GameType { get; set; }

    [Column("player1_id")]
    public Guid? Player1Id { get; set; }

    
    [Column("player2_id")]
    public Guid? Player2Id { get; set; }

    [Column("current_player_id")]
    public Guid? CurrentPlayerId { get; set; }

    [Column("winner_id")]
    public Guid? WinnerId { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("board", TypeName = "jsonb")]
    public GameBoardDto Board { get; set; }

    public List<MoveDto> Moves { get; set; }

    public GameDto()
    {
        Id = Guid.NewGuid();
        UserId = Guid.Empty;
        GameType = 0; // Computer
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        CreatedAt = DateTime.UtcNow;
        Board = new GameBoardDto();
        Moves = new List<MoveDto>();
    }
}

