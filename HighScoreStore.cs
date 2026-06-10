// AI-generated

namespace TheAdventure;

public class HighScoreStore
{
    private readonly string _filePath;

    public HighScoreStore(string fileName)
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    }

    public int Load()
    {
        if (!File.Exists(_filePath))
        {
            return 0;
        }

        string? content = File.ReadAllText(_filePath);

        return int.TryParse(content, out int highScore)
            ? highScore
            : 0;
    }

    public void Save(int score)
    {
        File.WriteAllText(_filePath, score.ToString());
    }

    // end AI-generated
}