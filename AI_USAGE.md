# AI Usage

## Tools used

ChatGPT (GPT-5.5 Thinking)

## How AI was used

I used ChatGPT as a coding assistant for refactoring, debugging, and improving the structure of the project.

Main uses:
- Suggested splitting the original large game file into smaller files such as `CatchGame.cs`, `GameRenderer.cs`, `HighScoreStore.cs`, `CollisionHelper.cs`, and entity classes.
- Helped generate the `Sprite.cs` file for loading and rendering PNG sprites.
- Helped generate and extend the `DrawCustomText` pixel-font logic, including the needed letters and numbers.
- Helped generate the collision helper logic in `CollisionHelper.cs`.
- Helped with parts of `CatchGame.cs`, especially `Restart()`, `UpdateLevel()`, and `SaveHighScoreIfNeeded()`.
- Explained how to implement `HighScoreStore.cs` for saving and loading the high score from disk.


## Fully AI-generated files or regions

The following files or regions were mostly AI-generated and then reviewed/edited by me:

- `Sprite.cs`
- `CollisionHelper.cs`
- Parts of `GameRenderer.cs`, especially `DrawCustomText()` for drawing characters and symbols.
- Parts of `CatchGame.cs`, especially `Restart()`, `UpdateLevel()`, and `SaveHighScoreIfNeeded()`.
- Parts of `HighScoreStore.cs`.

After all AI-generated code, I reviewed and tested the code, before submission.
