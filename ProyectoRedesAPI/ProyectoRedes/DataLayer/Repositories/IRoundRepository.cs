using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IRoundRepository
    {
        Task CreateRound(Round round);
        Task<Round> GetRoundById(Guid gameId, Guid roundId);
        Task<IEnumerable<Round>> GetRoundsByGameId(Guid gameId);
        Task UpdateRound(Round round);
    }
}
