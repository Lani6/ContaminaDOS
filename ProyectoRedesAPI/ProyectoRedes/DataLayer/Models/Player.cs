using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRedes.DataLayer.Models
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; }

        [Column("game_id")]
        public Guid GameId { get; set; }

        [Column("player_name")]
        public string PlayerName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relación con la entidad Game
        [ForeignKey("GameId")]
        public Game Game { get; set; }

    }
}
