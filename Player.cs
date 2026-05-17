namespace TheAdventure;

public class Player
{
	// The player's position on the screen
	public int X { get; private set; }
	public int Y { get; private set; }

	// The size of the spaceship (a rectangle for now)
	public int Width { get; private set; } = 60;
	public int Height { get; private set; } = 20;

	// How fast the spaceship moves
	public int Speed { get; private set; } = 17;

	// Player stats
	public int Lives { get; set; } = 3;
	public int Score { get; set; } = 0;

	public Player(int startX, int startY)
	{
		X = startX;
		Y = startY;
	}

	// Methods to move the ship
	public void MoveLeft()
	{
		X -= Speed;

		// Prevent going off the left edge (0)
		if (X < 0) X = 0;
	}

	public void MoveRight(int screenWidth)
	{
		X += Speed;

		// Prevent going off the right edge
		if (X + Width > screenWidth) X = screenWidth - Width;
	}
}