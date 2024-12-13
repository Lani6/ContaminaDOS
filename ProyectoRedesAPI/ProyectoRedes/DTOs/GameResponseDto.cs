using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DTOs
{
    public class GameResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasPassword { get; set; }
        public Guid? CurrentRound { get; set; }
        public List<string> Players { get; set; }
        public List<string> Enemies { get; set; }
    }

}

