using ProyectoRedes.DTOs;
using ProyectoRedes.DataLayer.Models;

namespace ProyectoRedes.Services
{
    public interface IGameService
    {
        Task<Game> CreateGame(string name, string owner, string password);
        Task<IEnumerable<GameResponseDto>> GetAllGames();
        Task<IEnumerable<GameResponseDto>> SearchGames(string? name, string? status, int page, int limit);
        Task<Game> GetGameById(Guid gameId);
        Task<Game> JoinGame(Guid gameId, string playerName, string? password);
        Task UpdateGame(Game game);  
        Task StartGame(Guid gameId, string player);
        Task<IEnumerable<Round>> GetRoundsByGameId(Guid gameId);
        Task<Round> GetRoundById(Guid gameId, Guid roundId);
        Task ProposeGroup(Guid gameId, Guid roundId, string leader, List<string> playerNames);
        Task SubmitVote(Guid gameId, Guid roundId, string playerName, bool vote);
        Task SubmitAction(Guid gameId, Guid roundId, string playerName, bool action);
        Task<IEnumerable<Actions>> GetActionsByRoundId(Guid roundId);
        Task<RoundOutcomeResult> CheckRoundOutcome(Game game, Guid roundId, IEnumerable<Actions> allActions);
        Task<bool> GameExistsByName(string name);

    }

    public class RoundOutcomeResult
    {
        public bool GameEnded { get; set; }
        public string RoundResult { get; set; }
    }
}
