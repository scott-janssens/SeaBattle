using SeaBattleEngine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static SeaBattleEngine.CpuPlayer;

namespace SeaBattleEngineTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class CpuPlayerTests
{
    [Test]
    public void CpuGuess_AlwaysReturnsValidGuess()
    {
        CpuPlayer cpuPlayer = new();

        for (int i = 0; i < 100; i++)
        {
            var (Row, Column) = cpuPlayer.CpuGuess();
            Assert.Multiple(() =>
            {
                Assert.That(Row, Is.InRange(0, 9));
                Assert.That(Column, Is.InRange(0, 9));
            });
        }
    }

    [Test]
    public void CpuGuessInternal_AlwaysReturnsValidGuess()
    {
        CpuPlayer cpuPlayer = new();

        for (int i = 0; i < 100; i++)
        {
            var (Row, Column) = cpuPlayer.CpuGuessInternal();
            Assert.Multiple(() =>
            {
                Assert.That(Row, Is.InRange(0, 9));
                Assert.That(Column, Is.InRange(0, 9));
            });
        }
    }

    [Test]
    public void ApplyGuessResult_Miss()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var row = 3;
        var column = 3;
        var result = new TurnResult { Result = GuessResult.Miss };

        // Act
        cpuPlayer.ApplyGuessResult(row, column, result);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cpuPlayer.CurrentHit, Is.Null);
            Assert.That(cpuPlayer.CurrentHitCount, Is.EqualTo(0));
            Assert.That(cpuPlayer.HitOrientation, Is.Null);
            Assert.That(cpuPlayer.LastDirection, Is.Null);
            Assert.That(cpuPlayer.GetMap()[row, column], Is.EqualTo(CellState.Miss));
        });
    }

    [Test]
    public void ApplyGuessResult_HitHorizontal()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var row = 3;
        var column = 3;
        var result = new TurnResult { Result = GuessResult.Hit };

        // Act
        cpuPlayer.ApplyGuessResult(row, column + 1, result);
        cpuPlayer.ApplyGuessResult(row, column, result);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cpuPlayer.CurrentHit, Is.EqualTo((row, column)));
            Assert.That(cpuPlayer.CurrentHitCount, Is.EqualTo(2));
            Assert.That(cpuPlayer.HitOrientation, Is.EqualTo(Orientation.Horizontal));
            Assert.That(cpuPlayer.GetMap()[row, column], Is.EqualTo(CellState.Hit));
            Assert.That(cpuPlayer.GetMap()[row, column + 1], Is.EqualTo(CellState.Hit));
        });
    }

    [Test]
    public void ApplyGuessResult_HitVertical()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var row = 3;
        var column = 3;
        var result = new TurnResult { Result = GuessResult.Hit };

        // Act
        cpuPlayer.ApplyGuessResult(row + 1, column, result);
        cpuPlayer.ApplyGuessResult(row, column, result);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cpuPlayer.CurrentHit, Is.EqualTo((row, column)));
            Assert.That(cpuPlayer.CurrentHitCount, Is.EqualTo(2));
            Assert.That(cpuPlayer.HitOrientation, Is.EqualTo(Orientation.Vertical));
            Assert.That(cpuPlayer.GetMap()[row, column], Is.EqualTo(CellState.Hit));
            Assert.That(cpuPlayer.GetMap()[row + 1, column], Is.EqualTo(CellState.Hit));
        });
    }

    [Test]
    public void ApplyGuessResult_SunkHorizontal()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var row = 3;
        var column = 3;

        cpuPlayer.Map.PlaceShip(row, column, Orientation.Horizontal, cpuPlayer.Map.Destroyer);
        cpuPlayer.Map.LockShip();

        var hitResult = new TurnResult { Result = GuessResult.Hit };
        var sunkRresult = new TurnResult
        {
            Result = GuessResult.Sunk,
            IsGameOver = false,
            Orientation = Orientation.Horizontal,
            ShipSunk = cpuPlayer.Map.Destroyer
        };

        // Act
        cpuPlayer.ApplyGuessResult(row, column + 1, hitResult);
        cpuPlayer.ApplyGuessResult(row, column, sunkRresult);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cpuPlayer.CurrentHit, Is.Null);
            Assert.That(cpuPlayer.CurrentHitCount, Is.EqualTo(0));
            Assert.That(cpuPlayer.HitOrientation, Is.Null);
            Assert.That(cpuPlayer.LastDirection, Is.Null);
            Assert.That(cpuPlayer.GetMap()[row, column], Is.EqualTo(CellState.Sunk));
            Assert.That(cpuPlayer.GetMap()[row, column + 1], Is.EqualTo(CellState.Sunk));
        });
    }

    [Test]
    public void ApplyGuessResult_SunkVertical()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var row = 3;
        var column = 3;

        cpuPlayer.Map.PlaceShip(row, column, Orientation.Vertical, cpuPlayer.Map.Destroyer);
        cpuPlayer.Map.LockShip();

        var hitResult = new TurnResult { Result = GuessResult.Hit };
        var sunkRresult = new TurnResult
        {
            Result = GuessResult.Sunk,
            IsGameOver = false,
            Orientation = Orientation.Vertical,
            ShipSunk = cpuPlayer.Map.Destroyer
        };

        // Act
        cpuPlayer.ApplyGuessResult(row + 1, column, hitResult);
        cpuPlayer.ApplyGuessResult(row, column, sunkRresult);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cpuPlayer.CurrentHit, Is.Null);
            Assert.That(cpuPlayer.CurrentHitCount, Is.EqualTo(0));
            Assert.That(cpuPlayer.HitOrientation, Is.Null);
            Assert.That(cpuPlayer.LastDirection, Is.Null);
            Assert.That(cpuPlayer.GetMap()[row, column], Is.EqualTo(CellState.Sunk));
            Assert.That(cpuPlayer.GetMap()[row + 1, column], Is.EqualTo(CellState.Sunk));
        });
    }

    [Test]
    public void GetAdjacent()
    {
        TestPlayerHelper.GetAdjacent(3, 3, Direction.Up, out var actualUp);
        TestPlayerHelper.GetAdjacent(3, 3, Direction.Down, out var actualDown);
        TestPlayerHelper.GetAdjacent(3, 3, Direction.Left, out var actualLeft);
        TestPlayerHelper.GetAdjacent(3, 3, Direction.Right, out var actualRight);

        Assert.Multiple(() =>
        {
            Assert.That(actualUp, Is.EqualTo((2, 3)));
            Assert.That(actualDown, Is.EqualTo((4, 3)));
            Assert.That(actualLeft, Is.EqualTo((3, 2)));
            Assert.That(actualRight, Is.EqualTo((3, 4)));

            Assert.That(TestPlayerHelper.GetAdjacent(0, 0, Direction.Up, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(0, 0, Direction.Left, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(0, 9, Direction.Up, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(0, 9, Direction.Right, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(9, 0, Direction.Down, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(9, 0, Direction.Left, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(9, 9, Direction.Down, out _), Is.False);
            Assert.That(TestPlayerHelper.GetAdjacent(9, 9, Direction.Right, out _), Is.False);

            Assert.Throws<NotImplementedException>(() => TestPlayerHelper.GetAdjacent(0, 0, (Direction)42, out _));
        });
    }

    [Test]
    public void GetLongestEmpty()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var map = cpuPlayer.GetMap();

        map[0, 0] = CellState.Miss;
        map[1, 3] = CellState.Miss;
        map[2, 6] = CellState.Miss;
        map[3, 3] = CellState.Miss;
        map[3, 7] = CellState.Miss;

        map[9, 1] = CellState.Miss;
        map[4, 2] = CellState.Miss;
        map[4, 5] = CellState.Miss;
        map[8, 5] = CellState.Miss;

        // Act
        var actualHorizontal0 = cpuPlayer.GetLongestEmpty(0, Orientation.Horizontal);
        var actualHorizontal1 = cpuPlayer.GetLongestEmpty(1, Orientation.Horizontal);
        var actualHorizontal2 = cpuPlayer.GetLongestEmpty(2, Orientation.Horizontal);
        var actualHorizontal3 = cpuPlayer.GetLongestEmpty(3, Orientation.Horizontal);

        var actualVertical0 = cpuPlayer.GetLongestEmpty(0, Orientation.Vertical);
        var actualVertical1 = cpuPlayer.GetLongestEmpty(1, Orientation.Vertical);
        var actualVertical2 = cpuPlayer.GetLongestEmpty(2, Orientation.Vertical);
        var actualVertical5 = cpuPlayer.GetLongestEmpty(5, Orientation.Vertical);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(actualHorizontal0, Is.EqualTo(new LongestEmptyResult(Orientation.Horizontal, 0, 1, 9)));
            Assert.That(actualHorizontal1, Is.EqualTo(new LongestEmptyResult(Orientation.Horizontal, 1, 4, 6)));
            Assert.That(actualHorizontal2, Is.EqualTo(new LongestEmptyResult(Orientation.Horizontal, 2, 0, 6)));
            Assert.That(actualHorizontal3, Is.EqualTo(new LongestEmptyResult(Orientation.Horizontal, 3, 0, 3)));
            Assert.That(actualVertical0, Is.EqualTo(new LongestEmptyResult(Orientation.Vertical, 1, 0, 9)));
            Assert.That(actualVertical1, Is.EqualTo(new LongestEmptyResult(Orientation.Vertical, 0, 1, 9)));
            Assert.That(actualVertical2, Is.EqualTo(new LongestEmptyResult(Orientation.Vertical, 5, 2, 5)));
            Assert.That(actualVertical5, Is.EqualTo(new LongestEmptyResult(Orientation.Vertical, 0, 5, 4)));
        });
    }

    [Test]
    public void CountSpaces()
    {
        // Arrange
        var cpuPlayer = new TestPlayerHelper();
        var map = cpuPlayer.GetMap();

        map[0, 0] = CellState.Miss;
        map[9, 1] = CellState.Miss;
        map[4, 2] = CellState.Miss;
        map[4, 5] = CellState.Miss;
        map[8, 5] = CellState.Miss;
        map[6, 1] = CellState.Miss;
        map[6, 8] = CellState.Miss;

        // Act
        var u_0_0 = cpuPlayer.CountSpaces((0, 0), Direction.Up);
        var d_0_0 = cpuPlayer.CountSpaces((0, 0), Direction.Down);
        var l_0_0 = cpuPlayer.CountSpaces((0, 0), Direction.Left);
        var r_0_0 = cpuPlayer.CountSpaces((0, 0), Direction.Right);

        var u_0_9 = cpuPlayer.CountSpaces((0, 9), Direction.Up);
        var d_0_9 = cpuPlayer.CountSpaces((0, 9), Direction.Down);
        var l_0_9 = cpuPlayer.CountSpaces((0, 9), Direction.Left);
        var r_0_9 = cpuPlayer.CountSpaces((0, 9), Direction.Right);

        var u_9_0 = cpuPlayer.CountSpaces((9, 0), Direction.Up);
        var d_9_0 = cpuPlayer.CountSpaces((9, 0), Direction.Down);
        var l_9_0 = cpuPlayer.CountSpaces((9, 0), Direction.Left);
        var r_9_0 = cpuPlayer.CountSpaces((9, 0), Direction.Right);

        var u_9_9 = cpuPlayer.CountSpaces((9, 9), Direction.Up);
        var d_9_9 = cpuPlayer.CountSpaces((9, 9), Direction.Down);
        var l_9_9 = cpuPlayer.CountSpaces((9, 9), Direction.Left);
        var r_9_9 = cpuPlayer.CountSpaces((9, 9), Direction.Right);

        var u_5_4 = cpuPlayer.CountSpaces((5, 4), Direction.Up);
        var d_5_4 = cpuPlayer.CountSpaces((5, 4), Direction.Down);
        var l_5_4 = cpuPlayer.CountSpaces((5, 4), Direction.Left);
        var r_5_4 = cpuPlayer.CountSpaces((5, 4), Direction.Right);

        var u_6_5 = cpuPlayer.CountSpaces((6, 5), Direction.Up);
        var d_6_5 = cpuPlayer.CountSpaces((6, 5), Direction.Down);
        var l_6_5 = cpuPlayer.CountSpaces((6, 5), Direction.Left);
        var r_6_5 = cpuPlayer.CountSpaces((6, 5), Direction.Right);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(u_0_0, Is.EqualTo(0));
            Assert.That(d_0_0, Is.EqualTo(9));
            Assert.That(l_0_0, Is.EqualTo(0));
            Assert.That(r_0_0, Is.EqualTo(9));

            Assert.That(u_0_9, Is.EqualTo(0));
            Assert.That(d_0_9, Is.EqualTo(9));
            Assert.That(l_0_9, Is.EqualTo(8));
            Assert.That(r_0_9, Is.EqualTo(0));

            Assert.That(u_9_0, Is.EqualTo(8));
            Assert.That(d_9_0, Is.EqualTo(0));
            Assert.That(l_9_0, Is.EqualTo(0));
            Assert.That(r_9_0, Is.EqualTo(0));

            Assert.That(u_9_9, Is.EqualTo(9));
            Assert.That(d_9_9, Is.EqualTo(0));
            Assert.That(l_9_9, Is.EqualTo(7));
            Assert.That(r_9_9, Is.EqualTo(0));

            Assert.That(u_5_4, Is.EqualTo(5));
            Assert.That(d_5_4, Is.EqualTo(4));
            Assert.That(l_5_4, Is.EqualTo(4));
            Assert.That(r_5_4, Is.EqualTo(5));

            Assert.That(u_6_5, Is.EqualTo(1));
            Assert.That(d_6_5, Is.EqualTo(1));
            Assert.That(l_6_5, Is.EqualTo(3));
            Assert.That(r_6_5, Is.EqualTo(2));
        });
    }

    private class TestPlayerHelper : CpuPlayer
    {
        private static readonly FieldInfo _currentHitFieldInfo = typeof(CpuPlayer).GetField("_currentHit", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly FieldInfo _currentHitCountFieldInfo = typeof(CpuPlayer).GetField("_currentHitCount", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly FieldInfo _hitOrientationFieldInfo = typeof(CpuPlayer).GetField("_hitOrientation", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly FieldInfo _lastDirectionieldInfo = typeof(CpuPlayer).GetField("_lastDirection", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly FieldInfo _mapFieldInfo = typeof(CpuPlayer).GetField("_map", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public (int Row, int Column)? CurrentHit => ((int Row, int Column)?)_currentHitFieldInfo.GetValue(this);

        public int CurrentHitCount => (int)_currentHitCountFieldInfo.GetValue(this)!;

        public Orientation? HitOrientation => (Orientation?)_hitOrientationFieldInfo.GetValue(this);

        public Direction? LastDirection() => (Direction?)_lastDirectionieldInfo.GetValue(this);

        public CellState[,] GetMap() => (CellState[,])_mapFieldInfo.GetValue(this)!;

        public static new bool GetAdjacent(int Row, int Column, Direction direction, out (int Row, int Column) adjacent)
        {
            return CpuPlayer.GetAdjacent(Row, Column, direction, out adjacent);
        }

        public new LongestEmptyResult GetLongestEmpty(int line, Orientation orientation)
        {
            return base.GetLongestEmpty(line, orientation);
        }

        public int CountSpaces((int Row, int Column) value, Direction direction)
        {
            return CountCells(value, direction, CellState.None);
        }
    }
}
