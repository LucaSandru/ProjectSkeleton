using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silk.NET.SDL;
using Silk.NET.Maths;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var random = new Random();
        var sdl = new Sdl(new SdlContext());

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
        }

        IntPtr renderer;
        unsafe
        {
            renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated);
            sdl.RenderSetVSync((Renderer*)renderer, 1);
        }

        if (renderer == IntPtr.Zero)
        {
            var ex = sdl.GetErrorAsException();
            if (ex != null) throw ex;
            throw new Exception("Failed to create renderer.");
        }

        // Setup Player and Items
        var player = new Player(370, 700);
        var itemsList = new List<FallingItem>();
        int numberOfItems = 3;

        for (int i = 0; i < numberOfItems; i++)
        {
            int randomX = random.Next(0, 800 - 30);
            int staggeredY = -i * 200;
            bool initialIsGood = random.Next(0, 10) < 7;
            itemsList.Add(new FallingItem(randomX, staggeredY, initialIsGood));
        }

        bool quit = false;
        int score = 0;
        int lives = 3;
        bool isGameOver = false;
        bool isGameWon = false;

        while (!quit)
        {
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
                            Console.WriteLine($"Key down: {(KeyCode)ev.Key.Keysym.Scancode}");

                            // RESET MECHANIC: Press 'P' to restart when game is over or won
                            if (isGameOver || isGameWon)
                            {
                                // FIXED: Added explicit (int) cast to Scancode to resolve compiler error
                                if (ev.Key.Keysym.Sym == 112 || (int)ev.Key.Keysym.Scancode == 19)
                                {
                                    score = 0;
                                    lives = 3;
                                    isGameOver = false;
                                    isGameWon = false;

                                    for (int i = 0; i < itemsList.Count; i++)
                                    {
                                        itemsList[i].Y = -i * 200;
                                        itemsList[i].X = random.Next(0, 800 - itemsList[i].Width);
                                        itemsList[i].IsGood = random.Next(0, 10) < 7;
                                    }
                                    player.X = 370;
                                    Console.WriteLine("--- Game Restarted! ---");
                                }
                            }
                            break;
                    }
                }
            }

            // FREEZE MECHANIC: Only update physics if the game is actively running
            if (!isGameOver && !isGameWon)
            {
                if (keyboardState[(byte)KeyCode.Left] > 0)
                {
                    player.MoveLeft();
                }
                if (keyboardState[(byte)KeyCode.Right] > 0)
                {
                    player.MoveRight(800);
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
                            Console.WriteLine($"Item Caught! +10 Points. Score: {score} | Lives: {lives}");
                            if (score >= 100) isGameWon = true;
                        }
                        else
                        {
                            lives--;
                            Console.WriteLine($"Ouch! Hit a Bad Item! -1 Life. Score: {score} | Lives: {lives}");
                            if (lives <= 0) isGameOver = true;
                        }

                        item.Y = 0;
                        item.X = random.Next(0, 800 - item.Width);
                        item.IsGood = random.Next(0, 10) < 7;
                    }
                    else if (item.Y > 800)
                    {
                        if (item.IsGood)
                        {
                            lives--;
                            Console.WriteLine($"Oh no! You missed a Good Item! -1 Life. Score: {score} | Lives: {lives}");
                            if (lives <= 0) isGameOver = true;
                        }

                        item.Y = 0;
                        item.X = random.Next(0, 800 - item.Width);
                        item.IsGood = random.Next(0, 10) < 7;
                    }
                }
            }

            // RENDER SYSTEM
            // RENDER SYSTEM
            unsafe
            {
                var r = (Renderer*)renderer;

                // Screen ALWAYS clears to pure space black
                sdl.SetRenderDrawColor(r, 0, 0, 0, 255);
                sdl.RenderClear(r);

                // ==========================================================
                // 1. LIVE HUD DISPLAY (Top-Left Score Counter)
                // ==========================================================
                // Formats the live text string dynamically every single frame
                string liveScoreText = $"SCORE: {score}";

                // Draws at (X: 15, Y: 15) with a compact pixel size of 2
                DrawCustomText(liveScoreText, 15, 15, 2, 255, 255, 255);
                // ==========================================================

                // Draw the Player (Blue)
                sdl.SetRenderDrawColor(r, 0, 150, 255, 255);
                var playerRect = new Rectangle<int>(player.X, player.Y, player.Width, player.Height);
                sdl.RenderFillRect(r, &playerRect);

                // Draw all Falling Objects (Green / Red)
                foreach (var item in itemsList)
                {
                    if (item.Y >= 0)
                    {
                        if (item.IsGood)
                            sdl.SetRenderDrawColor(r, 0, 255, 0, 255);
                        else
                            sdl.SetRenderDrawColor(r, 255, 0, 0, 255);

                        var itemRect = new Rectangle<int>(item.X, item.Y, item.Width, item.Height);
                        sdl.RenderFillRect(r, &itemRect);
                    }
                }

                // CENTERED UI PANEL OVERLAY (Game Over / Victory Screen)
                if (isGameOver || isGameWon)
                {
                    // Dark grey central UI container box
                    sdl.SetRenderDrawColor(r, 25, 25, 25, 255);
                    var panelRect = new Rectangle<int>(150, 250, 500, 300);
                    sdl.RenderFillRect(r, &panelRect);

                    // Thin colored accent border around the box
                    if (isGameWon)
                        sdl.SetRenderDrawColor(r, 0, 255, 0, 255); // Green border
                    else
                        sdl.SetRenderDrawColor(r, 255, 0, 0, 255); // Red border
                    var borderRect = new Rectangle<int>(148, 248, 504, 304);
                    sdl.RenderDrawRect(r, &borderRect);

                    // Render aligned text lines inside the display card
                    if (isGameWon)
                    {
                        DrawCustomText("YOU WON!", 304, 290, 6, 0, 255, 0);
                        string scoreText = $"POINTS: {score}";
                        int scoreX = 400 - (scoreText.Length * 24) / 2;
                        DrawCustomText(scoreText, scoreX, 360, 4, 255, 255, 255);
                        DrawCustomText("PRESS P TO PLAY AGAIN!", 224, 430, 4, 200, 200, 200);
                    }
                    else
                    {
                        DrawCustomText("YOU LOST!", 292, 290, 6, 255, 0, 0);
                        string scoreText = $"POINTS: {score}";
                        int scoreX = 400 - (scoreText.Length * 24) / 2;
                        DrawCustomText(scoreText, scoreX, 360, 4, 255, 255, 255);
                        DrawCustomText("PRESS P TO TRY AGAIN!", 232, 430, 4, 200, 200, 200);
                    }
                }

                sdl.RenderPresent(r);
            }

            framesRenderedCounter++;
        }

        unsafe
        {
            sdl.DestroyRenderer((Renderer*)renderer);
            sdl.DestroyWindow((Window*)window);
        }
        sdl.Quit();

        // Retro Text Engine Method
        void DrawCustomText(string text, int startX, int startY, int pixelSize, byte red, byte green, byte blue)
        {
            unsafe
            {
                var r = (Renderer*)renderer;
                sdl.SetRenderDrawColor(r, red, green, blue, 255);
                int currentX = startX;
                foreach (char c in text.ToUpper())
                {
                    if (c == ' ') { currentX += 4 * pixelSize; continue; }
                    int mask = c switch
                    {
                        'C' => 0b111_100_100_100_111, // Open left-facing bracket shape
                        'R' => 0b111_101_111_110_101, // Clear leg separation for R
                        'S' => 0b111_100_111_001_111,
                        'O' => 0b111_101_101_101_111,
                        'E' => 0b111_100_111_100_111,
                        'Y' => 0b101_101_010_010_010,
                        'U' => 0b101_101_101_101_111,
                        'L' => 0b100_100_100_100_111,
                        'T' => 0b111_010_010_010_010,
                        'W' => 0b101_101_101_111_101,
                        'N' => 0b101_111_101_101_101,
                        'P' => 0b111_101_111_100_100,
                        'A' => 0b111_101_111_101_101,
                        'I' => 0b111_010_010_010_111,
                        'G' => 0b111_100_101_101_111,
                        '!' => 0b010_010_010_000_010,
                        ':' => 0b000_010_000_010_000,
                        '0' => 0b111_101_101_101_111,
                        '1' => 0b010_110_010_010_111,
                        '2' => 0b111_001_111_100_111,
                        '3' => 0b111_001_111_001_111,
                        '4' => 0b101_101_111_001_001,
                        '5' => 0b111_100_111_001_111,
                        '6' => 0b111_100_111_101_111,
                        '7' => 0b111_001_010_010_010,
                        '8' => 0b111_101_111_101_111,
                        '9' => 0b111_101_111_001_111,
                        _ => 0b111_111_111_111_111  // Square block for unsupported chars
                    };
                    for (int row = 0; row < 5; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            int bitIndex = 14 - (row * 3 + col);
                            if (((mask >> bitIndex) & 1) == 1)
                            {
                                var pixelRect = new Rectangle<int>(currentX + col * pixelSize, startY + row * pixelSize, pixelSize, pixelSize);
                                sdl.RenderFillRect(r, &pixelRect);
                            }
                        }
                    }
                    currentX += 4 * pixelSize;
                }
            }
        }
    }
}