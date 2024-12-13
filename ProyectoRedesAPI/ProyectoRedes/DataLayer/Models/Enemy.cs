using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRedes.DataLayer.Models
{
    public class Enemy
    {
        [Key]
        public Guid Id { get; set; }

        [Column("game_id")]
        public Guid GameId { get; set; }

        [Column("enemy_name")]
        public string EnemyName { get; set; }

        // Relación con la entidad Game
        [ForeignKey("GameId")]
        public Game Game { get; set; }
    }
}
