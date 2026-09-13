using System;
using Game2048.Models;
using Xunit;

namespace Game2048.Tests.CoreLogic;

public class TilePlacementTests
{
    [Fact]
    public void AddTwo_OnEmptyBoard_PlacesSingleTwoAndReturnsTrue()
    {
        int[,] grid = new int[4, 4];

        bool placed = GameEngine.AddTwo(grid);

        Assert.True(placed);
        int twoCount = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (grid[i, j] == 2) twoCount++;
                else Assert.Equal(0, grid[i, j]);
            }
        }
        Assert.Equal(1, twoCount);
    }

    [Fact]
    public void AddTwo_OnBoardWithSingleEmptyCell_PlacesTwoAtExactLocation()
    {
        // Given board with exactly one zero at (2, 3)
        int[,] grid = {
            { 4, 8, 16, 32 },
            { 64, 128, 256, 512 },
            { 16, 32, 64, 0 },
            { 2, 4, 8, 16 }
        };

        bool placed = GameEngine.AddTwo(grid);

        Assert.True(placed);
        Assert.Equal(2, grid[2, 3]);
    }

    [Fact]
    public void AddTwo_OnFullBoard_ReturnsFalseAndLeavesGridUnchanged()
    {
        // Given fully occupied board
        int[,] grid = {
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 },
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 }
        };

        bool placed = GameEngine.AddTwo(grid);

        Assert.False(placed);
        Assert.Equal(2, grid[0, 0]);
        Assert.Equal(4, grid[0, 1]);
    }

    [Fact]
    public void AddTwo_WithSeededRandom_PlacesTileDeterministically()
    {
        int[,] gridA = new int[4, 4];
        int[,] gridB = new int[4, 4];

        var rngA = new Random(12345);
        var rngB = new Random(12345);

        GameEngine.AddTwo(gridA, rngA);
        GameEngine.AddTwo(gridB, rngB);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(gridA[i, j], gridB[i, j]);
            }
        }
    }

    [Fact]
    public void AddTwo_RepeatedCalls_OnlyPlacesTwosUntilFull()
    {
        int[,] grid = new int[4, 4];

        for (int step = 0; step < 16; step++)
        {
            bool placed = GameEngine.AddTwo(grid);
            Assert.True(placed);
        }

        // 17th call must fail
        Assert.False(GameEngine.AddTwo(grid));

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(2, grid[i, j]);
            }
        }
    }
}
