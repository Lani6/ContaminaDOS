using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRedes.DataLayer.Models
{
    public class Round
    {
        [Key]
        public Guid Id { get; set; }

        [Column("game_id")]
        public Guid GameId { get; set; }

        [Column("leader")]
        public string Leader { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("result")]
        public string Result { get; set; }

        [Column("phase")]
        public string Phase { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("GameId")]
        public Game Game { get; set; }
    }
}
