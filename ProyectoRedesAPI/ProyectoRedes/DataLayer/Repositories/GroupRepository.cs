using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly GameDbContext _context;

        public GroupRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task AddGroupAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Group>> GetGroupsByRoundId(Guid roundId)
        {
            return await _context.Groups
                                 .Where(g => g.RoundId == roundId)
                                 .ToListAsync();
        }

        public async Task DeleteGroupAsync(Guid groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearGroupsByRoundId(Guid roundId)
        {
            var groups = await _context.Groups.Where(g => g.RoundId == roundId).ToListAsync();
            _context.Groups.RemoveRange(groups);
            await _context.SaveChangesAsync();
        }
    }
}
