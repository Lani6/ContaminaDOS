using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRedes.DataLayer.Models
{
    public class Game
    {
        [Key]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("current_round")]
        public Guid? CurrentRound { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public bool HasPassword => !string.IsNullOrEmpty(Password);

        public List<Player> Players { get; set; } = new List<Player>();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();

        [NotMapped]
        public int EnemyPoints { get; set; } = 0;

        [NotMapped]
        public int CitizenPoints { get; set; } = 0;
    }
}
