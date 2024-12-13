using System.ComponentModel.DataAnnotations;

namespace ProyectoRedes.DTOs
{
    public class JoinGameRequest
    {
        [Required]
        public string Player { get; set; }

    }
}
