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


        // Create a list to hold all of our falling items
        var itemsList = new List<FallingItem>();
        int numberOfItems = 3; // You can change this to 2, 3, 5, etc. to set the difficulty!


        var random = new Random();

        for (int i = 0; i < numberOfItems; i++)
        {
            // Spawn each item at a random horizontal position and staggered heights
            // Staggering the Y positions (like 0, -150, -300) stops them from falling in a perfect, boring row
            int randomX = random.Next(0, 800 - 30);
            int staggeredY = -i * 200;

            // 70% chance to start as a good item, 30% chance to start as a bad item
            bool initialIsGood = random.Next(0, 10) < 7;  

            itemsList.Add(new FallingItem(randomX, staggeredY, initialIsGood));
        }


        bool quit = false;

        int score = 0;
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



            // Loop through every item in our game
            foreach (var item in itemsList)
            {
                // Move this specific item down
                item.Update();

                // Check collision between the player and this specific item
                bool isColliding = player.X < item.X + item.Width &&
                                   player.X + player.Width > item.X &&
                                   player.Y < item.Y + item.Height &&
                                   player.Y + player.Height > item.Y;

                if (isColliding)
                {
                    // Handle collision consequences based on item type
                    if (item.IsGood)
                    {
                        score += 10;
                        Console.WriteLine($"Item Caught! +10 Points. Score: {score}");
                    }
                    else
                    {
                        score -= 15;
                        Console.WriteLine($"Hit a Bad Item! -15 Points. Score: {score}");
                    }

                    // Reset to top, randomize position, and randomize its type for next time
                    item.Y = 0;
                    item.X = random.Next(0, 800 - item.Width);
                    item.IsGood = random.Next(0, 10) < 7;
                }
                else if (item.Y > 800)
                {

                    if (item.IsGood)
                    {
                        Console.WriteLine($"Good Item slip away! -15 Points. Score: {score}");
                    }
                    // Item fell off the screen without hitting the player. 
                    // No score change needed (ignoring bad items is good play!).

                    // Reset to top, randomize position, and randomize its type for next time
                    item.Y = 0;
                    item.X = random.Next(0, 800 - item.Width);
                    item.IsGood = random.Next(0, 10) < 7;
                }
            }




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

                // Draw ALL the items inside itemsList with dynamic colors
                foreach (var item in itemsList)
                {
                    if (item.Y >= 0)
                    {
                        // Choose Green if Good, Red if Bad
                        if (item.IsGood)
                        {
                            sdl.SetRenderDrawColor(r, 0, 255, 0, 255); // Green
                        }
                        else
                        {
                            sdl.SetRenderDrawColor(r, 255, 0, 0, 255); // Red
                        }

                        var itemRect = new Rectangle<int>(item.X, item.Y, item.Width, item.Height);
                        sdl.RenderFillRect(r, &itemRect);
                    }
                }

                sdl.RenderPresent(r);
            }

            framesRenderedCounter++;
        }


        // Clean up the renderer and window!
        unsafe
        {
            sdl.DestroyRenderer((Renderer*)renderer);
            sdl.DestroyWindow((Window*)window);
        }

        sdl.Quit();
    }
}