namespace TheAdventure;

public interface IGameEntity
{
    int X { get; set; }
    int Y { get; set; }
    int Width { get; }
    int Height { get; }

    // Every game entity should know how to update its own position or state
    void Update();
}