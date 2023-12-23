using Microsoft.AspNetCore.SignalR;
using SeaBattleEngine;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace SeaBattle.Hubs;

//https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-8.0
public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, GameInfo> _games = new();

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var gamePair = _games.Where(x => x.Value.Player1.Id == Context.ConnectionId || x.Value.Player2?.Id == Context.ConnectionId);

        // should only ever be 1, but just in case...
        foreach (var pair in gamePair)
        {
            await Clients.Group(pair.Value.GameId).SendAsync("Disconnect", Context.ConnectionId, exception?.Message);

            while (_games.ContainsKey(pair.Key))
            {
                if (!_games.TryRemove(pair))
                {
                    await Task.Delay(100);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterGame(string gameId, string user, string circuitId)
    {
        if (!_games.ContainsKey(gameId))
        {
            var game = new GameInfo { GameId = gameId, Player1 = new PlayerInfo(user, Context.ConnectionId) { CircuitId = circuitId } };
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            _games.TryAdd(gameId, game);
        }
    }

    public async Task JoinGame(string gameId, string user, string circuitId)
    {
        if (_games.TryGetValue(gameId, out var gameInfo))
        {
            gameInfo.Player2 = new PlayerInfo(user, Context.ConnectionId) { CircuitId = circuitId };
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            await Clients.Group(gameId).SendAsync("JoinNotification", gameInfo);
        }
    }

    public async Task SetupComplete(string gameId, string userId)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.Group(gameId).SendAsync("SetupComplete", userId);
        }
    }

    public async Task StartGame(string gameId, string firstTurnId)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.Group(gameId).SendAsync("StartGame", firstTurnId);
        }
    }

    public async Task Guess(string gameId, string guesserId, int row, int column)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.GroupExcept(gameId, new [] { guesserId }.ToImmutableList() ).SendAsync("Guess", guesserId, row, column);
        }
    }

    public async Task TurnResult(string gameId, string guesserId, int row, int column, TurnResult result)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.Client(guesserId).SendAsync("TurnResult", guesserId, row, column, result);
        }
    }

    public async Task SwitchTurn(string gameId, string newTurnPlayerId)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.Group(gameId).SendAsync("SwitchTurn", newTurnPlayerId);
        }
    }

    public async Task WinnerShips(string gameId, ShipInfo[] ships)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.GroupExcept(gameId, new[] { Context.ConnectionId }.ToImmutableList()).SendAsync("WinnerShips", ships);
        }
    }

    public async Task RestartRequest(string gameId, bool restartRequested)
    {
        await Clients.GroupExcept(gameId, new[] { Context.ConnectionId }.ToImmutableList()).SendAsync("RestartGame", Context.ConnectionId, restartRequested);
    }

    public async Task SendMessage(string gameId, string user, string message)
    {
        if (_games.ContainsKey(gameId))
        {
            await Clients.Group(gameId).SendAsync("ReceiveMessage", user, message);
        }
    }
}

public class GameInfo
{
    public required string GameId { get; set; }

    public required PlayerInfo Player1 { get; set; }

    public PlayerInfo Player2 { get; set; } = default!;
}
