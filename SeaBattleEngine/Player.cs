using CommunityToolkit.Diagnostics;

namespace SeaBattleEngine;

public class PlayerInfo : IPlayerInfo
{
    public string Id { get; }

    public string Name { get; }

    public string CircuitId { get; set; } = string.Empty;

    public PlayerInfo(string name, string id)
    {
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));

        Id = id;
        Name = name;
    }
}

public class Player(string name, string id) : PlayerInfo(name, id), IPlayer
{
    public IMap Map { get; } = new Map();
}
