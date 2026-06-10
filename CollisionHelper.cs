namespace TheAdventure;

public static class CollisionHelper
{
    public static bool IsColliding(IGameEntity first, IGameEntity second)
    {
        return first.X < second.X + second.Width &&
               first.X + first.Width > second.X &&
               first.Y < second.Y + second.Height &&
               first.Y + first.Height > second.Y;
    }
}