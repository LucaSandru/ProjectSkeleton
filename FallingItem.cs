namespace TheAdventure;

public class FallingItem : IGameEntity
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; } = 35;
    public int Height { get; } = 35;
    public int Speed { get; set; }
    public bool IsGood { get; set; }

    public FallingItem(int startX, int startY, int speed, bool isGood)
    {
        X = startX;
        Y = startY;
        Speed = speed;
        IsGood = isGood;
    }

    public void Update()
    {
        Y += Speed;
    }

    public void Reset(Random random, int virtualWidth, int speed)
    {
        X = random.Next(0, virtualWidth - Width);
        Y = 0;
        Speed = speed;
        IsGood = random.Next(0, 10) < 7;
    }
}