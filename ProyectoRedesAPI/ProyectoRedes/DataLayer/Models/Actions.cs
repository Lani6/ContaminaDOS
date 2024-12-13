using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRedes.DataLayer.Models
{
    public class Actions
    {
        [Key]
        public Guid Id { get; set; }

        [Column("round_id")]
        public Guid RoundId { get; set; }

        [Column("player_name")]
        public string PlayerName { get; set; }

        [Column("action")]
        public bool ActionValue { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relación con la entidad Round
        [ForeignKey("RoundId")]
        public Round Round { get; set; }
    }
}
