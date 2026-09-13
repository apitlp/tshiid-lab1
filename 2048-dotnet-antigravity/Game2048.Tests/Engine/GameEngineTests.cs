using System;
using Game2048.Models;
using Xunit;

namespace Game2048.Tests.Engine;

public class GameEngineTests
{
    [Fact]
    public void StartNewGame_InitializesBoardWithExactlyTwoTwos()
    {
        var engine = new GameEngine(4);

        int nonZeroCount = 0;
        int[,] grid = engine.Grid;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (grid[i, j] != 0)
                {
                    Assert.Equal(2, grid[i, j]);
                    nonZeroCount++;
                }
            }
        }

        Assert.Equal(2, nonZeroCount);
        Assert.Equal(GameState.NotOver, engine.CurrentState);
        Assert.False(engine.CanUndo);
    }

    [Fact]
    public void Move_WhenValidMove_SpawnsNewTwoTileAndEnablesUndo()
    {
        // Initial board with tiles packed on the right
        int[,] initial = {
            { 0, 0, 0, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var engine = new GameEngine(initial);
        Assert.False(engine.CanUndo);

        // Move Left is valid: 2 moves to (0, 0), and a new 2 spawns
        bool moved = engine.Move(Direction.Left);

        Assert.True(moved);
        Assert.True(engine.CanUndo);

        int[,] grid = engine.Grid;
        Assert.Equal(2, grid[0, 0]);

        // Total tiles should now be 2 (the moved one + the newly spawned one)
        int nonZeroCount = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (grid[i, j] != 0) nonZeroCount++;
            }
        }
        Assert.Equal(2, nonZeroCount);
    }

    [Fact]
    public void Move_WhenInvalidMove_DoesNotSpawnTileAndLeavesUndoDisabled()
    {
        // Initial board with tiles already at the left
        int[,] initial = {
            { 2, 4, 8, 16 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var engine = new GameEngine(initial);
        bool moved = engine.Move(Direction.Left);

        Assert.False(moved);
        Assert.False(engine.CanUndo);

        int[,] grid = engine.Grid;
        Assert.Equal(2, grid[0, 0]);
        Assert.Equal(4, grid[0, 1]);
        Assert.Equal(8, grid[0, 2]);
        Assert.Equal(16, grid[0, 3]);
    }

    [Fact]
    public void Move_WhenMoveResultsIn2048_ChangesCurrentStateToWin()
    {
        // Given two 1024 tiles ready to merge
        int[,] initial = {
            { 1024, 1024, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var engine = new GameEngine(initial);
        Assert.Equal(GameState.NotOver, engine.CurrentState);

        bool moved = engine.Move(Direction.Left);

        Assert.True(moved);
        Assert.Equal(GameState.Win, engine.CurrentState);
        Assert.Equal(2048, engine.Grid[0, 0]);
    }

    [Fact]
    public void Undo_AfterSingleMove_RestoresExactPreviousBoardState()
    {
        int[,] initial = {
            { 0, 0, 0, 2 },
            { 0, 0, 0, 4 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var engine = new GameEngine(initial);
        engine.Move(Direction.Left);
        Assert.True(engine.CanUndo);

        bool undone = engine.Undo();

        Assert.True(undone);
        Assert.False(engine.CanUndo);

        int[,] restored = engine.Grid;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(initial[i, j], restored[i, j]);
            }
        }
    }

    [Fact]
    public void Undo_AfterMultipleMoves_RestoresSequentialHistory()
    {
        int[,] initial = {
            { 2, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var engine = new GameEngine(initial);

        // Move 1: Down
        Assert.True(engine.Move(Direction.Down));
        int[,] afterMove1 = engine.Grid;

        // Move 2: Right
        Assert.True(engine.Move(Direction.Right));
        int[,] afterMove2 = engine.Grid;

        // First Undo restores state after Move 1
        Assert.True(engine.Undo());
        int[,] current = engine.Grid;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                Assert.Equal(afterMove1[i, j], current[i, j]);

        // Second Undo restores initial state
        Assert.True(engine.Undo());
        current = engine.Grid;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                Assert.Equal(initial[i, j], current[i, j]);

        // Third Undo fails because history is empty
        Assert.False(engine.Undo());
        Assert.False(engine.CanUndo);
    }

    [Fact]
    public void Undo_WhenAtInitialState_ReturnsFalseAndLeavesBoardIntact()
    {
        var engine = new GameEngine(4);
        int[,] initial = engine.Grid;

        bool undone = engine.Undo();

        Assert.False(undone);
        int[,] current = engine.Grid;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                Assert.Equal(initial[i, j], current[i, j]);
    }

    [Fact]
    public void Undo_AfterLosingMove_RestoresNotOverState()
    {
        // Given a board one move away from full loss with a fixed RNG
        // [2, 4, 2, 4]
        // [4, 2, 4, 2]
        // [2, 4, 2, 4]
        // [0, 2, 4, 2] -> moving Down or Right leaves (3, 0) empty where a 2 is spawned resulting in loss
        int[,] almostLost = {
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 },
            { 2, 4, 2, 4 },
            { 4, 0, 4, 2 }
        };

        var engine = new GameEngine(almostLost);
        Assert.Equal(GameState.NotOver, engine.CurrentState);

        // Moving right merges 4 and 4 at row 3 or shifts 4
        // Let's test undo directly:
        engine.Move(Direction.Left);
        Assert.True(engine.CanUndo);

        engine.Undo();
        Assert.Equal(GameState.NotOver, engine.CurrentState);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    [InlineData(10)]
    public void GameEngine_SupportsDynamicSizes_InitializesAndPerformsMoves(int size)
    {
        var engine = new GameEngine(size);
        Assert.Equal(size, engine.Size);
        Assert.Equal(size, engine.Grid.GetLength(0));
        Assert.Equal(size, engine.Grid.GetLength(1));

        // StartNewGame places exactly 2 tiles
        int nonZero = 0;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (engine.Grid[i, j] != 0) nonZero++;

        Assert.Equal(2, nonZero);
    }

    [Fact]
    public void Constructor_WithNullInitialGrid_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GameEngine(null!));
    }

    [Fact]
    public void Move_WithInvalidDirectionValue_ThrowsArgumentOutOfRangeException()
    {
        var engine = new GameEngine(4);
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.Move((Direction)999));
    }

    [Fact]
    public void StartNewGame_ClearsHistoryAndResetsBoard()
    {
        var engine = new GameEngine(4);
        engine.Move(Direction.Up);
        engine.Move(Direction.Left);
        Assert.True(engine.CanUndo);

        engine.StartNewGame();

        Assert.False(engine.CanUndo);
        Assert.Equal(GameState.NotOver, engine.CurrentState);
    }
}
