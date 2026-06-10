// AI-generated

using Silk.NET.Maths;
using Silk.NET.SDL;
using StbImageSharp;

namespace TheAdventure;

public sealed unsafe class Sprite : IDisposable
{
    private readonly Sdl _sdl;
    private readonly Texture* _texture;
    private bool _disposed;

    public Sprite(Sdl sdl, Renderer* renderer, string path)
    {
        _sdl = sdl;

        ImageResult image = ImageResult.FromMemory(
            File.ReadAllBytes(path),
            ColorComponents.RedGreenBlueAlpha
        );

        fixed (byte* pixels = image.Data)
        {
            Surface* surface = sdl.CreateRGBSurfaceFrom(
                pixels,
                image.Width,
                image.Height,
                32,
                image.Width * 4,
                0x000000FF,
                0x0000FF00,
                0x00FF0000,
                0xFF000000
            );

            _texture = sdl.CreateTextureFromSurface(renderer, surface);
            sdl.FreeSurface(surface);
        }

        _sdl.SetTextureBlendMode(_texture, BlendMode.Blend);
    }

    public void Draw(Renderer* renderer, Rectangle<int> destination)
    {
        _sdl.RenderCopy(renderer, _texture, null, &destination);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _sdl.DestroyTexture(_texture);
        _disposed = true;
    }
}

// end AI-generated