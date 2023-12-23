using SeaBattleEngine;
using System.Diagnostics.CodeAnalysis;

namespace SeaBattleEngineTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class MapTests
{
    [Test]
    public void UnplacedShips_ShouldContainAllShipsInitially()
    {
        // Arrange
        var map = new Map();

        // Act

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(map.UnplacedShips, Has.Member(map.Carrier));
            Assert.That(map.UnplacedShips, Has.Member(map.Battleship));
            Assert.That(map.UnplacedShips, Has.Member(map.Cruiser));
            Assert.That(map.UnplacedShips, Has.Member(map.Submarine));
            Assert.That(map.UnplacedShips, Has.Member(map.Destroyer));
        });
    }

    [Test]
    public void GetShipByName_ShouldReturnCorrectShip()
    {
        // Arrange
        var map = new Map();

        // Act
        var carrier = map.GetShipByName("Carrier");
        var battleship = map.GetShipByName("Battleship");
        var cruiser = map.GetShipByName("Cruiser");
        var submarine = map.GetShipByName("Submarine");
        var destroyer = map.GetShipByName("Destroyer");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(carrier, Is.EqualTo(map.Carrier));
            Assert.That(battleship, Is.EqualTo(map.Battleship));
            Assert.That(cruiser, Is.EqualTo(map.Cruiser));
            Assert.That(submarine, Is.EqualTo(map.Submarine));
            Assert.That(destroyer, Is.EqualTo(map.Destroyer));
        });
    }

    [Test]
    public void PlaceShip_ShouldPlaceShipInValidLocation()
    {
        // Arrange
        var map = new Map();
        var ship = map.Carrier;

        // Act
        map.PlaceShip(0, 0, Orientation.Vertical, ship);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => map.LockShip());
            Assert.That(ship.Row, Is.EqualTo(0));
            Assert.That(ship.Column, Is.EqualTo(0));
            Assert.That(ship.Orientation, Is.EqualTo(Orientation.Vertical));
        });
    }

    [Test]
    public void PlaceShip_WrongShip()
    {
        // Arrange
        var map = new Map();
        var ship = new Carrier();

        // Assert
        Assert.Throws<ArgumentException>(() => map.PlaceShip(0, 0, Orientation.Horizontal, ship));
    }

    [Test]
    public void PlaceShip_HorizontalOverlap()
    {
        // Arrange
        var map = new Map();
        var carrier = map.Carrier;
        var battleship = map.Battleship;
        map.PlaceShip(0, 0, Orientation.Vertical, carrier);
        map.LockShip();

        // Act
        map.PlaceShip(0, 0, Orientation.Horizontal, battleship);

        // Assert
        Assert.Throws<ArgumentNullException>(map.LockShip);
    }

    [Test]
    public void PlaceShip_VerticalOverlap()
    {
        // Arrange
        var map = new Map();
        var carrier = map.Carrier;
        var battleship = map.Battleship;
        map.PlaceShip(0, 0, Orientation.Horizontal, carrier);
        map.LockShip();

        // Act
        map.PlaceShip(0, 0, Orientation.Vertical, battleship);

        // Assert
        Assert.Throws<ArgumentNullException>(map.LockShip);
    }

    [Test]
    public void PlaceShip_HorizontalOffEdge()
    {
        // Arrange
        var map = new Map();
        var carrier = map.Carrier;

        // Act
        map.PlaceShip(8, 8, Orientation.Horizontal, carrier);

        // Assert
        Assert.Throws<ArgumentNullException>(map.LockShip);
    }

    [Test]
    public void PlaceShip_VerticalOffEdge()
    {
        // Arrange
        var map = new Map();
        var carrier = map.Carrier;

        // Act
        map.PlaceShip(8, 8, Orientation.Vertical, carrier);

        // Assert
        Assert.Throws<ArgumentNullException>(map.LockShip);
    }

    [Test]
    public void Map_Guess_Miss()
    {
        // Arrange
        var map = new Map();
        var ship = map.Destroyer;
        map.PlaceShip(0, 0, Orientation.Horizontal, ship);
        map.LockShip();

        // Act
        var result = map.Guess(5, 5);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(GuessResult.Miss));
            Assert.That(result.ShipSunk, Is.Null);
            Assert.That(result.Orientation, Is.Null);
            Assert.That(result.IsGameOver, Is.False);
        });
    }

    [Test]
    public void Map_Guess_Hit()
    {
        // Arrange
        var map = new Map();
        var ship = map.Destroyer;
        map.PlaceShip(0, 0, Orientation.Horizontal, ship);
        map.LockShip();

        // Act
        var result = map.Guess(0, 0);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(GuessResult.Hit));
            Assert.That(result.ShipSunk, Is.Null);
            Assert.That(result.Orientation, Is.Null);
            Assert.That(result.IsGameOver, Is.False);
        });
    }

    [Test]
    public void Map_Guess_Sink()
    {
        // Arrange
        var map = new Map();
        var ship = map.Destroyer;
        map.PlaceShip(0, 0, Orientation.Horizontal, ship);
        map.LockShip();

        // Act
        map.Guess(0, 0);
        var result = map.Guess(0, 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(GuessResult.Sunk));
            Assert.That(result.ShipSunk!.Name, Is.EqualTo(ship.Name));
            Assert.That(result.Orientation, Is.EqualTo(ship.Orientation));
            Assert.That(result.IsGameOver, Is.True);
        });
    }
}
