using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silk.NET.SDL;
using Silk.NET.Maths;
using System.Linq;
using System.IO;

namespace TheAdventure;

public static class Program
{
    private static Sdl sdl = null!;
    private static IntPtr renderer;

    private const int VirtualWidth = 800;
    private const int VirtualHeight = 800;

    public static T LoadGameConfig<T>(string value, Func<string, T> parser)
    {
        try
        {
            return parser(value);
        }
        catch
        {
            throw new GameAssetException($"Failed to safely parse game configuration value: '{value}'");
        }
    }

    public static void Main()
    {
        var random = new Random();
        sdl = new Sdl(new SdlContext());

        ulong framesRenderedCounter = 0;
        var timer = new Stopwatch();
        timer.Start();

        ReadOnlySpan<byte> keyboardState;
        unsafe
        {
            keyboardState = new(sdl.GetKeyboardState(null), (int)KeyCode.Count);
        }

        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];
        var ev = new Event();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller | Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        IntPtr window;
        unsafe
        {
            window = (IntPtr)sdl.CreateWindow(
                "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 800, 800,
                (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
            );

            if (window == IntPtr.Zero)
            {
                var ex = sdl.GetErrorAsException();
                if (ex != null) throw ex;
                throw new Exception("Failed to create window.");
            }

            renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated | (uint)RendererFlags.Presentvsync);
            if (renderer == IntPtr.Zero)
            {
                var ex = sdl.GetErrorAsException();
                if (ex != null) throw ex;
                throw new Exception("Failed to create renderer.");
            }
        }

        unsafe
        {
            sdl.RenderSetLogicalSize((Renderer*)renderer, VirtualWidth, VirtualHeight);
        }

        var player = new Player();
        var itemsList = new List<FallingItem>();
        int numberOfItems = 3;

        for (int i = 0; i < numberOfItems; i++)
        {
            int randomX = random.Next(0, VirtualWidth - 30);
            int staggeredY = -i * 200;
            int itemSpeed = 5;
            bool initialIsGood = random.Next(0, 10) < 7;

            itemsList.Add(new FallingItem(randomX, staggeredY, itemSpeed, initialIsGood));
        }

        bool quit = false;
        int score = 0;
        int highscore = 0;
        int lives = 3;
        bool isGameOver = false;
        bool isGameWon = false;
        int currentLevel = 1;

        string highscoreFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highscore.txt");
        if (System.IO.File.Exists(highscoreFile))
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(highscoreFile))
            {
                string? fileContent = reader.ReadLine();
                if (int.TryParse(fileContent, out int savedHighScore))
                {
                    highscore = savedHighScore;
                }
            }
        }

        const uint targetFps = 60;
        const uint frameDelayMs = 1000 / targetFps;

        while (!quit)
        {
            uint frameStartTicks = sdl.GetTicks();

            int calculatedLevel = 1 + (score / 60);
            int currentMaxItemSpeed = 5 + (calculatedLevel * 1);

            if (calculatedLevel != currentLevel)
            {
                currentLevel = calculatedLevel;
                player.Speed = 16 + currentLevel;
                itemsList.ForEach(item => item.Speed = currentMaxItemSpeed);
            }

            if (score > highscore)
            {
                highscore = score;
                try
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(highscoreFile, false))
                    {
                        writer.Write(highscore);
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Disk write postponed: {ex.Message}");
                }
            }

            if (score >= 300)
            {
                isGameWon = true;
            }

            unsafe
            {
                while (sdl.PollEvent(&ev) != 0)
                {
                    if (ev.Type == (uint)EventType.Quit)
                    {
                        quit = true;
                        break;
                    }

                    switch (ev.Type)
                    {
                        case (uint)EventType.Keydown:
                            if (ev.Key.Keysym.Sym == (int)KeyCode.F11 ||
                                (ev.Key.Keysym.Sym == (int)KeyCode.Return && (ev.Key.Keysym.Mod & (uint)Keymod.Alt) > 0))
                            {
                                uint flags = sdl.GetWindowFlags((Window*)window);
                                if ((flags & (uint)WindowFlags.FullscreenDesktop) > 0)
                                {
                                    sdl.SetWindowFullscreen((Window*)window, 0);
                                }
                                else
                                {
                                    sdl.SetWindowFullscreen((Window*)window, (uint)WindowFlags.FullscreenDesktop);
                                }
                            }

                            if (isGameOver)
                            {
                                if (ev.Key.Keysym.Sym == 112 || (int)ev.Key.Keysym.Scancode == 19) // 'P' key
                                {
                                    score = 0;
                                    lives = 3;
                                    isGameOver = false;
                                    isGameWon = false;

                                    for (int i = 0; i < itemsList.Count; i++)
                                    {
                                        itemsList[i].Y = -i * 200;
                                        itemsList[i].X = random.Next(0, VirtualWidth - itemsList[i].Width);
                                        itemsList[i].IsGood = random.Next(0, 10) < 7;
                                        itemsList[i].Speed = 5;
                                    }
                                    player.X = 370;
                                }
                            }
                            break;
                    }
                }
            }

            if (!isGameOver)
            {
                if (keyboardState[(byte)KeyCode.Left] > 0)
                {
                    player.MoveLeft();
                }
                if (keyboardState[(byte)KeyCode.Right] > 0)
                {
                    player.MoveRight(VirtualWidth);
                }

                foreach (var item in itemsList)
                {
                    item.Update();

                    bool isColliding = player.X < item.X + item.Width &&
                                       player.X + player.Width > item.X &&
                                       player.Y < item.Y + item.Height &&
                                       player.Y + player.Height > item.Y;

                    if (isColliding)
                    {
                        if (item.IsGood)
                        {
                            score += 10;
                        }
                        else
                        {
                            lives--;
                            if (lives <= 0) isGameOver = true;
                        }

                        item.Y = 0;
                        item.X = random.Next(0, VirtualWidth - item.Width);
                        item.IsGood = random.Next(0, 10) < 7;
                        item.Speed = currentMaxItemSpeed;
                    }
                    else if (item.Y > VirtualHeight)
                    {
                        if (item.IsGood)
                        {
                            lives--;
                            if (lives <= 0) isGameOver = true;
                        }

                        item.Y = 0;
                        item.X = random.Next(0, VirtualWidth - item.Width);
                        item.IsGood = random.Next(0, 10) < 7;
                        item.Speed = currentMaxItemSpeed;
                    }
                }
            }

            // RENDER SYSTEM
            unsafe
            {
                var r = (Renderer*)renderer;

                sdl.SetRenderDrawColor(r, 0, 0, 0, 255);
                sdl.RenderClear(r);

                if (isGameWon)
                {
                    DrawCustomText($"SCORE: {score} WINNER!", 15, 15, 2, 255, 215, 0);
                }
                else
                {
                    DrawCustomText($"SCORE: {score} / 300", 15, 15, 2, 255, 0, 0);
                }

                DrawCustomText("LIFE:", VirtualWidth - 190, 20, 2, 0, 150, 255);
                sdl.SetRenderDrawColor(r, 255, 0, 0, 255);

                int dotSize = 16;
                int dotSpacing = 10;
                for (int i = 0; i < lives; i++)
                {
                    int dotX = VirtualWidth - 100 + (i * (dotSize + dotSpacing));
                    int dotY = 22;
                    var lifeRect = new Rectangle<int>(dotX, dotY, dotSize, dotSize);
                    sdl.RenderFillRect(r, &lifeRect);
                }

                // --- DRAW PLAYER OBJECT (Neon Cyan/Blue) ---
                sdl.SetRenderDrawColor(r, 0, 180, 255, 255);
                var playerRect = new Rectangle<int>(player.X, player.Y, player.Width, player.Height);
                sdl.RenderFillRect(r, &playerRect);

                // --- DRAW FALLING ITEMS OBJECTS (Green for Money, Red for Virus) ---
                foreach (var item in itemsList)
                {
                    if (item.Y >= 0)
                    {
                        var itemRect = new Rectangle<int>(item.X, item.Y, item.Width, item.Height);

                        if (item.IsGood)
                        {
                            sdl.SetRenderDrawColor(r, 0, 255, 100, 255); // Green object
                        }
                        else
                        {
                            sdl.SetRenderDrawColor(r, 255, 50, 50, 255); // Red object
                        }
                        sdl.RenderFillRect(r, &itemRect);
                    }
                }

                if (isGameOver)
                {
                    sdl.SetRenderDrawColor(r, 25, 25, 25, 255);
                    var panelRect = new Rectangle<int>(150, 220, 500, 360);
                    sdl.RenderFillRect(r, &panelRect);

                    sdl.SetRenderDrawColor(r, 255, 0, 0, 255);
                    var borderRect = new Rectangle<int>(148, 218, 504, 364);
                    sdl.RenderDrawRect(r, &borderRect);

                    DrawCustomText("GAME OVER!", 275, 250, 4, 255, 0, 0);

                    if (score >= 300)
                    {
                        DrawCustomText("WINNER! SCORE > 300!", 240, 310, 3, 0, 255, 0);
                    }
                    else
                    {
                        DrawCustomText("LOSER! SCORE < 300!", 250, 310, 3, 255, 0, 0);
                    }

                    string scoreText = $"FINAL SCORE: {score}";
                    int scoreX = 400 - (scoreText.Length * 24) / 2;
                    DrawCustomText(scoreText, scoreX, 370, 4, 0, 150, 255);

                    string hiScoreText = $"HIGHSCORE: {highscore}";
                    int hiScoreX = 400 - (hiScoreText.Length * 24) / 2;
                    DrawCustomText(hiScoreText, hiScoreX, 410, 4, 255, 255, 0);

                    DrawCustomText("PRESS P TO RESTART", 220, 490, 3, 200, 200, 200);
                }

                sdl.RenderPresent(r);
            }

            framesRenderedCounter++;
            uint frameTimeMs = sdl.GetTicks() - frameStartTicks;
            if (frameTimeMs < frameDelayMs)
            {
                sdl.Delay(frameDelayMs - frameTimeMs);
            }
        }

        unsafe
        {
            sdl.DestroyRenderer((Renderer*)renderer);
            sdl.DestroyWindow((Window*)window);
        }
        sdl.Quit();
    }

    private static void DrawCustomText(string text, int startX, int startY, int pixelSize, byte red, byte green, byte blue)
    {
        unsafe
        {
            var r = (Renderer*)renderer;
            sdl.SetRenderDrawColor(r, red, green, blue, 255);
            int currentX = startX;

            foreach (char c in text.ToUpper())
            {
                if (c == ' ') { currentX += 6 * pixelSize; continue; }

                int mask = c switch
                {
                    'W' => 0b10001_10001_10101_10101_01010,
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
                    '!' => 0b00100_00100_00100_00000_00100,
                    ':' => 0b00000_01100_00000_01100_00000,
                    'V' => 0b10001_10001_01010_01010_00100,
                    'F' => 0b11111_10000_11110_10000_10000,
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
                    '_' => 0b11111_11111_11111_11111_11111,
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
                            var pixelRect = new Rectangle<int>(currentX + col * pixelSize, startY + row * pixelSize, pixelSize, pixelSize);
                            sdl.RenderFillRect(r, &pixelRect);
                        }
                    }
                }
                currentX += 6 * pixelSize;
            }
        }
    }
}