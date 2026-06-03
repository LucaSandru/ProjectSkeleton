namespace TheAdventure;


public class Player : IGameEntity
{
    // Interface requirements
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    // Movement speed constant
    public int Speed { get; set; }

    public Player()
    {
        // Initial spawning position matching your central main loop defaults
        X = 370;
        Y = 700;
        Width = 105;
        Height = 30;
        Speed = 10; // Adjust as needed for desired movement responsiveness
    }

    /// Fulfills the IGameEntity interface contract.
    /// Player movement is driven externally by real-time keyboard events,
    /// so this loop tick check can safely remain clear.
    public void Update()
    {
        // Intentionally left blank to fulfill interface layout cleanly
    }

    /// Shifts the spaceship position safely to the left, stopping at the boundary.
    public void MoveLeft()
    {
        X -= Speed;
        if (X < 0)
        {
            X = 0;
        }
    }

    /// <summary>
    /// Shifts the spaceship position safely to the right, stopping at the virtual width boundary.
    /// </summary>
    /// <param name="virtualWidth">The maximum design width of the game window (e.g., 800)</param>
    public void MoveRight(int virtualWidth)
    {
        X += Speed;
        if (X + Width > virtualWidth)
        {
            X = virtualWidth - Width;
        }
    }
}