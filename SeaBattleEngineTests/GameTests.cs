using Moq;
using SeaBattleEngine;
using System.Diagnostics.CodeAnalysis;

namespace SeaBattleEngineTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class GameTests
{
    [Test]
    public void Constructor_WithSinglePlayer_SetsModeToSetup()
    {
        // Arrange
        var player = new Player("Player1", "Id");

        // Act
        var game = new Game(player);

        // Assert
        Assert.That(game.Mode, Is.EqualTo(GameMode.Setup));
    }

    [Test]
    public void Constructor_WithTwoPlayers_SetsModeToSetup()
    {
        // Arrange
        var player1 = new Player("Player1", "Id1");
        var player2 = new Player("Player2", "Id2");

        // Act
        var game = new Game(player1, player2);

        // Assert
        Assert.That(game.Mode, Is.EqualTo(GameMode.Setup));
    }

    [Test]
    public void CompleteSetup_SetsModeToWaiting()
    {
        // Arrange
        var player = new Player("Player1", "Id");
        var game = new Game(player);

        // Act
        game.CompleteSetup();

        // Assert
        Assert.That(game.Mode, Is.EqualTo(GameMode.Waiting));
    }

    [Test]
    public void StartGame_SetsModeToPlay()
    {
        // Arrange
        var player = new Player("Player1", "Id");
        var game = new Game(player);

        // Act
        game.StartGame(player.Id);

        // Assert
        Assert.That(game.Mode, Is.EqualTo(GameMode.Play));
    }

    [Test]
    public void OpponentGuess_WhenGameIsNotStarted_ThrowsException()
    {
        // Arrange
        var player1 = new Player("Player1", "Id");
        var game = new Game(player1);

        // Act & Assert
        Assert.That(() => game.GetOpponentGuessResult(player1.Id, 0, 0), Throws.Exception.TypeOf<ArgumentNullException>());
        // Add specific exception type based on your implementation
    }

    [Test]
    public void OpponentGuess_WhenGameIsStartedAndOpponentMapsCell_ReturnsValidTurnResult()
    {
        // Arrange
        var player1 = new Player("Player1", "Id1");
        var player2 = new Mock<IPlayer>();
        var player2Map = new Mock<IMap>();

        player2Map.Setup(m => m.Guess(It.IsAny<int>(), It.IsAny<int>())).Returns(new TurnResult { Result = GuessResult.Miss });
        player2.Setup(x => x.Map).Returns(player2Map.Object);
        player2.Setup(x => x.Id).Returns("player2Id");

        var game = new Game(player1, player2.Object);
        game.CompleteSetup();
        game.StartGame(player2.Object.Id);

        // Act
        var result = game.GetOpponentGuessResult(player2.Object.Id, 0, 0);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(GuessResult.Miss));
            Assert.That(game.Mode, Is.EqualTo(GameMode.Play));
        });
    }

    [Test]
    public void OpponentGuess_WhenGameIsStartedAndOpponentMapsCell_GameOver()
    {
        // Arrange
        var player1 = new Player("Player1", "Id1");
        var player2 = new Mock<IPlayer>();
        var player2Map = new Mock<IMap>();

        player2Map.Setup(m => m.Guess(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new TurnResult { Result = GuessResult.Sunk, IsGameOver = true });
        player2.Setup(x => x.Map).Returns(player2Map.Object);
        player2.Setup(x => x.Id).Returns("player2Id");

        var game = new Game(player1, player2.Object);
        game.CompleteSetup();
        game.StartGame(player1.Id);

        // Act
        var result = game.GetOpponentGuessResult(player1.Id, 0, 0);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(GuessResult.Sunk));
            Assert.That(game.Mode, Is.EqualTo(GameMode.End));
            Assert.That(result.WinnerId, Is.EqualTo(player1.Id));
        });
    }

    [Test]
    public void SinglePlayerGuess_WhenGameIsStartedAndPlayerMapsCell_ReturnsValidTurnResult()
    {
        // Arrange
        var player1 = new Player("Player1", "Id1");
        var player2 = new CpuPlayer();
        var game = new Game(player1, player2);
        game.CompleteSetup();
        game.StartGame(player1.Id);

        // Act
        var result = game.GetOpponentGuessResult(player1.Id, 0, 0);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(GuessResult.Miss));
            Assert.That(game.Mode, Is.EqualTo(GameMode.Play));
        });
    }

    [Test]
    public void Guess_WhenGameIsInProgressAndResultIsGameOver_SetsModeToEnd()
    {
        // Arrange
        var player1 = new Player("Player1", "Id1");
        var player2 = new Player("Player2", "Id2");
        var game = new Game(player1, player2);
        game.CompleteSetup();
        game.StartGame(player1.Id);
        var turnResult = new TurnResult { Result = GuessResult.Miss, IsGameOver = true };

        // Act
        game.ApplyPlayerGuessResult(0, 0, turnResult);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(game.Mode, Is.EqualTo(GameMode.End));
            Assert.That(game.Turn, Is.EqualTo(player2)); // Assuming player2 is the winner in this scenario
        });
    }

    [Test]
    public void Guess_WhenGameIsInProgressAndResultIsNotGameOver_UpdatesGuessesAndTurn()
    {
        // Arrange
        var player1 = new Player("Player1", "Id1");
        var player2 = new Player("Player2", "Id2");
        var game = new Game(player1, player2);
        game.CompleteSetup();
        game.StartGame(player1.Id);
        var turnResult = new TurnResult { IsGameOver = false, Result = GuessResult.Hit };

        // Act
        game.ApplyPlayerGuessResult(0, 0, turnResult);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(game.Mode, Is.EqualTo(GameMode.Play));
            Assert.That(game.Guesses[0, 0], Is.EqualTo(CellState.Hit));
            Assert.That(game.Turn, Is.EqualTo(player2));
        });
    }
}
