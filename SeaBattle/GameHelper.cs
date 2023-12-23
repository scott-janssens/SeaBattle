using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SeaBattle.Components;
using SeaBattleEngine;

namespace SeaBattle;

public class GameHelper
{
    private readonly bool _isSinglePlayer;
    private SeaBattleEngine.Ship? _setupShip;

    public Game Game { get; private set; }

    public required NavigationManager Navigation { get; init; }
    public required Func<Task> StateHasChangedAsync { get; init; }

    public ElementReference GameContainer { get; set; }
    public PlayerMap PlayerMapComponent { get; set; } = default!;
    public PlayerMap OpponentMapComponent { get; set; } = default!;

    public Func<Task>? OnSetupCompletedAsync { get; init; }
    public Func<int, int, Task>? OnPlayerGuessAsync { get; init; }
    public Func<string, int, int, TurnResult, Task>? OnOpponentGuessAsync { get; init; }
    public Func<string, Task>? OnTurnEnd { get; init; }
    public Action? OnRestartGameInternal { get; init; }

    public string Instructions { get; set; }

    public GameHelper(int playerCount, string name, string id)
    {
        Guard.IsInRange(playerCount, 1, 3, "playerCount");
        Guard.IsNotNullOrWhiteSpace(name);
        Guard.IsNotNullOrWhiteSpace(id);

        _isSinglePlayer = playerCount == 1;

        Game = _isSinglePlayer
            ? new Game(new Player(name, id), new CpuPlayer())
            : new Game(new Player(name, id));

        Instructions = string.Empty;
    }

    public async Task<bool> SetupNextShipAsync()
    {
        Guard.IsNotNull(Game);
        var isCompleted = true;

        if (Game.Mode == GameMode.Setup)
        {
            if (Game.Player.Map.UnplacedShips.IsEmpty)
            {
                Game.CompleteSetup();

                if (_isSinglePlayer)
                {
                    ((CpuPlayer)Game.Opponent).SetupShips();
                    await StartGame();
                }
                else
                {
                    await OnSetupCompletedAsync!();
                }
            }
            else
            {
                _setupShip = Game.Player.Map.UnplacedShips.First();
                Instructions = $"Use the ARROW keys or LEFT CLICK to place your {_setupShip.Name}, SPACE to rotate, and ENTER to lock in coordinates.";
                Game.Player.Map.PlaceShip(0, 0, Orientation.Horizontal, _setupShip);
                PlayerMapComponent.PlaceShip(_setupShip);
                isCompleted = false;
                await StateHasChangedAsync();
            }
        }

        return isCompleted;
    }

    public async Task StartGame(string? firstTurnId = null)
    {
        var turnId = firstTurnId ?? ((new Random((int)DateTime.Now.Ticks).Next() & 1) == 1 ? Game.Player.Id : Game.Opponent!.Id);

        Game.StartGame(turnId);
        await SwitchTurnMsgAsync();
    }

    public async Task KeyDownHandler(KeyboardEventArgs e)
    {
        if (Game.Mode == GameMode.Setup)
        {
            var key = e.Key;

            if (_setupShip != null)
            {
                switch (key)
                {
                    case "ArrowUp":
                        if (_setupShip.Row > 0)
                        {
                            Game.Player.Map.PlaceShip((char)(_setupShip.Row - 1), _setupShip.Column, _setupShip.Orientation, _setupShip);
                        }
                        break;
                    case "ArrowDown":
                        if (_setupShip.Row < 9)
                        {
                            Game.Player.Map.PlaceShip((char)(_setupShip.Row + 1), _setupShip.Column, _setupShip.Orientation, _setupShip);
                        }
                        break;
                    case "ArrowLeft":
                        if (_setupShip.Column > 0)
                        {
                            Game.Player.Map.PlaceShip(_setupShip.Row, _setupShip.Column - 1, _setupShip.Orientation, _setupShip);
                        }
                        break;
                    case "ArrowRight":
                        if (_setupShip.Column < 9)
                        {
                            Game.Player.Map.PlaceShip(_setupShip.Row, _setupShip.Column + 1, _setupShip.Orientation, _setupShip);
                        }
                        break;
                    case " ":
                        Game.Player.Map.PlaceShip(_setupShip.Row, _setupShip.Column, _setupShip.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal, _setupShip);
                        break;
                    case "Enter":
                        if (_setupShip.IsValid)
                        {
                            Game.Player.Map.LockShip();
                            _setupShip = null;
                            PlayerMapComponent.LockShip();
                            await SetupNextShipAsync();
                        }
                        break;
                }
            }
        }
    }

    public Task PlayerMapMouseUpHandler(MouseEventArgs e)
    {
        if (_setupShip != null && Game.Mode == GameMode.Setup)
        {
            var row = ((int)(e.OffsetY - 50)) / 50;
            var column = ((int)(e.OffsetX - 50)) / 50;

            Game!.Player.Map.PlaceShip(row, column, _setupShip.Orientation, _setupShip);
        }

        return Task.CompletedTask;
    }

    public async Task OpponentMapMouseUpHandler(MouseEventArgs e)
    {
        if (Game?.Mode == GameMode.Play && Game.Turn == Game.Player)
        {
            var row = ((int)(e.OffsetY - 50)) / 50;
            var column = ((int)(e.OffsetX - 50)) / 50;

            if (Game.Guesses[row, column] == CellState.None)
            {
                if (_isSinglePlayer)
                {
                    var result = Game.GetOpponentGuessResult(Game.Player.Id, row, column);
                    await TurnResultMsgAsync(Game.Player.Id, row, column, result);
                }
                else
                {
                    await OnPlayerGuessAsync!(row, column);
                }
            }
        }
    }

    public async Task TurnResultMsgAsync(string guesserId, int row, int column, TurnResult result)
    {
        Guard.IsEqualTo(guesserId, Game.Player.Id, nameof(guesserId));

        Game.ApplyPlayerGuessResult(row, column, result);

        if (result.ShipSunk != null)
        {
            var ship = Game.Opponent.Map.GetShipByName(result.ShipSunk.Name);
            ship.MarkSunk();
            
            if (!_isSinglePlayer)
            {
                Game.Opponent.Map.PlaceShip(result.ShipSunk.Row, result.ShipSunk.Column, result.ShipSunk.Orientation, ship);
            }
            
            OpponentMapComponent.PlaceShip(ship);
        }

        if (result.IsGameOver)
        {
            Instructions = $"{Game.GetPlayerById(result.WinnerId!).Name} Wins! ";
        }
        else
        {
            if (_isSinglePlayer)
            {
                await SwitchTurnMsgAsync();
            }
            else
            {
                await OnTurnEnd!(Game.Turn.Id);
            }
        }

        await StateHasChangedAsync();
    }

    public async Task OpponentGuessMsgAsync(string guesserId, int row, int column)
    {
        Guard.IsEqualTo(guesserId, Game.Opponent.Id, nameof(guesserId));

        var result = Game.GetOpponentGuessResult(guesserId, row, column);

        if (_isSinglePlayer)
        {
            ((CpuPlayer)Game.Opponent).ApplyGuessResult(row, column, result);
        }
        else
        {
            await OnOpponentGuessAsync!(guesserId, row, column, result);
        }

        if (result.IsGameOver)
        {
            Instructions = $"{Game.GetPlayerById(result.WinnerId!).Name} Wins! ";
        }
        else if (_isSinglePlayer)
        {
            await SwitchTurnMsgAsync();
        }
    }

    public async Task SwitchTurnMsgAsync()
    {
        if (Game.Mode != GameMode.End)
        {
            Instructions = $"Turn {Game.TurnCount}: {Game.Turn.Name}";

            if (_isSinglePlayer && Game.Turn == Game.Opponent)
            {
                var (Row, Column) = ((CpuPlayer)Game.Opponent).CpuGuessInternal();
                await OpponentGuessMsgAsync(Game.Opponent.Id, Row, Column);
            }
        }

        await StateHasChangedAsync();
    }

    public async Task RestartHandler(bool restartRequested)
    {
        if (restartRequested)
        {
            await RestartGameInternalAsync();
        }
        else
        {
            Navigation.NavigateTo($"Index/{Game.Player.Name}");
        }
    }

    public async Task RestartGameInternalAsync()
    {
        PlayerMapComponent.Reset();
        OpponentMapComponent.Reset();

        if (_isSinglePlayer)
        {
            Game = new Game(new Player(Game.Player.Name, Game.Player.Id), new CpuPlayer());
            ((CpuPlayer)Game.Opponent).SetupShips();
        }
        else
        {
            Game = new Game(new Player(Game.Player.Name, Game.Player.Id), new Player(Game.Opponent.Name, Game.Opponent.Id));
            OnRestartGameInternal!();
        }

        await GameContainer.FocusAsync();
        await SetupNextShipAsync();
    }
}
