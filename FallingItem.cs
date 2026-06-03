namespace TheAdventure;

public class FallingItem : IGameEntity
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsGood { get; set; }

    // Change this from 'private readonly' to a public get/set property
    public int Speed { get; set; }

    public FallingItem(int startX, int startY, int speed, bool isGood)
    {
        X = startX;
        Y = startY;
        Width = 35;
        Height = 35;
        IsGood = isGood;
        Speed = speed; // Assign dynamic speed
    }

    public void Update()
    {
        Y += Speed; // Drops down faster or slower depending on current Speed value
    }
}