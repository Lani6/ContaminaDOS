using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class EnemyRepository : IEnemyRepository
    {
        private readonly GameDbContext _context;

        public EnemyRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task AddEnemy(Enemy enemy)
        {
            _context.Enemies.Add(enemy);
            await _context.SaveChangesAsync();
        }
    }
}
