using CommunityToolkit.Diagnostics;
using System.Collections;

namespace SeaBattleEngine;

public class CellGrid : IEnumerable<CellState>
{
    private readonly CellState[,] _cells = new CellState[10, 10];

    public CellGrid()
    {
        for (int i = 0; i < 100; i++)
        {
            _cells[i / 10, i % 10] = new();
        }
    }

    public CellState? GetCell(int x, int y)
    {
        return (x < 0 || x > 9 || y < 0 || y > 9) ? null : _cells[x, y];
    }

    public void SetCell(int x, int y, CellState state)
    {
        Guard.IsInRange(x, 0, 10, nameof(x));
        Guard.IsInRange(y, 0, 10, nameof(y));

        _cells[x, y] = state;
    }

    public CellState? this[int row, int column]
    {
        get => GetCell(row, column);
        set 
        { 
            if (value.HasValue) 
            {
                SetCell(row, column, value.Value);
            }
        }
    }

    public IEnumerator<CellState> GetEnumerator() => _cells.Cast<CellState>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _cells.GetEnumerator();
}
