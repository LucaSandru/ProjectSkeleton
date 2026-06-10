using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure;

public class GameRenderer
{
    private readonly Sdl _sdl;
    private readonly IntPtr _renderer;

    private readonly Sprite _playerSprite;
    private readonly Sprite _coinSprite;
    private readonly Sprite _bombSprite;

    private readonly Sprite _heartSprite;

    public unsafe GameRenderer(Sdl sdl, IntPtr renderer)
    {
        _sdl = sdl;
        _renderer = renderer;

        var realRenderer = (Renderer*)renderer;
        string assetsPath = Path.Combine(AppContext.BaseDirectory, "assets");

        _playerSprite = new Sprite(sdl, realRenderer, Path.Combine(assetsPath, "player.png"));
        _coinSprite = new Sprite(sdl, realRenderer, Path.Combine(assetsPath, "coin.png"));
        _bombSprite = new Sprite(sdl, realRenderer, Path.Combine(assetsPath, "bomb.png"));
        _heartSprite = new Sprite(sdl, realRenderer, Path.Combine(assetsPath, "heart.png"));
    }

    public unsafe void Render(CatchGame game, int virtualWidth)
    {
        var renderer = (Renderer*)_renderer;

        _sdl.SetRenderDrawColor(renderer, 0, 0, 0, 255);
        _sdl.RenderClear(renderer);

        DrawHud(game, virtualWidth);
        DrawPlayer(game.Player);
        DrawItems(game.Items);

        if (game.IsGameOver)
        {
            DrawGameOverPanel(game);
        }

        _sdl.RenderPresent(renderer);
    }

    private unsafe void DrawHud(CatchGame game, int virtualWidth)
    {
        if (game.HasReachedWinningScore)
        {
            DrawCustomText($"SCORE: {game.Score} WINNER!", 15, 15, 2, 255, 215, 0);
        }
        else
        {
            DrawCustomText($"SCORE: {game.Score} / 300", 15, 15, 2, 255, 0, 0);
        }

        DrawCustomText("LIFE:", virtualWidth - 190, 20, 2, 0, 150, 255);

        var renderer = (Renderer*)_renderer;
        const int heartSize = 24;
        const int heartSpacing = 8;

        for (int i = 0; i < game.Lives; i++)
        {
            int heartX = virtualWidth - 105 + i * (heartSize + heartSpacing);

            var heartRect = new Rectangle<int>(
                heartX,
                15,
                heartSize,
                heartSize
            );

            _heartSprite.Draw(renderer, heartRect);
        }
    }

    private unsafe void DrawPlayer(Player player)
    {
        var renderer = (Renderer*)_renderer;

        var destination = new Rectangle<int>(
            player.X,
            player.Y - 25,
            player.Width,
            player.Height + 35
        );

        _playerSprite.Draw(renderer, destination);
    }

    private unsafe void DrawItems(IReadOnlyList<FallingItem> items)
    {
        var renderer = (Renderer*)_renderer;

        foreach (FallingItem item in items)
        {
            if (item.Y < 0)
            {
                continue;
            }

            var destination = new Rectangle<int>(
                item.X,
                item.Y,
                item.Width,
                item.Height
            );

            if (item.IsGood)
            {
                _coinSprite.Draw(renderer, destination);
            }
            else
            {
                _bombSprite.Draw(renderer, destination);
            }
        }
    }

    private unsafe void DrawGameOverPanel(CatchGame game)
    {
        var renderer = (Renderer*)_renderer;

        _sdl.SetRenderDrawColor(renderer, 25, 25, 25, 255);
        var panelRect = new Rectangle<int>(150, 220, 500, 360);
        _sdl.RenderFillRect(renderer, &panelRect);

        _sdl.SetRenderDrawColor(renderer, 255, 0, 0, 255);
        var borderRect = new Rectangle<int>(148, 218, 504, 364);
        _sdl.RenderDrawRect(renderer, &borderRect);

        string title = game.Status == GameStatus.Won ? "YOU WIN!" : "GAME OVER!";
        DrawCustomText(title, 275, 250, 4, 255, 0, 0);

        string resultText = game.Status == GameStatus.Won
            ? "WINNER! SCORE >= 300!"
            : "LOSER! NO LIVES LEFT!";

        DrawCustomText(resultText, 220, 310, 3, 255, 0, 0);

        string scoreText = $"FINAL SCORE: {game.Score}";
        DrawCustomText(scoreText, 400 - scoreText.Length * 12, 370, 4, 0, 150, 255);

        string highScoreText = $"HIGHSCORE: {game.HighScore}";
        DrawCustomText(highScoreText, 400 - highScoreText.Length * 12, 410, 4, 255, 255, 0);

        DrawCustomText("PRESS R TO RESTART", 220, 490, 3, 200, 200, 200);
        DrawCustomText("PRESS E TO EXIT", 250, 535, 3, 200, 200, 200);
    }

    private unsafe void DrawCustomText(string text, int startX, int startY, int pixelSize, byte red, byte green, byte blue)
    {
        // AI-generated

        var renderer = (Renderer*)_renderer;
        _sdl.SetRenderDrawColor(renderer, red, green, blue, 255);

        int currentX = startX;

        foreach (char character in text.ToUpperInvariant())
        {
            if (character == ' ')
            {
                currentX += 6 * pixelSize;
                continue;
            }

            int mask = character switch
            {
                'W' => 0b10001_10001_10101_10101_01010,
                'X' => 0b10001_01010_00100_01010_10001,
                'Q' => 0b01110_10001_10001_10011_01111,
                'N' => 0b10001_11001_10101_10011_10001,
                'C' => 0b01110_10000_10000_10000_01110,
                'R' => 0b11110_10001_11110_10010_10001,
                'S' => 0b01111_10000_01110_00001_11110,
                'O' => 0b01110_10001_10001_10001_01110,
                'E' => 0b11111_10000_11110_10000_11111,
                'Y' => 0b10001_10001_01010_00100_00100,
                'U' => 0b10001_10001_10001_10001_01110,
                'L' => 0b10000_10000_10000_10000_11111,
                'T' => 0b11111_00100_00100_00100_00100,
                'P' => 0b11110_10001_11110_10000_10000,
                'A' => 0b01110_10001_11111_10001_10001,
                'I' => 0b01110_00100_00100_00100_01110,
                'G' => 0b01110_10000_11110_10001_01110,
                'M' => 0b10001_11011_10101_10001_10001,
                'H' => 0b10001_10001_11111_10001_10001,
                'F' => 0b11111_10000_11110_10000_10000,
                'V' => 0b10001_10001_01010_01010_00100,
                'D' => 0b11110_10001_10001_10001_11110,
                '!' => 0b00100_00100_00100_00000_00100,
                ':' => 0b00000_01100_00000_01100_00000,
                '0' => 0b01110_10011_10101_11001_01110,
                '1' => 0b00100_01100_00100_00100_01110,
                '2' => 0b01110_10001_00010_00100_11111,
                '3' => 0b11111_00010_01110_00010_11111,
                '4' => 0b10001_10001_11111_00001_00001,
                '5' => 0b11111_10000_11110_00001_11110,
                '6' => 0b01110_10000_11110_10001_01110,
                '7' => 0b11111_00001_00010_00100_00100,
                '8' => 0b01110_10001_01110_10001_01110,
                '9' => 0b01110_10001_01111_00001_01110,
                '/' => 0b00001_00010_00100_01100_10000,
                '>' => 0b10000_01000_00100_01000_10000,
                '<' => 0b00100_01000_10000_01000_00100,
                '=' => 0b00000_11111_00000_11111_00000,
                '-' => 0b00000_00000_11111_00000_00000,
                _ => 0
            };

            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    int bitIndex = 24 - (row * 5 + col);

                    if (((mask >> bitIndex) & 1) == 1)
                    {
                        var pixelRect = new Rectangle<int>(
                            currentX + col * pixelSize,
                            startY + row * pixelSize,
                            pixelSize,
                            pixelSize
                        );

                        _sdl.RenderFillRect(renderer, &pixelRect);
                    }
                }
            }

            currentX += 6 * pixelSize;
        }

        // end AI-generated
    }

}