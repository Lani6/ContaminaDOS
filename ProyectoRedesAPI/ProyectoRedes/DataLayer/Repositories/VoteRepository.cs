using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class VoteRepository : IVoteRepository
    {
        private readonly GameDbContext _context;

        public VoteRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task AddVoteAsync(Vote vote)
        {
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();
        }

        public async Task<Vote> GetVoteByRoundAndPlayerAsync(Guid roundId, string playerName)
        {
            return await _context.Votes
                .FirstOrDefaultAsync(v => v.RoundId == roundId && v.PlayerName == playerName);
        }
        public async Task<List<Vote>> GetVotesByRoundId(Guid roundId) 
        {
            return await _context.Votes
                .Where(v => v.RoundId == roundId)
                .ToListAsync();
        }

        public async Task ClearVotesForRound(Guid roundId)
        {
            var votes = _context.Votes.Where(v => v.RoundId == roundId);
            _context.Votes.RemoveRange(votes);
            await _context.SaveChangesAsync();
        }
    }
}
