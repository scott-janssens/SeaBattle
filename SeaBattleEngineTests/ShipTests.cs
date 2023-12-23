using NUnit.Framework.Internal;
using SeaBattleEngine;
using System.Diagnostics.CodeAnalysis;

namespace SeaBattleEngineTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class ShipTests
{
    [Test]
    public void Ship_SetLocation_ValidLocation()
    {
        // Arrange
        var ship = new Carrier();

        // Act
        ship.SetLocation(1, 2, Orientation.Horizontal, false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ship.Row, Is.EqualTo(1));
            Assert.That(ship.Column, Is.EqualTo(2));
            Assert.That(ship.Orientation, Is.EqualTo(Orientation.Horizontal));
            Assert.That(ship.IsValid, Is.True);
        });
    }

    [Test]
    public void Ship_SetLocation_InvalidLocationVertical()
    {
        // Arrange
        var ship = new Battleship();

        // Act
        ship.SetLocation(9, 8, Orientation.Vertical, false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ship.Row, Is.EqualTo(9));
            Assert.That(ship.Column, Is.EqualTo(8));
            Assert.That(ship.Orientation, Is.EqualTo(Orientation.Vertical));
            Assert.That(ship.IsValid, Is.False);
        });
    }

    [Test]
    public void Ship_SetLocation_InvalidLocationHorizontal()
    {
        // Arrange
        var ship = new Destroyer();

        // Act
        ship.SetLocation(9, 9, Orientation.Horizontal, false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ship.Row, Is.EqualTo(9));
            Assert.That(ship.Column, Is.EqualTo(9));
            Assert.That(ship.Orientation, Is.EqualTo(Orientation.Horizontal));
            Assert.That(ship.IsValid, Is.False);
        });
    }

    [Test]
    public void Ship_SetLocation_ForceInvalid()
    {
        // Arrange
        var ship = new Carrier();

        // Act
        ship.SetLocation(1, 2, Orientation.Horizontal, true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ship.Row, Is.EqualTo(1));
            Assert.That(ship.Column, Is.EqualTo(2));
            Assert.That(ship.Orientation, Is.EqualTo(Orientation.Horizontal));
            Assert.That(ship.IsValid, Is.False);
        });
    }


    [Test]
    public void Ship_Hit_UpdateZonesHit()
    {
        // Arrange
        var ship = new Cruiser();
        ship.SetLocation(1, 2, Orientation.Horizontal, false);

        // Act
        ship.Hit(1, 2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ship.ZonesHit, Contains.Item(true));
            Assert.That(ship.IsSeaworthy, Is.True);
        });
    }

    [Test]
    public void Ship_MarkSunk_SetAllZonesHit()
    {
        // Arrange
        var ship = new Submarine();

        // Act
        ship.MarkSunk();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ship.ZonesHit, Has.All.True);
            Assert.That(ship.IsSeaworthy, Is.False);
        });
    }

    [Test]
    public void Ship_ShipInfoCast()
    {
        // Arrange
        var ship = new Carrier();

        // Act
        ship.SetLocation(3, 5, Orientation.Vertical, false);
        ShipInfo actual = ship;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual.Row, Is.EqualTo(ship.Row));
            Assert.That(actual.Column, Is.EqualTo(ship.Column));
            Assert.That(actual.Orientation, Is.EqualTo(ship.Orientation));
            Assert.That(actual.Name, Is.EqualTo(ship.Name));
            Assert.That(actual.Size, Is.EqualTo(ship.Size));
        });
    }
}
