using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly GameDbContext _context;

        public PlayerRepository(GameDbContext context)
        {
            _context = context;
        }

        // Obtener un jugador por el nombre y el ID del juego
        public async Task<Player?> GetPlayerByGameAndName(Guid gameId, string playerName)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.GameId == gameId && p.PlayerName == playerName);
        }

        // Agregar un nuevo jugador
        public async Task AddPlayer(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        }
    }
}
