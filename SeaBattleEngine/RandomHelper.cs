namespace SeaBattleEngine;

public interface IRandom
{
    int Seed { get; }
    int Next(int maxValue);
}

public class RandomHelper : IRandom
{
    private Random _random = default!;

    private int _seed;
    public int Seed
    {
        get => _seed;
        private set
        {
            _seed = value;
            _random = new Random(_seed);
        }
    }

    public RandomHelper()
        : this(DateTime.Now.Ticks)
    {            
    }

    public RandomHelper(long seed)
    {
        Seed = (int)seed;
    }

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }
}
