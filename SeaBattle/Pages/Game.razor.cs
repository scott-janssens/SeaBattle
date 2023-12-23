using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using SeaBattle.Components;
using SeaBattle.Hubs;
using SeaBattleEngine;
using Serilog;

namespace SeaBattle.Pages;

public partial class Game
{
    private GameHelper _gameHelper = default!;
    private ElementReference gameContainer;
    private PlayerMap playerMapComponent = default!;
    private PlayerMap opponentMapComponent = default!;
    private string circuitId = string.Empty;

    private bool isInitialized = false;

    private int intializers = 0;

    private string joinUrl = string.Empty;
    private bool isJoining = false;
    private bool isOpponentSetupComplete = false;

    private HubConnection? hubConnection;
    private readonly List<MessageDto> messages = [];
    private string? messageInput;

    private bool? restart;
    private bool? opponentRestart;
    private bool urlCopied;

    private bool RestartDeclined => (opponentRestart.HasValue && !opponentRestart.Value) || (restart.HasValue && !restart.Value);

    [Parameter]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public string? GameId { get; set; }

    protected override void OnInitialized()
    {
        (circuitHandler as CircuitHandlerService)!.ConnectionDown += (o, e) =>
        {
            if (e.CircuitId == circuitId)
            {
                InvokeAsync(() => DisposeAsync().AsTask());
            }
        };

        Navigation.LocationChanged += OnNavigating;

        isJoining = Navigation.Uri.EndsWith("join");
        joinUrl = $"{Navigation.BaseUri}Index/join/{GameId}";

        ConnectToSignalR();
    }

    private void OnNavigating(object? sender, LocationChangedEventArgs e)
    {
        Navigation.LocationChanged -= OnNavigating;
        InvokeAsync(() => DisposeAsync().AsTask());
    }

    protected override async Task OnParametersSetAsync()
    {
        Guard.IsNotNull(GameId);

        _gameHelper = new(2, Name, hubConnection!.ConnectionId!)
        {
            Navigation = Navigation,
            StateHasChangedAsync = async () => await InvokeAsync(StateHasChanged),
            OnSetupCompletedAsync = OnSetupCompletedAsync,
            OnPlayerGuessAsync = OnPlayerGuessAsync,
            OnTurnEnd = OnTurnEnd,
            OnRestartGameInternal = OnRestartGameInternal,
            OnOpponentGuessAsync = OnOpponentGuessAsync
        };

        circuitId = (await ProtectedLocalStorage.GetAsync<string>("circuitId")).Value!;

        if (isJoining)
        {
            await hubConnection!.SendAsync("JoinGame", GameId, Name, circuitId);
        }
        else
        {
            await hubConnection!.SendAsync("RegisterGame", GameId, Name, circuitId);

            intializers++;
        }

        base.OnParametersSet();

        await StartSetup();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await gameContainer.FocusAsync();

            intializers++;

            await StartSetup();
        }
    }

    public async Task JoinMsgAsync(GameInfo gameInfo)
    {
        _gameHelper.GameContainer = gameContainer;

        if (isJoining)
        {
            _gameHelper.Game.Opponent = new Player(gameInfo.Player1.Name, gameInfo.Player1.Id);

            intializers++;
            await StartSetup();
        }
        else
        {
            _gameHelper.Game.Opponent = new Player(gameInfo.Player2!.Name, gameInfo.Player2.Id);
            await hubConnection!.SendAsync("SendMessage", GameId, null, $"{gameInfo!.Player2!.Name} has joined.");

            if (_gameHelper.Game.Mode == GameMode.Waiting)
            {
                await hubConnection!.SendAsync("SetupComplete", GameId, _gameHelper.Game.Player.Id);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task StartSetup()
    {
        if (intializers >= 2)
        {
            isInitialized = true;

            // needed to populate playerMapComponent field
            await InvokeAsync(StateHasChanged);

            _gameHelper.PlayerMapComponent = playerMapComponent;
            _gameHelper.OpponentMapComponent = opponentMapComponent;

            await _gameHelper.SetupNextShipAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnSetupCompletedAsync()
    {
        var opponentName = _gameHelper.Game.Opponent?.Name ?? "opponent";
        _gameHelper.Instructions = $"Waiting for {opponentName} to finish setting up.";

        await hubConnection!.SendAsync("SetupComplete", GameId, _gameHelper.Game.Player.Id);
    }

    private async Task SetupCompleteMsgAsync(string playerId)
    {
        if (_gameHelper.Game.Player.Id == playerId)
        {
            if (isOpponentSetupComplete)
            {
                await _gameHelper.StartGame();
                await hubConnection!.SendAsync("StartGame", GameId, _gameHelper.Game.Turn.Id);
                await InvokeAsync(StateHasChanged);
            }
        }
        else
        {
            if (hubConnection != null)
            {
                await hubConnection.SendAsync("SendMessage", GameId, null, $"{_gameHelper.Game.Opponent!.Name} has finished setting up.");
            }

            isOpponentSetupComplete = true;
        }
    }

    private async Task StartGameMsgAsync(string firstTurnId)
    {
        await _gameHelper.StartGame(firstTurnId);
    }

    private async Task OnPlayerGuessAsync(int row, int column)
    {
        await hubConnection!.SendAsync("Guess", GameId, _gameHelper.Game.Player.Id, row, column);
    }

    private async Task OnOpponentGuessAsync(string guesserId, int row, int column, TurnResult result)
    {
        await hubConnection!.SendAsync("TurnResult", GameId, guesserId, row, column, result);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OpponentGuessMsgAsync(string guesserId, int row, int column)
    {
        await _gameHelper.OpponentGuessMsgAsync(guesserId, row, column);
        await InvokeAsync(StateHasChanged);
    }

    private async Task TurnResultMsgAsync(string guesserId, int row, int column, TurnResult result)
    {
        await _gameHelper.TurnResultMsgAsync(guesserId, row, column, result);

        if (result.ShipSunk != null)
        {
            await hubConnection!.SendAsync("SendMessage", GameId, null, $"{_gameHelper.Game.Player.Name} sunk {result.ShipSunk.Name}!");
        }

        if (result.IsGameOver)
        {
            await hubConnection!.SendAsync("WinnerShips", GameId, playerMapComponent.Map.GetShipInfo());
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnTurnEnd(string turnId)
    {
        await hubConnection!.SendAsync("SwitchTurn", GameId, turnId);
    }

    private async Task SwitchTurnMsgAsync(string newTurnPlayerId)
    {
        await _gameHelper.SwitchTurnMsgAsync(); // TODO: check this after everything is settled, may be pointless
        await InvokeAsync(StateHasChanged);
    }

    private void WinnerShipsMsgAsync(ShipInfo[] ships)
    {
        opponentMapComponent.ShowAll(ships);
    }

    private async Task RestartHandler(bool restartRequested)
    {
        restart = restartRequested;
        await hubConnection!.SendAsync("RestartRequest", GameId, restart.Value);

        await CheckRestart();
    }

    private async Task RestartGameMsgAsync(string playerId, bool restartRequested)
    {
        if (playerId == _gameHelper.Game.Opponent.Id)
        {
            opponentRestart = restartRequested;
        }

        await CheckRestart();
    }

    private async Task CheckRestart()
    {
        if (RestartDeclined)
        {
            restart = opponentRestart = false;
            _gameHelper.Instructions = "Rematch declined. Thank you for playing. ";
            await InvokeAsync(StateHasChanged);
        }
        else if (opponentRestart.HasValue && opponentRestart.Value && restart.HasValue && restart.Value)
        {
            await _gameHelper.RestartGameInternalAsync();
        }
    }

    private void OnRestartGameInternal()
    {
        isOpponentSetupComplete = false;
        restart = opponentRestart = null;
    }

    private async Task CopyTextToClipboard()
    {
        await jsRuntime.InvokeVoidAsync("clipboardCopy.copyText", joinUrl);
        urlCopied = true;
    }

    #region Chat

    private void ConnectToSignalR()
    {
        hubConnection = new HubConnectionBuilder()
        .WithUrl(Navigation.ToAbsoluteUri("gamehub"))
        .ConfigureLogging(x => x.AddDebug())
        .Build();

        hubConnection.On<GameInfo>("JoinNotification", JoinMsgAsync);
        hubConnection.On<string>("SetupComplete", SetupCompleteMsgAsync);
        hubConnection.On<string>("StartGame", StartGameMsgAsync);
        hubConnection.On<string, int, int>("Guess", OpponentGuessMsgAsync);
        hubConnection.On<string, int, int, TurnResult>("TurnResult", TurnResultMsgAsync);
        hubConnection.On<string>("SwitchTurn", SwitchTurnMsgAsync);
        hubConnection.On<string, string>("ReceiveMessage", ReceiveMessageAsync);
        hubConnection.On<ShipInfo[]>("WinnerShips", WinnerShipsMsgAsync);
        hubConnection.On<string, bool>("RestartGame", RestartGameMsgAsync);
        hubConnection.On<string, string>("Disconnect", DisconnectMsgAsync);

        var timestamp = DateTime.Now.Millisecond;

        Log.Logger.Information("Connecting to SignalR...");
        while (hubConnection.State == HubConnectionState.Disconnected && DateTime.Now.Millisecond - timestamp < 60000)
        {
            try
            {
                hubConnection.StartAsync().Wait();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error connecting to SignalR");

                // Failed to connect, trying again in 2s.
                Task.Delay(2000).Wait();
            }
        }

        Log.Logger.Information("SignalR State = {state}", hubConnection.State);

        if (hubConnection.State == HubConnectionState.Disconnected)
        {
            ThrowHelper.ThrowTimeoutException("Could not connect to SignalR.");
        }
    }

    private async Task DisconnectMsgAsync(string id, string message)
    {
        message ??= "No message";
        await ReceiveMessageAsync(null, $"{_gameHelper.Game.GetPlayerById(id).Name} Disconnected. [{message}]");

        if (!RestartDeclined)
        {
            _gameHelper.Game.StopGame();
            _gameHelper.Instructions = $"Connection with {_gameHelper.Game.Opponent.Name} lost.";
            opponentRestart = false;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task ChatHandler(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await gameContainer.FocusAsync();
            await SendAsync();
            messageInput = null;
        }
    }

    private async Task SendAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendMessage", GameId, _gameHelper.Game.Player.Name, messageInput);
        }
    }

    private async Task ReceiveMessageAsync(string? user, string message)
    {
        var mesage = new MessageDto
        {
            IsPlayer = user == _gameHelper.Game.Player.Name,
            Name = user,
            Content = message
        };

        messages.Add(mesage);
        await InvokeAsync(StateHasChanged);
    }

    public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.StopAsync();
            await hubConnection.DisposeAsync();
            hubConnection = null;
        }
    }

    private class MessageDto
    {
        public bool IsPlayer { get; set; }

        public string? Name { get; set; }

        public required string Content { get; set; }
    }

    #endregion
}
