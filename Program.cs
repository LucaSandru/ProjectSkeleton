using System;
using System.Diagnostics;
using Silk.NET.SDL;
using Silk.NET.Maths;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
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

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
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
                if (ex != null)
                {
                    throw ex;
                }

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
            if (ex != null)
            {
                throw ex;
            }

            throw new Exception("Failed to create renderer.");
        }

        // Create our spaceship near the bottom center of the 800x800 screen
        var player = new Player(370, 700);

        // Spawn a falling item at X:400 (middle), Y:0 (top of screen)
        var goodItem = new FallingItem(400, 0);

        bool quit = false;
        while (!quit)
        {
            // FIXED 1: The unsafe block correctly wraps the Event Loop pointer (&ev)
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
                        case (uint)EventType.Windowevent:
                            {
                                switch (ev.Window.Event)
                                {
                                    case (byte)WindowEventID.Shown:
                                    case (byte)WindowEventID.Exposed:
                                        break;
                                    case (byte)WindowEventID.Hidden:
                                        break;
                                    case (byte)WindowEventID.Moved:
                                        break;
                                    case (byte)WindowEventID.SizeChanged:
                                        break;
                                    case (byte)WindowEventID.Minimized:
                                    case (byte)WindowEventID.Maximized:
                                    case (byte)WindowEventID.Restored:
                                        break;
                                    case (byte)WindowEventID.Enter:
                                        break;
                                    case (byte)WindowEventID.Leave:
                                        break;
                                    case (byte)WindowEventID.FocusGained:
                                        break;
                                    case (byte)WindowEventID.FocusLost:
                                        break;
                                    case (byte)WindowEventID.Close:
                                        break;
                                    case (byte)WindowEventID.TakeFocus:
                                        {
                                            sdl.SetWindowInputFocus(sdl.GetWindowFromID(ev.Window.WindowID));
                                            break;
                                        }
                                }
                                break;
                            }

                        case (uint)EventType.Fingermotion:
                            break;

                        // FIXED 2: Removed all the old mouse logic (startX, endY, etc)
                        case (uint)EventType.Mousemotion:
                            break;

                        case (uint)EventType.Fingerdown:
                            mouseButtonStates[(byte)MouseButton.Primary] = 1;
                            break;

                        case (uint)EventType.Mousebuttondown:
                            mouseButtonStates[ev.Button.Button] = 1;
                            break;

                        case (uint)EventType.Fingerup:
                            mouseButtonStates[(byte)MouseButton.Primary] = 0;
                            break;

                        case (uint)EventType.Mousebuttonup:
                            mouseButtonStates[ev.Button.Button] = 0;
                            break;

                        case (uint)EventType.Mousewheel:
                            break;

                        case (uint)EventType.Keyup:
                            break;

                        case (uint)EventType.Keydown:
                            Console.WriteLine($"Key down: {(KeyCode)ev.Key.Keysym.Scancode}");
                            break;
                    }
                }
            }

            // Handle continuous keyboard input for smooth movement
            if (keyboardState[(byte)KeyCode.Left] > 0)
            {
                player.MoveLeft();
            }
            if (keyboardState[(byte)KeyCode.Right] > 0)
            {
                player.MoveRight(800); // 800 is the width of our window
            }

            // Update the falling item
            goodItem.Update();

            var elapsed = timer.Elapsed;
            timer.Restart();

            unsafe
            {
                var r = (Renderer*)renderer;

                // 1. Draw a black background for space
                sdl.SetRenderDrawColor(r, 0, 0, 0, 255);
                sdl.RenderClear(r);

                // 2. Draw the player as a Blue Rectangle
                sdl.SetRenderDrawColor(r, 0, 150, 255, 255);
                var playerRect = new Rectangle<int>(player.X, player.Y, player.Width, player.Height);

                sdl.RenderFillRect(r, &playerRect);
                
                // Draw the falling item as a Green Rectangle
                sdl.SetRenderDrawColor(r, 0, 255, 0, 255);
                var goodItemRect = new Rectangle<int>(goodItem.X, goodItem.Y, goodItem.Width, goodItem.Height);
                sdl.RenderFillRect(r, &goodItemRect);

                // 3. Show it on screen
                sdl.RenderPresent(r);
            }

            framesRenderedCounter++;
        }


        // FIXED 4: Clean up happens completely outside the while loop, destroying both renderer and window!
        unsafe
        {
            sdl.DestroyRenderer((Renderer*)renderer);
            sdl.DestroyWindow((Window*)window);
        }

        sdl.Quit();
    }
}