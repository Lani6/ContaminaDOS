namespace ProyectoRedes.DTOs
{
    public class CreateGameRequest
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string? Password { get; set; }
    }
}
