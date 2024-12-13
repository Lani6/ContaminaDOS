using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRedes.DataLayer.Models
{
    public class Vote
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("Round")]
        [Column("round_id")]
        public Guid RoundId { get; set; }

        [Column("player_name")]
        public string PlayerName { get; set; }

        [Column("vote")]
        public bool VoteValue { get; set; } 

        public Round Round { get; set; } 
    }
}
