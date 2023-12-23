using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;

namespace SeaBattleEngine;

public abstract class Ship
{
    public readonly string InstanceId = Guid.NewGuid().ToString();

    public abstract string Name { get; }

    public abstract int Size { get; }

    public int Row { get; private set; }

    public int Column { get; private set; }

    public Orientation Orientation { get; private set; }

    public ImmutableArray<bool> ZonesHit { get; private set; }

    public bool IsValid { get; internal set; } = true;

    public bool IsSeaworthy => ZonesHit.Any(x => !x);

    public Ship()
    {
        ZonesHit = ImmutableArray.Create(new bool[Size]);
    }

    public void SetLocation(int row, int column, Orientation orientation, bool forceInvalid)
    {
        if (row >= 0 && row < 10 && column >= 0 && column < 10)
        {
            Row = row;
            Column = column;
            Orientation = orientation;

            IsValid = !forceInvalid && 
                (row >= 0 && row < 10 && column >= 0 && column < 10 &&
                orientation == Orientation.Horizontal && column + Size <= 10 ||
                (orientation == Orientation.Vertical && row + Size <= 10));
        }
    }

    public bool Hit(int row, int column)
    {
        Guard.IsInRange(row, Row, Row + Size, nameof(row));
        Guard.IsInRange(column, Column, Column + Size, nameof(column));

        var zone = row - Row + (column - Column);
        var newZonesHit = new bool[Size];

        for (int i = 0; i < Size; i++)
        {
            newZonesHit[i] = ZonesHit[i];
        }

        newZonesHit[zone] = true;

        ZonesHit = ImmutableArray.Create(newZonesHit);
        return !IsSeaworthy;
    }

    public void SetHits(bool[] hits)
    {
        ZonesHit = [.. hits];
    }

    public void MarkSunk()
    {
        ZonesHit = Enumerable.Repeat(true, Size).ToImmutableArray();
    }

    public static implicit operator ShipInfo(Ship ship) => new() { Name = ship.Name, Row = ship.Row, Column = ship.Column, Size = ship.Size, Orientation = ship.Orientation, ZonesHit = [.. ship.ZonesHit] };
}

public class ShipInfo
{
    public required string Name { get; set; }

    public required int Row { get; set; }

    public required int Column { get; set; }

    public required int Size { get; set; }

    public required Orientation Orientation { get; set; }

    public required bool[] ZonesHit { get; set; }
}

public class Carrier : Ship
{
    public override string Name => "Carrier";

    public override int Size => 5;
}

public class Battleship : Ship
{
    public override string Name => "Battleship";

    public override int Size => 4;
}

public class Cruiser : Ship
{
    public override string Name => "Cruiser";

    public override int Size => 3;
}

public class Submarine : Ship
{
    public override string Name => "Submarine";

    public override int Size => 3;
}

public class Destroyer : Ship
{
    public override string Name => "Destroyer";

    public override int Size => 2;
}
