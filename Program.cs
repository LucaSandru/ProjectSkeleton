using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    private const int VirtualWidth = 800;
    private const int VirtualHeight = 800;

    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        int initResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer);

        if (initResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        unsafe
        {
            Window* window = sdl.CreateWindow(
                "The Adventure",
                Sdl.WindowposUndefined,
                Sdl.WindowposUndefined,
                VirtualWidth,
                VirtualHeight,
                (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
            );

            if (window is null)
            {
                throw new InvalidOperationException("Failed to create SDL window.");
            }

            Renderer* renderer = sdl.CreateRenderer(
                window,
                -1,
                (uint)RendererFlags.Accelerated | (uint)RendererFlags.Presentvsync
            );

            if (renderer is null)
            {
                throw new InvalidOperationException("Failed to create SDL renderer.");
            }

            sdl.RenderSetLogicalSize(renderer, VirtualWidth, VirtualHeight);

            RunGameLoop(sdl, window, renderer);

            sdl.DestroyRenderer(renderer);
            sdl.DestroyWindow(window);
        }

        sdl.Quit();
    }

    private static unsafe void RunGameLoop(Sdl sdl, Window* window, Renderer* renderer)
    {
        ReadOnlySpan<byte> keyboardState = new(sdl.GetKeyboardState(null), (int)KeyCode.Count);

        var ev = new Event();
        var highScoreStore = new HighScoreStore("highscore.txt");
        var game = new CatchGame(highScoreStore, VirtualWidth);
        var gameRenderer = new GameRenderer(sdl, (IntPtr)renderer);

        bool quit = false;

        const uint targetFps = 60;
        const uint frameDelayMs = 1000 / targetFps;

        while (!quit)
        {
            uint frameStartTicks = sdl.GetTicks();

            while (sdl.PollEvent(&ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    quit = true;
                    break;
                }

                if (ev.Type == (uint)EventType.Keydown)
                {
                    if (HandleKeyDown(sdl, window, ev, game))
                    {
                        quit = true;
                        break;
                    }
                }
            }

            game.HandleKeyboard(keyboardState, VirtualWidth);
            game.Update(VirtualWidth, VirtualHeight);
            gameRenderer.Render(game, VirtualWidth);

            uint frameTimeMs = sdl.GetTicks() - frameStartTicks;

            if (frameTimeMs < frameDelayMs)
            {
                sdl.Delay(frameDelayMs - frameTimeMs);
            }
        }
    }

    private static unsafe bool HandleKeyDown(Sdl sdl, Window* window, Event ev, CatchGame game)
    {
        bool pressedF11 = ev.Key.Keysym.Sym == (int)KeyCode.F11;
        bool pressedAltEnter =
            ev.Key.Keysym.Sym == (int)KeyCode.Return &&
            (ev.Key.Keysym.Mod & (uint)Keymod.Alt) > 0;

        if (pressedF11 || pressedAltEnter)
        {
            ToggleFullscreen(sdl, window);
        }

        bool pressedR = ev.Key.Keysym.Sym == 114;

        if (pressedR && game.IsGameOver)
        {
            game.Restart(VirtualWidth);
        }

        bool pressedE = ev.Key.Keysym.Sym == 101 || (int)ev.Key.Keysym.Scancode == 26;

        if (pressedE  && game.IsGameOver)
        {
            return true;
        }

        return false;
    }

    private static unsafe void ToggleFullscreen(Sdl sdl, Window* window)
    {
        uint flags = sdl.GetWindowFlags(window);

        if ((flags & (uint)WindowFlags.FullscreenDesktop) > 0)
        {
            sdl.SetWindowFullscreen(window, 0);
        }
        else
        {
            sdl.SetWindowFullscreen(window, (uint)WindowFlags.FullscreenDesktop);
        }
    }
}