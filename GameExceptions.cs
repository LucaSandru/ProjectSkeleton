using System;

namespace TheAdventure;

public class GameAssetException : Exception
{
    public GameAssetException(string message) : base(message) { }
}