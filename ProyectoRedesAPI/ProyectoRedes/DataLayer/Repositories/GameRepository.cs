using Microsoft.EntityFrameworkCore;
using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly GameDbContext _context;

        public GameRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Game>> GetAllGames()
        {
            return await _context.Games
                .Include(g => g.Players)
                .Include(g => g.Enemies)
                .ToListAsync();
        }

        public async Task<Game> GetGameById(Guid gameId)
        {
            return await _context.Games
                .Include(g => g.Players)  
                .Include(g => g.Enemies)  
                .FirstOrDefaultAsync(g => g.Id == gameId);
        }

        public async Task CreateGame(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGame(Game game)
        {
            _context.Games.Update(game);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Game>> SearchGames(string? name, string? status, int page, int limit)
        {
            var query = _context.Games
                .Include(g => g.Players)  // Incluye los jugadores relacionados
                .Include(g => g.Enemies)  // Incluye los enemigos relacionados
                .AsQueryable();

            // Filtra por nombre si se proporciona
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(g => g.Name.Contains(name));
            }

            // Filtra por estado si se proporciona
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(g => g.Status == status);
            }

            // Aplica la paginación
            query = query.Skip(page * limit).Take(limit);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetGamesByName(string name)
        {
            return await _context.Games
                .Where(g => g.Name == name)
                .ToListAsync();
        }
    }
}
