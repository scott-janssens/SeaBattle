using CommunityToolkit.Diagnostics;
using System;

namespace SeaBattleEngine;

public class CpuPlayer : Player
{
    private readonly IRandom _random;
    private readonly CellState[,] _map;
    private readonly bool _checkerboard0;
    private readonly Stack<int> _cpuSection;
    private int? _cpuCurrentSection;
    private (int Row, int Column)? _currentHit;
    private int _currentHitCount;
    private Orientation? _hitOrientation;
    private Direction? _lastDirection;

    public CpuPlayer()
        : this(new RandomHelper())
    {
    }

    public CpuPlayer(IRandom random)
        : base("CPU", Guid.Empty.ToString())
    {
        _random = random;
        _checkerboard0 = _random.Next(2) == 0;
        _map = new CellState[10, 10];

        for (var r = 0; r < 10; r++)
        {
            for (var c = 0; c < 10; c++)
            {
                _map[r, c] = new();
            }
        }

        var sections = new List<int> { 0, 1, 2, 3, 5, 6, 7, 8 };
        _cpuSection = new();

        while (sections.Count > 0)
        {
            var i = _random.Next(sections.Count);
            _cpuSection.Push(sections[i]);
            sections.RemoveAt(i);
        }

        _cpuCurrentSection = 4;
    }

    public void SetupShips()
    {
        while (!Map.UnplacedShips.IsEmpty)
        {
            var ship = Map.UnplacedShips.First();
            do
            {
                Map.PlaceShip(
                    _random.Next(10 - ship.Size), 
                    _random.Next(10 - ship.Size), 
                    _random.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical, ship);

                if (ship.IsValid)
                {
                    Map.LockShip();
                }
            } while (!ship.IsValid);
        }
    }
    public (int Row, int Column) CpuGuess()
    {
        (int Row, int Column) guess;

        do
        {
            guess = CpuGuessInternal();
        } while (_map[guess.Row, guess.Column] != CellState.None);

        return guess;
    }

    public (int Row, int Column) CpuGuessInternal()
    {
        (int Row, int Column) guess;

        if (_currentHit.HasValue)
        {
            guess = SinkShip();
        }
        else
        {
            var orphanedHits = GetHits();

            if (orphanedHits.Count > 0)
            {
                _currentHit = orphanedHits.First();
                _currentHitCount = 1;
                _hitOrientation = null;
                guess = SinkShip();
            }
            else if (_cpuCurrentSection == null)
            {
                guess = WideAttack();
            }
            else
            {
                guess = GridAttack();
            }
        }

        return guess;
    }

    protected (int Row, int Column) SinkShip()
    {
        (int Row, int Column) guess;

        if (_hitOrientation == null)
        {
            List<(Direction Direction, int Length)> list = new()
                {
                    { (Direction.Up, CountCells(_currentHit!.Value, Direction.Up, CellState.None)) },
                    { (Direction.Down, CountCells(_currentHit.Value, Direction.Down, CellState.None)) },
                    { (Direction.Left, CountCells(_currentHit.Value, Direction.Left, CellState.None)) },
                    { (Direction.Right, CountCells(_currentHit.Value, Direction.Right, CellState.None)) }
                };

            list.Sort((x, y) => -(x.Length - y.Length));
            _lastDirection = list.First().Direction;

            GetAdjacent(_currentHit.Value.Row, _currentHit.Value.Column, _lastDirection.Value, out guess);
        }
        else
        {
            var isValidCell = GetAdjacent(_currentHit!.Value.Row, _currentHit.Value.Column, _lastDirection!.Value, out guess);

            if (!isValidCell || _map[guess.Row, guess.Column] != CellState.None)
            {
                _lastDirection = _lastDirection switch
                {
                    Direction.Up => Direction.Down,
                    Direction.Down => Direction.Up,
                    Direction.Left => Direction.Right,
                    Direction.Right => Direction.Left,
                    null => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };

                do
                {
                    isValidCell = GetAdjacent(guess.Row, guess.Column, _lastDirection.Value, out guess);

                    if (!isValidCell || _map[guess.Row, guess.Column] == CellState.Miss)
                    {
                        _currentHitCount = 1;
                        _hitOrientation = null;
                        guess = SinkShip();
                        break;
                    }
                } while (_map[guess.Row, guess.Column] != CellState.None);
            }
        }

        return guess;
    }

    protected (int Row, int Column) WideAttack()
    {
        (int Row, int Column) guess;

        List<LongestEmptyResult> list = new()
            {
                { GetLongestEmpty(0, Orientation.Horizontal) },
                { GetLongestEmpty(1, Orientation.Horizontal) },
                { GetLongestEmpty(2, Orientation.Horizontal) },
                { GetLongestEmpty(3, Orientation.Horizontal) },
                { GetLongestEmpty(4, Orientation.Horizontal) },
                { GetLongestEmpty(5, Orientation.Horizontal) },
                { GetLongestEmpty(6, Orientation.Horizontal) },
                { GetLongestEmpty(7, Orientation.Horizontal) },
                { GetLongestEmpty(8, Orientation.Horizontal) },
                { GetLongestEmpty(9, Orientation.Horizontal) },
                { GetLongestEmpty(0, Orientation.Vertical) },
                { GetLongestEmpty(1, Orientation.Vertical) },
                { GetLongestEmpty(2, Orientation.Vertical) },
                { GetLongestEmpty(3, Orientation.Vertical) },
                { GetLongestEmpty(4, Orientation.Vertical) },
                { GetLongestEmpty(5, Orientation.Vertical) },
                { GetLongestEmpty(6, Orientation.Vertical) },
                { GetLongestEmpty(7, Orientation.Vertical) },
                { GetLongestEmpty(8, Orientation.Vertical) },
                { GetLongestEmpty(9, Orientation.Vertical) }
            };

        list.Sort((x, y) => -(x.Length - y.Length));

        var longestResult = list.First();

        if (longestResult.Orientation == Orientation.Horizontal)
        {
            guess = (longestResult.Row, longestResult.Column + longestResult.Length / 2);

            if (!IsCheckerboardMatch(guess.Row, guess.Column))
            {
                guess = (guess.Row, guess.Column + 1);

                if (_map[guess.Row, guess.Column] != CellState.None)
                {
                    guess = (guess.Row, guess.Column - 2);
                }
            }
        }
        else
        {
            guess = (longestResult.Row + longestResult.Length / 2, longestResult.Column);

            if (!IsCheckerboardMatch(guess.Row, guess.Column))
            {
                guess = (guess.Row + 1, guess.Column);

                if (_map[guess.Row, guess.Column] != CellState.None)
                {
                    guess = (guess.Row - 2, guess.Column);
                }
            }
        }

        return guess;
    }

    protected (int Row, int Column) GridAttack()
    {
        (int Row, int Column) guess;

        var section = CpuGetSection();
        var guesses = CellStateCount(section);

        if (guesses >= 2)
        {
            _cpuCurrentSection = _cpuSection.Count == 0 ? _cpuSection.Pop() : null;
            return CpuGuessInternal();
        }

        var rStart = (_cpuCurrentSection / 3 * 3)!.Value;
        var cStart = (_cpuCurrentSection % 3 * 3)!.Value;
        var isStartCheckerboard = IsCheckerboardMatch(rStart, cStart);

        if (guesses == 0)
        {

            if (isStartCheckerboard)
            {
                var offset = _random.Next(2) + 1;
                guess = (rStart + offset, cStart + offset);
            }
            else
            {
                guess = isStartCheckerboard ? (rStart + 1, cStart + 2) : (rStart + 2, cStart + 1);
            }
        }
        else
        {
            var r = _random.Next(2);
            int c = _random.Next(2) * 2;

            if (r == 0)
            {
                if (isStartCheckerboard)
                {
                    c++;
                }
            }
            else
            {
                if (!isStartCheckerboard)
                {
                    c++;
                }
            }

            guess = (rStart + r * 3, cStart + c);
        }

        return guess;
    }

    protected List<(int Row, int Column)> GetHits()
    {
        var list = new List<(int Row, int Column)>();

        for (var r = 0; r < 10; r++)
        {
            for (var c = 0; c < 10; c++)
            {
                if (_map[r,c] == CellState.Hit)
                {
                    list.Add((r, c));
                }
            }
        }

        return list;
    }

    protected static bool GetAdjacent(int Row, int Column, Direction direction, out (int Row, int Column) adjacent)
    {
        adjacent = direction switch
        {
            Direction.Up => (Row - 1, Column),
            Direction.Down => (Row + 1, Column),
            Direction.Left => (Row, Column - 1),
            Direction.Right => (Row, Column + 1),
            _ => throw new NotImplementedException(),
        };

        return adjacent.Row >= 0 && adjacent.Row < 10 &&
            adjacent.Column >= 0 && adjacent.Column < 10;
    }

    protected LongestEmptyResult GetLongestEmpty(int line, Orientation orientation)
    {
        LongestEmptyResult result;
        int index = 0;
        var count = 0;
        int length = 0;

        if (orientation == Orientation.Horizontal)
        {
            for (var i = 0; i < 10; i++)
            {
                if (_map[line, i] == CellState.None)
                {
                    count++;
                }
                else
                {
                    if (count > length)
                    {
                        index = i - count;
                        length = count;
                    }

                    count = 0;
                }
            }

            if (count > length)
            {
                index = 10 - count;
                length = count;
            }

            result = new(orientation, line, index, length);
        }
        else
        {
            for (var i = 0; i < 10; i++)
            {
                if (_map[i, line] == CellState.None)
                {
                    count++;
                }
                else
                {
                    if (count > length)
                    {
                        index = i - count;
                        length = count;
                    }

                    count = 0;
                }
            }

            if (count > length)
            {
                index = 10 - count;
                length = count;
            }

            result = new(orientation, index, line, length);
        }

        return result;
    }

    protected int CountCells((int Row, int Column) value, Direction direction, CellState cellState)
    {
        int row = value.Row;
        int column = value.Column;
        var count = 0;

        if (direction == Direction.Up)
        {
            row--;

            while (row >= 0 && _map[row, column] == cellState)
            {
                row--;
                count++;
            }
        }
        else if (direction == Direction.Down)
        {
            row++;

            while (row < 10 && _map[row, column] == cellState)
            {
                row++;
                count++;
            }
        }
        else if (direction == Direction.Left)
        {
            column--;

            while (column >= 0 && _map[row, column] == cellState)
            {
                column--;
                count++;
            }
        }
        else
        {
            column++;

            while (column < 10 && _map[row, column] == cellState)
            {
                column++;
                count++;
            }
        }

        return count;
    }

    public void ApplyGuessResult(int row, int column, TurnResult result)
    {
        _map[row, column] = (CellState)(result.Result + 1);

        if (result.Result == GuessResult.Hit)
        {
            _currentHitCount++;

            if (_currentHitCount == 2)
            {
                _hitOrientation = _currentHit!.Value.Row - row == 0 ? Orientation.Horizontal : Orientation.Vertical;
            }

            _currentHit = (row, column);
        }
        else if (result.Result == GuessResult.Sunk)
        {
            _currentHit = null;
            _currentHitCount = 0;
            _hitOrientation = null;
        }

        if (result.ShipSunk != null)
        {
            if (result.ShipSunk.Orientation == Orientation.Horizontal)
            {
                for (var c = result.ShipSunk.Column; c < result.ShipSunk.Column + result.ShipSunk.Size; c++)
                {
                    _map[result.ShipSunk.Row, c] = CellState.Sunk;
                }
            }
            else
            {
                for (var r = result.ShipSunk.Row; r < result.ShipSunk.Row + result.ShipSunk.Size; r++)
                {
                    _map[r, result.ShipSunk.Column] = CellState.Sunk;
                }
            }
        }
    }

    protected bool IsCheckerboardMatch(int r, int c)
    {
        var match = (r & 1) == (c & 1);
        return _checkerboard0 ? match : !match;
    }

    protected CellState[,] CpuGetSection()
    {
        var result = new CellState[4, 4];
        var ri = 0;
        int ci;
        var rStart = (_cpuCurrentSection / 3 * 3)!.Value;
        var cStart = (_cpuCurrentSection % 3 * 3)!.Value;

        for (var r = rStart; r < rStart + 4; r++, ri++)
        {
            ci = 0;

            for (var c = cStart; c < cStart + 4; c++, ci++)
            {
                result[ri, ci] = _map[r, c];
            }
        }

        return result;
    }

    protected static int CellStateCount(CellState[,] cells)
    {
        var count = 0;

        foreach (var c in cells)
        {
            if (c != CellState.None)
            {
                count++;
            }
        }

        return count;
    }

    protected internal record LongestEmptyResult(Orientation Orientation, int Row, int Column, int Length);
}
