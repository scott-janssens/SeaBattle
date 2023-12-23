namespace SeaBattleEngine;

public enum GameMode
{
    Setup,
    Waiting,
    Play,
    End
}

public enum Orientation
{
    Horizontal,
    Vertical
}

public enum CellState
{
    None,
    Miss,
    Hit,
    Sunk
}

public enum GuessResult
{
    Miss,
    Hit,
    Sunk
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
