using Microsoft.AspNetCore.Mvc;
using ProyectoRedes.DataLayer.Models;
using ProyectoRedes.DataLayer.Repositories;
using ProyectoRedes.DTOs;
using ProyectoRedes.Services;
using System.Data;
using System.Linq;
using System.Numerics;

namespace ProyectoRedes.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GameController : Controller
    {
        private readonly IGameService _gameService;
        private readonly IGroupRepository _groupRepository;
        private readonly IVoteRepository _voteRepository;

        public GameController(IGameService gameService, IGroupRepository groupRepository, IVoteRepository voteRepository)
        {
            _gameService = gameService;
            _groupRepository = groupRepository;
            _voteRepository = voteRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 20)
            {
                return BadRequest(new { status = 400, msg = "Client Error: Invalid game name." });
            }

            if (string.IsNullOrWhiteSpace(request.Owner) || request.Owner.Length < 3 || request.Owner.Length > 20)
            {
                return BadRequest(new { status = 400, msg = "Client Error: Invalid owner name." });
            }

            if (request.Password != null && (request.Password.Length < 3 || request.Password.Length > 20))
            {
                return BadRequest(new { status = 400, msg = "Client Error: Invalid password." });
            }

            try
            {
                bool gameExists = await _gameService.GameExistsByName(request.Name);
                if (gameExists)
                {
                    return Conflict(new { status = 409, msg = "Asset already exists" });
                }

                var game = await _gameService.CreateGame(request.Name, request.Owner, request.Password);
                var owner = game.Players.FirstOrDefault()?.PlayerName ?? "Unknown";

                return CreatedAtAction(nameof(GetGame), new { gameId = game.Id }, new
                {
                    status = 201,
                    msg = "Game Created",
                    data = new
                    {
                        name = game.Name,
                        owner = owner,
                        status = game.Status,
                        createdAt = game.CreatedAt,
                        updatedAt = game.UpdatedAt,
                        password = game.HasPassword,
                        players = game.Players.Select(p => p.PlayerName).ToList(),
                        enemies = game.Enemies.Select(e => e.EnemyName).ToList(),
                        currentRound = game.CurrentRound.HasValue ? game.CurrentRound.ToString() : "0000000000000000000000000",
                        id = game.Id,
                    },
                    others = new { }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, msg = "Client Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGames([FromQuery] string? name, [FromQuery] string? status = "lobby", [FromQuery] int page = 0, [FromQuery] int limit = 50)
        {
            try
            {
                var games = await _gameService.SearchGames(name, status, page, limit);

                var result = games.Select(game => new
                {
                    name = game.Name,
                    owner = game.Owner,
                    status = game.Status,
                    createdAt = game.CreatedAt,
                    updatedAt = game.UpdatedAt,
                    password = game.HasPassword,
                    players = game.Players,
                    enemies = game.Enemies,
                    currentRound = game.CurrentRound.HasValue ? game.CurrentRound.ToString() : "0000000000000000000000000",
                    id = game.Id,
                });

                return Ok(new { status = 200, msg = "Games Found", data = result });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    status = 400,
                    msg = "Client Error"
                });
            }
        }

        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGame([FromRoute] Guid gameId, [FromHeader] string? password, [FromHeader] string player)
        {
            try
            {
                var game = await _gameService.GetGameById(gameId);

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                if (!game.Players.Any(p => p.PlayerName == player))
                {
                    return StatusCode(403, new { status = 403, msg = "Not part of the game" });
                }

                var result = new
                {
                    id = game.Id,
                    name = game.Name,
                    owner = game.Players.FirstOrDefault()?.PlayerName ?? "Unknown",
                    status = game.Status,
                    createdAt = game.CreatedAt,
                    updatedAt = game.UpdatedAt,
                    password = game.HasPassword,
                    players = game.Players.Select(p => p.PlayerName).ToList(),
                    enemies = game.Enemies.Select(e => e.EnemyName).ToList(),
                    currentRound = game.CurrentRound.HasValue ? game.CurrentRound.ToString() : "0000000000000000000000000",
                };

                return Ok(new { status = 200, msg = "Game Found", data = result });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { status = 404, msg = "The specified resource was not found" });
            }
        }

        [HttpPut("{gameId}")]
        public async Task<IActionResult> JoinGame(Guid gameId, [FromHeader(Name = "player")] string playerHeader, [FromHeader] string? password, [FromBody] JoinGameRequest request)
        {
            try
            {
                if (playerHeader != request.Player)
                {
                    return BadRequest(new { status = 400, msg = "Player name in the header and body must be the same" });
                }

                var game = await _gameService.GetGameById(gameId);

                if (game == null)
                {
                    return NotFound(new { status = 404, msg = "The specified resource was not found" });
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                if (game.Players.Any(p => p.PlayerName == request.Player))
                {
                    return Conflict(new { status = 409, msg = "Asset already exists" });
                }

                if (game.Status != "lobby")
                {
                    return StatusCode(428, new { status = 428, msg = "This action is not allowed at this time" });
                }

                var updatedGame = await _gameService.JoinGame(gameId, request.Player, password);

                return Ok(new
                {
                    status = 200,
                    msg = "Player joined successfully",
                    data = new
                    {
                        id = updatedGame.Id,
                        name = updatedGame.Name,
                        owner = updatedGame.Players.FirstOrDefault()?.PlayerName ?? "Unknown",
                        status = updatedGame.Status,
                        createdAt = updatedGame.CreatedAt,
                        updatedAt = updatedGame.UpdatedAt,
                        password = updatedGame.HasPassword,
                        players = updatedGame.Players.Select(p => p.PlayerName).ToList(),
                        enemies = updatedGame.Enemies.Select(e => e.EnemyName).ToList(),
                        currentRound = updatedGame.CurrentRound.HasValue ? updatedGame.CurrentRound.ToString() : "0000000000000000000000000"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, msg = ex.Message });
            }
        }

        [HttpHead("{gameId}/start")]
        public async Task<IActionResult> StartGame(Guid gameId, [FromHeader] string player, [FromHeader] string? password)
        {
            try
            {
                var game = await _gameService.GetGameById(gameId);

                if (game == null)
                {
                    Response.Headers.Add("X-msg", "Game not found.");
                    return StatusCode(404);
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    Response.Headers.Add("X-msg", "Unauthorized: Incorrect password.");
                    return StatusCode(401);
                }

                if (!game.Players.Any(p => p.PlayerName == player))
                {
                    Response.Headers.Add("X-msg", "Forbidden: Player not part of the game.");
                    return StatusCode(403);
                }

                if (game.Status != "lobby")
                {
                    Response.Headers.Add("X-msg", "Game already started.");
                    return StatusCode(409);
                }

                if (game.Players.Count < 5)
                {
                    Response.Headers.Add("X-msg", "Need 5 players to start.");
                    return StatusCode(428);
                }

                await _gameService.StartGame(gameId, player);

                Response.Headers.Add("X-msg", "Game started");
                return Ok();
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-msg", $"Client Error: {ex.Message}");
                return StatusCode(400);
            }
        }

        [HttpGet("{gameId}/rounds/{roundId}")]
        public async Task<IActionResult> GetRound(Guid gameId, Guid roundId, [FromHeader] string? password, [FromHeader] string player)
        {

            if ((player.Length < 3 || player.Length > 20) || (password != null && (password.Length < 3 || password.Length > 20)))
            {

            }

            try
            {
                var game = await _gameService.GetGameById(gameId);
                if (game == null)
                {
                    return NotFound(new { status = 404, msg = "Game Not Found" });
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                if (game.Status != "rounds" || game.CurrentRound != roundId)
                {
                    Response.Headers.Add("X-msg", "This action is not allowed at this time");
                    return StatusCode(428, new { status = 428, msg = "This action is not allowed at this time" });
                }

                var playerExists = game.Players.Any(p => p.PlayerName == player);
                if (!playerExists)
                {
                    return StatusCode(403, new { status = 403, msg = "Not part of the game" });
                }

                var round = await _gameService.GetRoundById(gameId, roundId);
                if (round == null)
                {
                    return NotFound(new { status = 404, msg = "The specified resource was not found" });
                }

                var group = await _groupRepository.GetGroupsByRoundId(round.Id);
                var votes = await _voteRepository.GetVotesByRoundId(round.Id);


                var response = new
                {
                    id = round.Id,
                    status = round.Status,
                    phase = round.Phase,
                    result = round.Result,
                    leader = round.Leader,
                    createdAt = round.CreatedAt,
                    updatedAt = DateTime.Now,
                    group = group.Select(g => g.PlayerName).ToList(),
                    votes = votes.Select(v => v.VoteValue).ToList()
                };

                return Ok(new { status = 200, msg = "Round found", data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, msg = ex.Message });
            }
        }

        [HttpGet("{gameId}/rounds")]
        public async Task<IActionResult> GetRounds(Guid gameId, [FromHeader] string? password, [FromHeader] string player)
        {
            try
            {
                var game = await _gameService.GetGameById(gameId);
                if (game == null)
                {
                    return NotFound(new { status = 404, msg = "The specified resource was not found" });
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                if (!game.Players.Any(p => p.PlayerName == player))
                {
                    return StatusCode(403, new { status = 403, msg = "Not part of the game" });
                }

                var rounds = await _gameService.GetRoundsByGameId(gameId);
                var response = new List<object>();

                foreach (var round in rounds)
                {
                    var group = await _groupRepository.GetGroupsByRoundId(round.Id);
                    var votes = await _voteRepository.GetVotesByRoundId(round.Id);

                    response.Add(new
                    {
                        id = round.Id,
                        status = round.Status,
                        phase = round.Phase,
                        result = round.Result,
                        leader = round.Leader,
                        createdAt = round.CreatedAt,
                        updatedAt = DateTime.Now,
                        group = group.Select(g => g.PlayerName).ToList(),
                        votes = votes.Select(v => v.VoteValue).ToList()
                    });
                }

                return Ok(new { status = 200, msg = "Rounds found", data = response, others = new { } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, msg = ex.Message });
            }
        }

        [HttpPatch("{gameId}/rounds/{roundId}")]
        public async Task<IActionResult> ProposeGroup(
            Guid gameId,
            Guid roundId,
            [FromHeader] string? password,
            [FromHeader(Name = "player")] string leader,
            [FromBody] ProposeGroupRequest request)
        {
            try
            {
                var game = await _gameService.GetGameById(gameId);
                if (game == null)
                {
                    return NotFound(new { status = 404, msg = "The specified resource was not found" });
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                await _gameService.ProposeGroup(gameId, roundId, leader, request.Group);

                var round = await _gameService.GetRoundById(gameId, roundId);
                if (round == null)
                {
                    return NotFound(new { status = 404, msg = "Round not found" });
                }

                var groupDetails = await _groupRepository.GetGroupsByRoundId(round.Id);
                var votes = await _voteRepository.GetVotesByRoundId(round.Id);

                var response = new
                {
                    id = round.Id,
                    status = round.Status,
                    phase = round.Phase,
                    result = round.Result,
                    leader = round.Leader,
                    createdAt = round.CreatedAt,
                    updatedAt = DateTime.Now,
                    group = groupDetails.Select(g => g.PlayerName).ToList(),
                    votes = votes.Select(v => v.VoteValue).ToList()
                };

                return Ok(new { status = 200, msg = "Group Created", data = response });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = 404, msg = "The specified resource was not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { status = 401, msg = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, msg = ex.Message });
            }
        }

        [HttpPost("{gameId}/rounds/{roundId}")]
        public async Task<IActionResult> SubmitVote(
            Guid gameId,
            Guid roundId,
            [FromHeader] string? password,
            [FromHeader] string player,
            [FromBody] VoteRequest voteRequest)
        {
            try
            {
                var game = await _gameService.GetGameById(gameId);
                if (game == null)
                {
                    Response.Headers.Add("X-msg", "The specified resource was not found");
                    return NotFound(new { status = 404, msg = "The specified resource was not found" });
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    Response.Headers.Add("X-msg", "Invalid credentials");
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                if (!game.Players.Any(p => p.PlayerName == player))
                {
                    Response.Headers.Add("X-msg", "Not part of the game");
                    return StatusCode(403, new { status = 403, msg = "Not part of the game" });
                }

                var existingVote = await _voteRepository.GetVoteByRoundAndPlayerAsync(roundId, player);
                if (existingVote != null)
                {
                    Response.Headers.Add("X-msg", "Asset already exists");
                    return Conflict(new { status = 409, msg = "Asset already exists" });
                }

                var round = await _gameService.GetRoundById(gameId, roundId);
                if (round == null)
                {
                    Response.Headers.Add("X-msg", "Round not found");
                    return NotFound(new { status = 404, msg = "Round not found" });
                }


                await _gameService.SubmitVote(gameId, roundId, player, voteRequest.Vote);

                var group = await _groupRepository.GetGroupsByRoundId(round.Id);
                var votes = await _voteRepository.GetVotesByRoundId(round.Id);

                var response = new
                {
                    status = 200,
                    msg = "Voted successfully",
                    data = new
                    {
                        id = round.Id,
                        status = round.Status,
                        phase = round.Phase,
                        result = round.Result,
                        leader = round.Leader,
                        createdAt = round.CreatedAt,
                        updatedAt = DateTime.Now,
                        group = group.Select(g => g.PlayerName).ToList(),
                        votes = votes.Select(v => v.VoteValue).ToList()
                    },
                    others = new { }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-msg", "Client error occurred");
                return BadRequest(new { status = 400, msg = "Client error occurred", detail = ex.Message });
            }
        }

        [HttpPut("{gameId}/rounds/{roundId}")]
        public async Task<IActionResult> SubmitAction(
            Guid gameId,
            Guid roundId,
            [FromHeader] string? password,
            [FromHeader] string player,
            [FromBody] ActionRequest actionRequest)
        {
            try
            {
                var game = await _gameService.GetGameById(gameId);
                if (game == null)
                {
                    Response.Headers.Add("X-msg", "The specified resource was not found");
                    return NotFound(new { status = 404, msg = "The specified resource was not found" });
                }

                if (game.HasPassword && !string.IsNullOrEmpty(game.Password) && game.Password != password)
                {
                    Response.Headers.Add("X-msg", "Invalid credentials");
                    return Unauthorized(new { status = 401, msg = "Invalid credentials" });
                }

                if (!game.Players.Any(p => p.PlayerName == player))
                {
                    Response.Headers.Add("X-msg", "Not part of the game");
                    return StatusCode(403, new { status = 403, msg = "Not part of the game" });
                }

                var group = await _groupRepository.GetGroupsByRoundId(roundId);
                if (!group.Any(g => g.PlayerName == player))
                {
                    Response.Headers.Add("X-msg", "Player is not part of the current group");
                    return BadRequest(new { status = 400, msg = "Player is not part of the current group" });
                }

                if (!game.Enemies.Any(e => e.EnemyName == player) && !actionRequest.Action)
                {
                    Response.Headers.Add("X-msg", "Only enemies can sabotage");
                    return BadRequest(new { status = 400, msg = "Only enemies can sabotage" });
                }

                var round = await _gameService.GetRoundById(gameId, roundId);
                if (round == null)
                {
                    Response.Headers.Add("X-msg", "Round not found");
                    return NotFound(new { status = 404, msg = "Round not found" });
                }

                if (round.Status != "waiting-on-leader")
                {
                    Response.Headers.Add("X-msg", "This action is not allowed at this time");
                    return StatusCode(428, new { status = 428, msg = "This action is not allowed at this time" });
                }

                await _gameService.SubmitAction(gameId, roundId, player, actionRequest.Action);

                var allActions = await _gameService.GetActionsByRoundId(roundId);
                var groupMembers = await _groupRepository.GetGroupsByRoundId(roundId);

                if (allActions.Count() == groupMembers.Count())
                {
                    var result = await _gameService.CheckRoundOutcome(game, roundId, allActions);
                    var response = new
                    {
                        status = 200,
                        msg = "Action registered",  
                        data = new
                        {
                            id = round.Id,
                            status = round.Status,
                            phase = round.Phase,
                            result = result.RoundResult,
                            leader = round.Leader,
                            createdAt = round.CreatedAt,
                            updatedAt = DateTime.Now,
                            group = groupMembers.Select(g => g.PlayerName).ToList(),
                            votes = allActions.Select(a => a.ActionValue).ToList()
                        },
                        others = new { }
                    };

                    Response.Headers.Add("X-msg", "Action submitted successfully");
                    return Ok(response);
                }

                Response.Headers.Add("X-msg", "Action submitted successfully");
                return Ok(new { status = 200, msg = "Action submitted successfully" });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-msg", "Client error occurred");
                return BadRequest(new { status = 400, msg = "Client error occurred", detail = ex.Message });
            }
        }
    }
}
