using System;

namespace TheAdventure;

// Your own custom exception class!
public class GameAssetException : Exception
{
    public GameAssetException(string message) : base(message) { }
}