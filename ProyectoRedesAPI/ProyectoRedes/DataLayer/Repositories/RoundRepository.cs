using Microsoft.EntityFrameworkCore;
using ProyectoRedes.DataLayer.Context;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.DataLayer.Repositories
{
    public class RoundRepository : IRoundRepository
    {
        private readonly GameDbContext _context;

        public RoundRepository(GameDbContext context)
        {
            _context = context;
        }

        public async Task CreateRound(Round round)
        {
            bool savedSuccessfully = false;
            int retryCount = 0;

            while (!savedSuccessfully && retryCount < 3) 
            {
                try
                {
                    _context.Rounds.Add(round);
                    await _context.SaveChangesAsync();
                    savedSuccessfully = true; 
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    Console.WriteLine($"Concurrency Exception: Reintento {retryCount}");

                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Round)
                        {
                            var databaseValues = await entry.GetDatabaseValuesAsync();

                            if (databaseValues == null)
                            {
                                throw new Exception("Unable to save the changes. The round was deleted.");
                            }

                            entry.OriginalValues.SetValues(databaseValues);
                        }
                    }
                }
            }

            if (!savedSuccessfully)
            {
                throw new Exception("Unable to save the round after multiple retries.");
            }
        }
        public async Task<IEnumerable<Round>> GetRoundsByGameId(Guid gameId){
            return await _context.Rounds.Where(r => r.GameId == gameId).ToListAsync();
        }

        public async Task<Round> GetRoundById(Guid gameId, Guid roundId)
        {
            return await _context.Rounds.FirstOrDefaultAsync(r => r.GameId == gameId && r.Id == roundId);
        }

        public async Task UpdateRound(Round round)
        {
            _context.Rounds.Update(round);
            await _context.SaveChangesAsync();
        }

    }
}