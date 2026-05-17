namespace TheAdventure;

public class FallingItem
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 30;  // A bit smaller than the player
    public int Height { get; set; } = 30;
    public int Speed { get; set; } = 5;   // How fast it falls down the screen

    public FallingItem(int startX, int startY)
    {
        X = startX;
        Y = startY;
    }

    // This method makes gravity work by pushing the item down
    public void Update()
    {
        Y += Speed;
    }
}