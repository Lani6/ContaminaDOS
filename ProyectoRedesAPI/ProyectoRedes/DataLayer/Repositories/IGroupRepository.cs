using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IGroupRepository
    {
        Task AddGroupAsync(Group group);
        Task<IEnumerable<Group>> GetGroupsByRoundId(Guid roundId); 
        Task DeleteGroupAsync(Guid groupId);

        Task ClearGroupsByRoundId(Guid roundId);
    }
}
