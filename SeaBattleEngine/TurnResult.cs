namespace SeaBattleEngine;

public record TurnResult
{
    public required GuessResult Result { get; init; }

    public ShipInfo? ShipSunk { get; init; } = null;

    public Orientation? Orientation { get; init; } = null;

    public bool IsGameOver { get; init; }

    public string? WinnerId { get; set; } = null;

    public ShipInfo[]? Ships { get; set; } = null;
}
