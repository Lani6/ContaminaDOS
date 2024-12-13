using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoRedes.DataLayer.Models
{
    public class Group
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } 

        [ForeignKey("Round")]
        public Guid RoundId { get; set; }

        [Column("player_name")]
        public string PlayerName { get; set; } 

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        public Round Round { get; set; }
    }
}
