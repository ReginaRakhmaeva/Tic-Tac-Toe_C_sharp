using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tic_Tac_Toe.datasource.model;
using Tic_Tac_Toe.datasource.dbcontext.Converters;

namespace Tic_Tac_Toe.datasource.dbcontext;

/// Контекст базы данных для работы с Entity Framework Core и PostgreSQL
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserDto> Users { get; set; }

    public DbSet<GameDto> Games { get; set; }

    public DbSet<MoveDto> Moves { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var gameBoardConverter = new ValueConverter<GameBoardDto, string>(
            v => GameBoardJsonConverter.Serialize(v),
            v => GameBoardJsonConverter.Deserialize(v));

        var moveHistoryConverter = new ValueConverter<List<MoveDto>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<MoveDto>>(v, (JsonSerializerOptions?)null) ?? new List<MoveDto>());

        var moveHistoryComparer = new ValueComparer<List<MoveDto>>(
            (c1, c2) => MoveHistoryComparer.Compare(c1, c2),
            c => MoveHistoryComparer.GetHashCode(c),
            c => MoveHistoryComparer.Clone(c));

        modelBuilder.Entity<UserDto>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login).HasColumnName("login").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(255);
            
            entity.HasIndex(e => e.Login).IsUnique();
        });

        modelBuilder.Entity<GameDto>(entity =>
        {
            entity.ToTable("Games");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Player1Id).HasColumnName("player1_id");
            entity.Property(e => e.Player2Id).HasColumnName("player2_id");
            entity.Property(e => e.CurrentPlayerId).HasColumnName("current_player_id");
            entity.Property(e => e.WinnerId).HasColumnName("winner_id");
            entity.Property(e => e.Board)
                .HasColumnName("board")
                .HasColumnType("jsonb")
                .IsRequired()
                .HasConversion(gameBoardConverter);
            entity.Property(e => e.MoveHistory)
                .HasColumnName("move_history")
                .HasColumnType("jsonb")
                .HasConversion(moveHistoryConverter, moveHistoryComparer);
        });

        modelBuilder.Entity<MoveDto>(entity =>
        {
            entity.ToTable("Moves");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Row).HasColumnName("row").IsRequired();
            entity.Property(e => e.Col).HasColumnName("col").IsRequired();
            entity.Property(e => e.Player).HasColumnName("player").IsRequired();
            entity.Property(e => e.GameId).HasColumnName("game_id").IsRequired();
        });
    }
}
