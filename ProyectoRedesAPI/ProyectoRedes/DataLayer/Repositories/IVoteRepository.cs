using System;
using System.Threading.Tasks;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IVoteRepository
    {
        Task AddVoteAsync(Vote vote);
        Task<Vote> GetVoteByRoundAndPlayerAsync(Guid roundId, string playerName);
        Task<List<Vote>> GetVotesByRoundId(Guid roundId);
        Task ClearVotesForRound(Guid roundId);

    }
}
