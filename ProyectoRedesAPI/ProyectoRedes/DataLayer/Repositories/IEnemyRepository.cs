using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public interface IEnemyRepository
    {
        Task AddEnemy(Enemy enemy);  
    }
}
