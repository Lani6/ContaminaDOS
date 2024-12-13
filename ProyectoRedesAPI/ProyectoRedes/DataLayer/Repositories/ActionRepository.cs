using ProyectoRedes.DataLayer.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoRedes.DataLayer.Context;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class ActionRepository : IActionRepository
    {
        private readonly GameDbContext _context;

        public ActionRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task AddActionAsync(Actions action)
        {
            _context.Actions.Add(action);
            await _context.SaveChangesAsync();
        }

        public async Task<Actions> GetActionByRoundAndPlayerAsync(Guid roundId, string playerName)
        {
            return await _context.Actions
                .FirstOrDefaultAsync(a => a.RoundId == roundId && a.PlayerName == playerName);
        }
        public async Task<IEnumerable<Actions>> GetActionsByRoundId(Guid roundId) // Implementación del nuevo método
        {
            return await _context.Actions
                .Where(a => a.RoundId == roundId)
                .ToListAsync();
        }
    }
}
