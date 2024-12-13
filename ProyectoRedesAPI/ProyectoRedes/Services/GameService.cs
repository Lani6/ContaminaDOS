using ProyectoRedes.DTOs;
using ProyectoRedes.DataLayer.Models;
using ProyectoRedes.DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoRedes.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IRoundRepository _roundRepository;
        private readonly IEnemyRepository _enemyRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IActionRepository _actionRepository;

        public GameService(
            IGameRepository gameRepository,
            IPlayerRepository playerRepository,
            IRoundRepository roundRepository,
            IEnemyRepository enemyRepository,
            IGroupRepository groupRepository,
            IVoteRepository voteRepository,
            IActionRepository actionRepository)
        {
            _gameRepository = gameRepository;
            _playerRepository = playerRepository;
            _roundRepository = roundRepository;
            _enemyRepository = enemyRepository;
            _groupRepository = groupRepository;
            _voteRepository = voteRepository;
            _actionRepository = actionRepository;
        }

        public async Task<Game> CreateGame(string name, string owner, string? password)
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Name = name,
                Password = string.IsNullOrEmpty(password) ? null : password,
                Status = "lobby",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Players = new List<Player> { new Player { Id = Guid.NewGuid(), PlayerName = owner, CreatedAt = DateTime.Now } },
                Enemies = new List<Enemy>()
            };

            await _gameRepository.CreateGame(game);
            return game;
        }

        public async Task<bool> GameExistsByName(string name)
        {
            var games = await _gameRepository.GetGamesByName(name);
            return games.Any();
        }

        public async Task<IEnumerable<GameResponseDto>> GetAllGames()
        {
            var games = await _gameRepository.GetAllGames();
            return games.Select(g => new GameResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                Owner = g.Players.FirstOrDefault()?.PlayerName ?? "Unknown",
                Status = g.Status,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                HasPassword = g.HasPassword,
                CurrentRound = g.CurrentRound,
                Players = g.Players.Select(p => p.PlayerName).ToList(),
                Enemies = g.Enemies.Select(e => e.EnemyName).ToList()
            });
        }

        public async Task<Game> GetGameById(Guid gameId)
        {
            var game = await _gameRepository.GetGameById(gameId);
            if (game == null)
                throw new KeyNotFoundException("Game not found");

            return game;
        }

        public async Task<Game> JoinGame(Guid gameId, string playerName, string? password)
        {
            var game = await _gameRepository.GetGameById(gameId);
            if (game == null)
                throw new Exception("Game not found");

            if (game.HasPassword && game.Password != password)
                throw new Exception("Invalid password");

            var existingPlayer = await _playerRepository.GetPlayerByGameAndName(gameId, playerName);
            if (existingPlayer != null)
                throw new Exception("Player is already in the game");

            var newPlayer = new Player
            {
                Id = Guid.NewGuid(),
                PlayerName = playerName,
                GameId = gameId,
                CreatedAt = DateTime.Now
            };

            await _playerRepository.AddPlayer(newPlayer);
            return game;
        }

        public async Task UpdateGame(Game game)
        {
            await _gameRepository.UpdateGame(game);
        }

        public async Task<IEnumerable<GameResponseDto>> SearchGames(string? name, string? status, int page, int limit)
        {
            var games = await _gameRepository.SearchGames(name, status, page, limit);
            return games.Select(g => new GameResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                Owner = g.Players.FirstOrDefault()?.PlayerName ?? "Unknown",
                Status = g.Status,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                HasPassword = g.HasPassword,
                CurrentRound = g.CurrentRound,
                Players = g.Players.Select(p => p.PlayerName).ToList(),
                Enemies = g.Enemies.Select(e => e.EnemyName).ToList()
            });
        }

        public async Task StartGame(Guid gameId, string playerName)
        {
            var game = await _gameRepository.GetGameById(gameId);
            if (game == null)
                throw new Exception("Game not found");

            int numPlayers = game.Players.Count;
            int numEnemies = numPlayers switch
            {
                5 or 6 => 2,
                7 or 8 or 9 => 3,
                10 => 4,
                _ => throw new Exception("Invalid number of players. Must be between 5 and 10.")
            };

            Random random = new Random();
            var selectedEnemies = game.Players.OrderBy(p => random.Next()).Take(numEnemies).ToList();

            foreach (var enemyPlayer in selectedEnemies)
            {
                var enemy = new Enemy
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    EnemyName = enemyPlayer.PlayerName
                };
                await _enemyRepository.AddEnemy(enemy);
            }

            var newRound = new Round
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Leader = playerName,
                Status = "waiting-on-leader",
                Result = "none",
                Phase = "vote1",
                CreatedAt = DateTime.Now
            };

            await _roundRepository.CreateRound(newRound);
            game.CurrentRound = newRound.Id;
            game.Status = "rounds";
            await _gameRepository.UpdateGame(game);
        }

        public async Task<IEnumerable<Round>> GetRoundsByGameId(Guid gameId)
        {
            return await _roundRepository.GetRoundsByGameId(gameId);
        }

        public async Task<Round> GetRoundById(Guid gameId, Guid roundId)
        {
            var round = await _roundRepository.GetRoundById(gameId, roundId);
            if (round == null)
                throw new KeyNotFoundException("Round not found");

            return round;
        }

        public async Task StartNewRound(Guid gameId)
        {
            var game = await _gameRepository.GetGameById(gameId);
            if (game.Status == "ended")
                throw new Exception("Game has already ended.");

            int roundNumber = (await _roundRepository.GetRoundsByGameId(gameId)).Count() + 1;

            Random random = new Random();
            var leader = game.Players.OrderBy(p => random.Next()).First().PlayerName;

            var newRound = new Round
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Leader = leader,
                Status = "waiting-on-leader",
                Result = "none",
                Phase = "vote1",
                CreatedAt = DateTime.Now
            };

            await _roundRepository.CreateRound(newRound);
            game.CurrentRound = newRound.Id;
            await _gameRepository.UpdateGame(game);
        }

        public async Task ProposeGroup(Guid gameId, Guid roundId, string leader, List<string> playerNames)
        {
            var game = await _gameRepository.GetGameById(gameId);
            var round = await _roundRepository.GetRoundById(gameId, roundId);

            if (round == null)
                throw new KeyNotFoundException("Round not found");

            if (round.Leader != leader)
                throw new UnauthorizedAccessException("Only the round leader can propose a group.");

            int roundNumber = (await _roundRepository.GetRoundsByGameId(gameId)).Count(r => r.Status == "ended") + 1;

            if (roundNumber < 1 || roundNumber > 5)
                throw new Exception($"Invalid round number: {roundNumber}");

            int requiredGroupSize = GetRequiredGroupSize(game.Players.Count, roundNumber);

            if (playerNames.Count != requiredGroupSize)
                throw new Exception($"This round requires a group of {requiredGroupSize} players.");

            await _groupRepository.ClearGroupsByRoundId(roundId);

            foreach (var playerName in playerNames)
            {
                var group = new Group
                {
                    Id = Guid.NewGuid(),
                    RoundId = roundId,
                    PlayerName = playerName,
                    CreatedAt = DateTime.Now
                };
                await _groupRepository.AddGroupAsync(group);
            }

            round.Status = "waiting-on-leader";
            await _roundRepository.UpdateRound(round);
        }
        private int GetRequiredGroupSize(int numPlayers, int roundNumber)
        {
            var groupSizes = new Dictionary<int, int[]>
            {
                { 5, new[] { 2, 3, 2, 3, 3 } },
                { 6, new[] { 2, 3, 4, 3, 4 } },
                { 7, new[] { 2, 3, 3, 4, 4 } },
                { 8, new[] { 3, 4, 4, 5, 5 } },
                { 9, new[] { 3, 4, 4, 5, 5 } },
                { 10, new[] { 3, 4, 4, 5, 5 } }
            };

            if (!groupSizes.ContainsKey(numPlayers))
                throw new Exception("Invalid number of players. Must be between 5 and 10.");

            return groupSizes[numPlayers][roundNumber - 1];
        }

        public async Task SubmitVote(Guid gameId, Guid roundId, string playerName, bool vote)
        {
            var game = await _gameRepository.GetGameById(gameId);
            if (game == null)
                throw new Exception("Game not found");

            var round = await _roundRepository.GetRoundById(gameId, roundId);
            if (round == null)
                throw new Exception("Round not found");

            if (game.Status == "ended")
                throw new Exception("Game has already ended.");

            var newVote = new Vote
            {
                Id = Guid.NewGuid(),
                RoundId = roundId,
                PlayerName = playerName,
                VoteValue = vote
            };

            await _voteRepository.AddVoteAsync(newVote);

            var allVotes = await _voteRepository.GetVotesByRoundId(roundId);
            if (allVotes.Count() == game.Players.Count)
            {
                int positiveVotes = allVotes.Count(v => v.VoteValue);
                int negativeVotes = allVotes.Count(v => !v.VoteValue);

                if (positiveVotes > negativeVotes)
                {
                    round.Status = "waiting-on-leader";
                    round.Result = "none";
                }
                else
                {
                    await _groupRepository.ClearGroupsByRoundId(roundId);

                    if (round.Phase == "vote1")
                    {
                        round.Phase = "vote2";
                    }
                    else if (round.Phase == "vote2")
                    {
                        round.Phase = "vote3";
                    }
                    else
                    {
                        game.EnemyPoints += 1;
                        round.Result = "enemies";

                        if (game.EnemyPoints >= 3)
                        {
                            game.Status = "ended";
                            round.Result = "enemies-win";
                        }
                        else
                        {
                            await StartNewRound(gameId);
                        }
                    }

                    await _voteRepository.ClearVotesForRound(roundId);
                    round.Status = "waiting-on-leader";
                }

                await _roundRepository.UpdateRound(round);
                await _gameRepository.UpdateGame(game);
            }
        }


        public async Task SubmitAction(Guid gameId, Guid roundId, string playerName, bool action)
        {
            var game = await _gameRepository.GetGameById(gameId);
            if (game == null)
                throw new Exception("Game not found");

            if (game.Status == "ended")
                throw new Exception("Game has already ended.");

            var round = await _roundRepository.GetRoundById(gameId, roundId);
            if (round == null || round.Status != "waiting-on-leader")
                throw new Exception("Round not in action phase or not found");

            var newAction = new Actions
            {
                Id = Guid.NewGuid(),
                RoundId = roundId,
                PlayerName = playerName,
                ActionValue = action,
                CreatedAt = DateTime.Now
            };

            await _actionRepository.AddActionAsync(newAction);

            var allActions = await _actionRepository.GetActionsByRoundId(roundId);
            if (allActions.Count() == (await _groupRepository.GetGroupsByRoundId(roundId)).Count())
            {
                bool sabotageOccurred = allActions.Any(a => !a.ActionValue);

                if (sabotageOccurred)
                {
                    game.EnemyPoints++;
                    round.Result = "enemies";

                    if (game.EnemyPoints >= 3)
                    {
                        game.Status = "ended";
                        round.Result = "enemies-win";
                        await _gameRepository.UpdateGame(game);
                        await _roundRepository.UpdateRound(round);
                        return;
                    }
                    else
                    {
                        await StartNewRound(gameId);
                    }
                }
                else
                {
                    game.CitizenPoints++;
                    round.Result = "citizens";

                    if (game.CitizenPoints >= 3)
                    {
                        game.Status = "ended";
                        round.Result = "citizens-win";
                        await _gameRepository.UpdateGame(game);
                        await _roundRepository.UpdateRound(round);
                        return;
                    }
                    else
                    {
                        await StartNewRound(gameId);
                    }
                }

                round.Status = "ended";
                await _roundRepository.UpdateRound(round);
                await _gameRepository.UpdateGame(game);
            }
        }

        private async Task StartNewRound(Guid gameId, string leader)
        {
            var game = await _gameRepository.GetGameById(gameId);
            int roundNumber = (await _roundRepository.GetRoundsByGameId(gameId)).Count() + 1; 

            var newRound = new Round
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Leader = leader,
                Status = "waiting-on-leader",
                Result = "none",
                Phase = "vote1",
                CreatedAt = DateTime.Now
            };

            Console.WriteLine($"Nueva ronda creada con ID: {newRound.Id}, Líder: {leader}, Estado: {newRound.Status}");

            await _roundRepository.CreateRound(newRound);
            game.CurrentRound = newRound.Id;
            await _gameRepository.UpdateGame(game);
        }


        public async Task<IEnumerable<Actions>> GetActionsByRoundId(Guid roundId)
        {
            return await _actionRepository.GetActionsByRoundId(roundId);
        }

        public async Task<RoundOutcomeResult> CheckRoundOutcome(Game game, Guid roundId, IEnumerable<Actions> allActions)
        {
            bool sabotageOccurred = allActions.Any(action => !action.ActionValue);
            var round = await _roundRepository.GetRoundById(game.Id, roundId);

            if (sabotageOccurred)
            {
                game.EnemyPoints++;
                if (game.EnemyPoints >= 3)
                {
                    game.Status = "ended";
                    round.Result = "enemies-win";  
                    await _gameRepository.UpdateGame(game);
                    await _roundRepository.UpdateRound(round);  
                    return new RoundOutcomeResult { GameEnded = true, RoundResult = "enemies-win" };
                }

                round.Result = "enemies";
                await _roundRepository.UpdateRound(round);
                return new RoundOutcomeResult { GameEnded = false, RoundResult = "enemies" };
            }
            else
            {
                game.CitizenPoints++;
                if (game.CitizenPoints >= 3)
                {
                    game.Status = "ended";
                    round.Result = "citizens-win";  
                    await _gameRepository.UpdateGame(game);
                    await _roundRepository.UpdateRound(round);  
                    return new RoundOutcomeResult { GameEnded = true, RoundResult = "citizens-win" };
                }

                round.Result = "citizens";
                await _roundRepository.UpdateRound(round);
                return new RoundOutcomeResult { GameEnded = false, RoundResult = "citizens" };
            }
        }
    }
}
