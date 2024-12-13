using ProyectoRedes.DataLayer.Models;
using System;
using System.Threading.Tasks;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IActionRepository
    {
        Task AddActionAsync(Actions action);
        Task<Actions> GetActionByRoundAndPlayerAsync(Guid roundId, string playerName);
        Task<IEnumerable<Actions>> GetActionsByRoundId(Guid roundId);
    }
}
