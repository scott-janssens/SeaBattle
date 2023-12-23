using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Collections.Immutable;

namespace SeaBattleEngine;

public class Map : IMap
{
    public readonly string InstanceId = Guid.NewGuid().ToString();

    private Ship?[,] _map = new Ship?[10, 10];
    private readonly CellGrid _cellGrid = new();
    private readonly HashSet<Ship> _ships = [];
    private Ship[,]? _placementMap = null;

    public Carrier Carrier { get; } = new();
    public Battleship Battleship { get; } = new();
    public Cruiser Cruiser { get; } = new();
    public Submarine Submarine { get; } = new();
    public Destroyer Destroyer { get; } = new();

    public ImmutableList<Ship> UnplacedShips { get; private set; }

    public int PlayerShipsLeft => _ships.Count(s => s.IsSeaworthy);

    public Map()
    {
        UnplacedShips =
        [
            Carrier,
            Battleship,
            Cruiser,
            Submarine,
            Destroyer
        ];
    }

    public Ship GetShipByName(string name) => name switch
    {
        "Carrier" => Carrier,
        "Battleship" => Battleship,
        "Cruiser" => Cruiser,
        "Submarine" => Submarine,
        "Destroyer" => Destroyer,
        _ => throw new NotImplementedException()
    };

    public void PlaceShip(int row, int column, Orientation orientation, Ship ship)
    {
        if (!UnplacedShips.Contains(ship) && !_ships.Contains(ship))
        {
            ThrowHelper.ThrowArgumentException("ship does not belong to this map.");
        }

        if (UnplacedShips.Contains(ship))
        {
            UnplacedShips = UnplacedShips.Remove(ship);
            _ships.Add(ship);
        }

        var forceValidFalse = false;
        _placementMap = (Ship[,])_map.Clone();

        if (orientation == Orientation.Vertical)
        {
            for (int i = row; i < row + ship.Size; i++)
            {
                if (i > 9 || _placementMap[i, column] != null)
                {
                    forceValidFalse = true;
                    _placementMap = null;
                    break;
                }

                _placementMap[i, column] = ship;
            }
        }
        else
        {
            for (int i = column; i < column + ship.Size; i++)
            {
                if (i > 9 || _placementMap[row, i] != null)
                {
                    forceValidFalse = true;
                    _placementMap = null;
                    break;
                }

                _placementMap[row, i] = ship;
            }
        }

        ship.SetLocation(row, column, orientation, forceValidFalse);
    }

    public void LockShip()
    {
        Guard.IsNotNull(_placementMap);
        _map = _placementMap;
    }

    public ShipInfo[] GetShipInfo()
    {
        return _ships.Select(x => (ShipInfo)x).ToArray();
    }

    public TurnResult Guess(int row, int column)
    {
        TurnResult result;
        var ship = _map[row, column];

        if (ship == null)
        {
            result = new TurnResult { Result = GuessResult.Miss };
            _cellGrid[row, column] = CellState.Miss;
        }
        else
        {
            var isSunk = ship.Hit(row, column);

            if (isSunk)
            {
                result = new TurnResult { Result = GuessResult.Sunk, ShipSunk = ship, Orientation = ship.Orientation, IsGameOver = PlayerShipsLeft == 0 };
            }
            else
            {
                result = new TurnResult { Result = GuessResult.Hit };
            }
        }

        return result;
    }

    public static implicit operator CellGrid(Map map) => map._cellGrid;

    public IEnumerator<CellState> GetEnumerator() => _cellGrid.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _cellGrid.GetEnumerator();
}
