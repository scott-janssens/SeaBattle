using System.Collections.Immutable;

namespace SeaBattleEngine;

public interface IMap : IEnumerable<CellState>
{
    Carrier Carrier { get; }
    Battleship Battleship { get; }
    Cruiser Cruiser { get; }
    Submarine Submarine { get; }
    Destroyer Destroyer { get; }
    
    int PlayerShipsLeft { get; }
    ImmutableList<Ship> UnplacedShips { get; }

    Ship GetShipByName(string name);
    void LockShip();
    ShipInfo[] GetShipInfo();
    void PlaceShip(int row, int column, Orientation orientation, Ship ship);
    TurnResult Guess(int row, int column);
}