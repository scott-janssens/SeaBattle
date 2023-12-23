namespace SeaBattleEngine;

public interface IPlayerInfo
{
    string Id { get; }
    string Name { get; }
    string CircuitId { get; }
}

public interface IPlayer : IPlayerInfo
{
    IMap Map { get; }
}