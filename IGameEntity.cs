namespace TheAdventure;

public interface IGameEntity
{
    int X { get; set; }
    int Y { get; set; }
    int Width { get; }
    int Height { get; }

    void Update();
}