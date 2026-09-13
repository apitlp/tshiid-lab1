using Game2048.Models;
using Xunit;

namespace Game2048.Tests.CoreLogic;

public class GameStateTests
{
    [Fact]
    public void CheckGameState_With2048TilePresent_ReturnsWin()
    {
        // Given a board with 2048 tile and empty spaces
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 32, 64, 128, 256 },
            { 512, 1024, 2048, 0 },
            { 0, 0, 0, 0 }
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.Win, state);
    }

    [Fact]
    public void CheckGameState_With2048TileInFullyOccupiedGridWithNoMatches_ReturnsWin()
    {
        // Win must take priority even if board is full
        int[,] grid = {
            { 2048, 2, 4, 8 },
            { 8, 4, 2, 16 },
            { 16, 32, 64, 128 },
            { 128, 64, 32, 16 }
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.Win, state);
    }

    [Fact]
    public void CheckGameState_WithEmptyCellsRemaining_ReturnsNotOver()
    {
        // Given a board with at least one zero and no 2048
        int[,] grid = {
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 },
            { 2, 4, 2, 4 },
            { 4, 2, 4, 0 } // exactly one empty cell
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.NotOver, state);
    }

    [Fact]
    public void CheckGameState_WhenBoardFullWithHorizontalMatchInFirstRow_ReturnsNotOver()
    {
        int[,] grid = {
            { 2, 2, 4, 8 }, // match: 2 == 2
            { 4, 8, 16, 32 },
            { 8, 16, 32, 64 },
            { 16, 32, 64, 128 }
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.NotOver, state);
    }

    [Fact]
    public void CheckGameState_WhenBoardFullWithVerticalMatchInMiddle_ReturnsNotOver()
    {
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 4, 8, 16, 32 },
            { 8, 8, 32, 64 }, // (1, 1) and (2, 1) are both 8!
            { 16, 32, 64, 128 }
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.NotOver, state);
    }

    [Fact]
    public void CheckGameState_WhenBoardFullWithHorizontalMatchOnLastRow_ReturnsNotOver()
    {
        // Tests edge loop: mat[len-1][k] == mat[len-1][k+1]
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 4, 8, 16, 32 },
            { 8, 16, 32, 64 },
            { 16, 32, 64, 64 } // bottom-right horizontal match
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.NotOver, state);
    }

    [Fact]
    public void CheckGameState_WhenBoardFullWithVerticalMatchOnLastColumn_ReturnsNotOver()
    {
        // Tests edge loop: mat[j, size - 1] == mat[j + 1, size - 1]
        // Ensure no earlier horizontal or last-row match triggers first
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 4, 8, 2, 16 }, // (0,3) == (1,3) == 16 is the only match on the board
            { 2, 4, 8, 32 },
            { 4, 2, 4, 2 }
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.NotOver, state);
    }

    [Fact]
    public void CheckGameState_WhenBoardFullWithNoMatches_ReturnsLose()
    {
        // Alternating pattern with zero adjacent matches horizontally or vertically
        int[,] grid = {
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 },
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 }
        };

        var state = GameEngine.CheckGameState(grid);

        Assert.Equal(GameState.Lose, state);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void CheckGameState_OnArbitraryGridSizes_EvaluatesCorrectly(int size)
    {
        // Full board without matches
        int[,] loseGrid = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                loseGrid[i, j] = ((i + j) % 2 == 0) ? 2 : 4;
            }
        }
        Assert.Equal(GameState.Lose, GameEngine.CheckGameState(loseGrid));

        // Grid with an empty cell
        loseGrid[0, 0] = 0;
        Assert.Equal(GameState.NotOver, GameEngine.CheckGameState(loseGrid));

        // Grid with a winning 2048 tile
        loseGrid[0, 0] = 2048;
        Assert.Equal(GameState.Win, GameEngine.CheckGameState(loseGrid));
    }
}
