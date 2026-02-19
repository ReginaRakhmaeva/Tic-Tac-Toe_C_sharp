using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tic_Tac_Toe.datasource.model;

/// Модель пользователя для хранения в datasource слое
[Table("Users")]
public class UserDto
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("login")]
    [MaxLength(255)]
    public string Login { get; set; }

    [Required]
    [Column("password")]
    [MaxLength(255)]
    public string Password { get; set; }

    public UserDto()
    {
        Id = Guid.NewGuid();
        Login = string.Empty;
        Password = string.Empty;
    }

    public UserDto(Guid id, string login, string password)
    {
        Id = id;
        Login = login ?? string.Empty;
        Password = password ?? string.Empty;
    }
}
