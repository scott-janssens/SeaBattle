using CommunityToolkit.Diagnostics;

namespace SeaBattleEngine;

public class Game
{
    private string _firstTurnPlayerId = string.Empty;

    public IPlayer Player { get; set; }
    public IPlayer Opponent { get; set; }
    public CellGrid Guesses => (Map)Opponent.Map;

    public GameMode Mode { get; private set; }
    public IPlayer Turn { get; private set; } = default!;
    public int TurnCount { get; private set; }

    public Game(IPlayer player1)
    {
        Mode = GameMode.Setup;
        Player = player1;
        Opponent = new Player("Opponent", string.Empty);
    }

    public Game(IPlayer player1, IPlayer player2)
    {
        Mode = GameMode.Setup;
        Player = player1;
        Opponent = player2;
    }

    public void CompleteSetup()
    {
        Mode = GameMode.Waiting;
    }

    public void StartGame(string firstTurnPlayerId)
    {
        Guard.IsNotNull(Opponent);

        Mode = GameMode.Play;
        _firstTurnPlayerId = firstTurnPlayerId;
        Turn = firstTurnPlayerId == Player.Id ? Player : Opponent;
        TurnCount = 1;
    }

    public TurnResult GetOpponentGuessResult(string guesserId, int row, int column)
    {
        Guard.IsNotNullOrWhiteSpace(guesserId);
        Guard.IsNotNull(Opponent);
        Guard.IsNotNull(Turn);
        Guard.IsEqualTo(guesserId, Turn.Id);

        var guessee = guesserId == Player.Id ? Opponent : Player;
        var result = guessee.Map.Guess(row, column);

        if (result.IsGameOver)
        {
            Mode = GameMode.End;
            result.WinnerId = guesserId;
        }
        else
        {
            Turn = guessee;

            if (Turn.Id == _firstTurnPlayerId)
            {
                TurnCount++;
            }
        }

        return result;
    }

    public void ApplyPlayerGuessResult(int row, int column, TurnResult turnResult)
    {
        if (turnResult.IsGameOver)
        {
            Mode = GameMode.End;
        }

        Guesses[row, column] = turnResult.Result == GuessResult.Miss ? CellState.Miss : CellState.Hit;
        Turn = Opponent!;

        if (Turn.Id == _firstTurnPlayerId)
        {
            TurnCount++;
        }
    }

    public IPlayer GetPlayerById(string id) => Player.Id == id ? Player : Opponent;

    public void StopGame() => Mode = GameMode.End;
}
