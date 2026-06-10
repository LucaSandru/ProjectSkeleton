using Silk.NET.SDL;

namespace TheAdventure;

public class CatchGame
{
    private const int WinningScore = 300;

    private readonly Random _random = new();
    private readonly HighScoreStore _highScoreStore;
    private readonly List<FallingItem> _items = [];

    public Player Player { get; } = new();

    public IReadOnlyList<FallingItem> Items => _items;
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int Lives { get; private set; } = 3;
    public int CurrentLevel { get; private set; } = 1;
    public GameStatus Status { get; private set; } = GameStatus.Playing;

    public bool IsGameOver => Status is GameStatus.Won or GameStatus.Lost;
    public bool HasReachedWinningScore => Score >= WinningScore;

    public CatchGame(HighScoreStore highScoreStore, int virtualWidth)
    {
        _highScoreStore = highScoreStore;
        HighScore = _highScoreStore.Load();

        for (int i = 0; i < 3; i++)
        {
            int x = _random.Next(0, virtualWidth - 35);
            int y = -i * 200;
            bool isGood = _random.Next(0, 10) < 7;

            _items.Add(new FallingItem(x, y, 5, isGood));
        }
    }

    public void HandleKeyboard(ReadOnlySpan<byte> keyboardState, int virtualWidth)
    {
        if (Status != GameStatus.Playing)
        {
            return;
        }

        if (keyboardState[(byte)KeyCode.Left] > 0)
        {
            Player.MoveLeft();
        }

        if (keyboardState[(byte)KeyCode.Right] > 0)
        {
            Player.MoveRight(virtualWidth);
        }
    }

    public void Update(int virtualWidth, int virtualHeight)
    {
        if (Status != GameStatus.Playing)
        {
            return;
        }

        UpdateLevel();

        foreach (FallingItem item in _items)
        {
            item.Update();

            if (CollisionHelper.IsColliding(Player, item))
            {
                HandleCaughtItem(item, virtualWidth);
            }
            else if (item.Y > virtualHeight)
            {
                HandleMissedItem(item, virtualWidth);
            }
        }
    }

    public void Restart(int virtualWidth)
    {
        Score = 0;
        Lives = 3;
        CurrentLevel = 1;
        Status = GameStatus.Playing;

        // AI-generated

        Player.Reset();

        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].X = _random.Next(0, virtualWidth - _items[i].Width);
            _items[i].Y = -i * 200;
            _items[i].Speed = 5;
            _items[i].IsGood = _random.Next(0, 10) < 7;
        }

        // end AI-generated
    }

    private void UpdateLevel()
    {
        // AI-generated

        int calculatedLevel = 1 + Score / 60;

        if (calculatedLevel == CurrentLevel)
        {
            return;
        }

        CurrentLevel = calculatedLevel;
        Player.Speed = 16 + CurrentLevel;

        int itemSpeed = GetCurrentItemSpeed();

        foreach (FallingItem item in _items)
        {
            item.Speed = itemSpeed;
        }

        // end AI-generated
    }

    private void HandleCaughtItem(FallingItem item, int virtualWidth)
    {
        if (item.IsGood)
        {
            Score += 10;
            SaveHighScoreIfNeeded();
        }
        else
        {
            LoseLife();
        }

        item.Reset(_random, virtualWidth, GetCurrentItemSpeed());
    }

    private void HandleMissedItem(FallingItem item, int virtualWidth)
    {
        if (item.IsGood)
        {
            LoseLife();
        }

        item.Reset(_random, virtualWidth, GetCurrentItemSpeed());
    }

    private void LoseLife()
    {
        Lives--;

        if (Lives <= 0)
        {
            Status = Score >= WinningScore
                ? GameStatus.Won
                : GameStatus.Lost;

            SaveHighScoreIfNeeded();
        }
    }

    private int GetCurrentItemSpeed()
    {
        return 5 + CurrentLevel;
    }

    // AI-generated
    private void SaveHighScoreIfNeeded()
    {

        if (Score <= HighScore)
        {
            return;
        }

        HighScore = Score;
        _highScoreStore.Save(HighScore);
    }

    // end AI-generated
}