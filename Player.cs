namespace TheAdventure;

public class Player : IGameEntity
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; } = 105;
    public int Height { get; } = 30;
    public int Speed { get; set; } = 10;

    public Player()
    {
        Reset();
    }

    public void Update()
    {
    }

    public void MoveLeft()
    {
        X -= Speed;

        if (X < 0)
        {
            X = 0;
        }
    }

    public void MoveRight(int virtualWidth)
    {
        X += Speed;

        if (X + Width > virtualWidth)
        {
            X = virtualWidth - Width;
        }
    }

    public void Reset()
    {
        X = 370;
        Y = 700;
        Speed = 10;
    }
}