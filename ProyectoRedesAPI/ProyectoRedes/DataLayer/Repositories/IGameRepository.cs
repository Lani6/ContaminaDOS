using ProyectoRedes.DataLayer.Models;
using ProyectoRedes.DTOs;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IGameRepository
    {
        Task<Game> GetGameById(Guid gameId);
        Task<IEnumerable<Game>> GetAllGames();
        Task<IEnumerable<Game>> SearchGames(string? name, string? status, int page, int limit);
        Task CreateGame(Game game);
        Task UpdateGame(Game game);
        Task<IEnumerable<Game>> GetGamesByName(string name);

    }
}
