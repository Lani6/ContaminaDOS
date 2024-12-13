using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player?> GetPlayerByGameAndName(Guid gameId, string playerName);  // Obtener jugador por ID del juego y nombre
        Task AddPlayer(Player player);  // Agregar nuevo jugador
    }
}
