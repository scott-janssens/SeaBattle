using SeaBattleEngine;
using System.Diagnostics.CodeAnalysis;

namespace SeaBattleEngineTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class CellGridTests
{
    [Test]
    public void CellGrid_Initialization_AllCellsAreNotNull()
    {
        // Arrange
        var cellGrid = new CellGrid();

        // Assert
        Assert.That(cellGrid, Is.All.Not.Null);
    }

    [Test]
    public void GetCell_ValidCoordinates_ReturnsCorrectCellState()
    {
        // Arrange
        var cellGrid = new CellGrid();

        // Act
        var cellState = cellGrid.GetCell(1, 1);

        // Assert
        Assert.That(cellState, Is.TypeOf<CellState>());
    }

    [Test]
    public void SetCell_ValidCoordinates_SetsCorrectCellState()
    {
        // Arrange
        var cellGrid = new CellGrid();
        var newState = new CellState();

        // Act
        cellGrid.SetCell(1, 1, newState);

        // Assert
        Assert.That(cellGrid.GetCell(1, 1), Is.EqualTo(newState));
    }

    [Test]
    public void Indexer_ValidCoordinates_ReturnsCorrectCellState()
    {
        // Arrange
        var cellGrid = new CellGrid();

        // Act
        var cellState = cellGrid[1, 1];

        // Assert
        Assert.That(cellState, Is.TypeOf<CellState>());
    }

    [Test]
    public void Indexer_ValidCoordinates_SetsCorrectCellState()
    {
        // Arrange
        var cellGrid = new CellGrid();
        var newState = new CellState();

        // Act
        cellGrid[1, 1] = newState;

        // Assert
        Assert.That(cellGrid.GetCell(1, 1), Is.EqualTo(newState));
    }

    [Test]
    public void GetEnumerator_ReturnsAllCells()
    {
        // Arrange
        var cellGrid = new CellGrid();

        // Act
        var allCells = cellGrid.ToList();

        // Assert
        Assert.That(allCells, Is.All.TypeOf<CellState>());
        Assert.That(allCells, Has.Count.EqualTo(100));
    }

    [Test]
    public void GetCell_InvalidCoordinates()
    {
        // Arrange
        var cellGrid = new CellGrid();

        // Act
        var actual1 = cellGrid.GetCell(-1, 0);
        var actual2 = cellGrid.GetCell(10, 0);
        var actual3 = cellGrid.GetCell(0, -1);
        var actual4 = cellGrid.GetCell(0, 10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual1, Is.Null);
            Assert.That(actual2, Is.Null);
            Assert.That(actual3, Is.Null);
            Assert.That(actual4, Is.Null);
        });
    }

    [Test]
    public void SetCell_InvalidCoordinates()
    {
        // Arrange
        var cellGrid = new CellGrid();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => cellGrid.SetCell(-1, 0, CellState.Miss));
            Assert.Throws<ArgumentOutOfRangeException>(() => cellGrid.SetCell(10, 0, CellState.Miss));
            Assert.Throws<ArgumentOutOfRangeException>(() => cellGrid.SetCell(0, -1, CellState.Miss));
            Assert.Throws<ArgumentOutOfRangeException>(() => cellGrid.SetCell(0, 10, CellState.Miss));
        });
    }
}
