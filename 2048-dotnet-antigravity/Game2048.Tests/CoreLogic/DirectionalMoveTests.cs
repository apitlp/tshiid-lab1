using Game2048.Models;
using Xunit;

namespace Game2048.Tests.CoreLogic;

public class DirectionalMoveTests
{
    [Fact]
    public void MoveLeft_WithGapsAndEqualTiles_CompressesAndMergesToLeft()
    {
        // Given row [2, 0, 2, 4]
        int[,] grid = {
            { 2, 0, 2, 4 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When MoveLeft is executed
        var (result, done) = GameEngine.MoveLeft(grid);

        // Then compressed and merged into [4, 4, 0, 0]
        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(4, result[0, 1]);
        Assert.Equal(0, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void MoveLeft_WithTrailingTilesNeedingSecondPassCompression_PacksCompletely()
    {
        // Given row [4, 2, 2, 0]
        // CoverUp: [4, 2, 2, 0]
        // Merge:   [4, 4, 0, 0]
        // Second CoverUp: [4, 4, 0, 0]
        int[,] grid = {
            { 4, 2, 2, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveLeft(grid);

        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(4, result[0, 1]);
        Assert.Equal(0, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void MoveLeft_WithFourIdenticalTiles_ProducesTwoMergedTilesOnLeft()
    {
        // Given [2, 2, 2, 2]
        int[,] grid = {
            { 2, 2, 2, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveLeft(grid);

        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(4, result[0, 1]);
        Assert.Equal(0, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void MoveLeft_WhenAlreadyFullyShiftedAndNoMerges_ReturnsDoneFalse()
    {
        // Given fully compacted row with no equal neighbors
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 16, 8, 4, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveLeft(grid);

        Assert.False(done);
        Assert.Equal(2, result[0, 0]); Assert.Equal(4, result[0, 1]); Assert.Equal(8, result[0, 2]); Assert.Equal(16, result[0, 3]);
        Assert.Equal(16, result[1, 0]); Assert.Equal(8, result[1, 1]); Assert.Equal(4, result[1, 2]); Assert.Equal(2, result[1, 3]);
    }

    [Fact]
    public void MoveRight_WithGapsAndEqualTiles_CompressesAndMergesToRight()
    {
        // Given row [4, 2, 0, 2]
        int[,] grid = {
            { 4, 2, 0, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveRight(grid);

        // Then shifted and merged into [0, 0, 4, 4]
        Assert.True(done);
        Assert.Equal(0, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(4, result[0, 2]);
        Assert.Equal(4, result[0, 3]);
    }

    [Fact]
    public void MoveRight_WithFourIdenticalTiles_ProducesTwoMergedTilesOnRight()
    {
        // Given [2, 2, 2, 2]
        int[,] grid = {
            { 2, 2, 2, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveRight(grid);

        Assert.True(done);
        Assert.Equal(0, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(4, result[0, 2]);
        Assert.Equal(4, result[0, 3]);
    }

    [Fact]
    public void MoveRight_WhenAlreadyShiftedToRight_ReturnsDoneFalse()
    {
        // Given tiles already against right edge with no merges
        int[,] grid = {
            { 0, 2, 4, 8 },
            { 2, 4, 8, 16 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveRight(grid);

        Assert.False(done);
        Assert.Equal(0, result[0, 0]); Assert.Equal(2, result[0, 1]); Assert.Equal(4, result[0, 2]); Assert.Equal(8, result[0, 3]);
    }

    [Fact]
    public void MoveUp_WithVerticalGapsAndEqualTiles_CompressesAndMergesUpward()
    {
        // Given column 0 has [2, 0, 2, 4]^T
        int[,] grid = {
            { 2, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 4, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveUp(grid);

        // Then column 0 becomes [4, 4, 0, 0]^T
        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(4, result[1, 0]);
        Assert.Equal(0, result[2, 0]);
        Assert.Equal(0, result[3, 0]);
    }

    [Fact]
    public void MoveUp_WithFourIdenticalTilesInColumn_ProducesTwoMergedTilesAtTop()
    {
        // Given column 0 has [2, 2, 2, 2]^T
        int[,] grid = {
            { 2, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 2, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveUp(grid);

        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(4, result[1, 0]);
        Assert.Equal(0, result[2, 0]);
        Assert.Equal(0, result[3, 0]);
    }

    [Fact]
    public void MoveUp_WhenAlreadyCompactedAtTop_ReturnsDoneFalse()
    {
        int[,] grid = {
            { 2, 0, 0, 0 },
            { 4, 0, 0, 0 },
            { 8, 0, 0, 0 },
            { 16, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveUp(grid);

        Assert.False(done);
        Assert.Equal(2, result[0, 0]);
        Assert.Equal(4, result[1, 0]);
        Assert.Equal(8, result[2, 0]);
        Assert.Equal(16, result[3, 0]);
    }

    [Fact]
    public void MoveDown_WithVerticalGapsAndEqualTiles_CompressesAndMergesDownward()
    {
        // Given column 0 has [4, 2, 0, 2]^T
        int[,] grid = {
            { 4, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 2, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveDown(grid);

        // Then column 0 becomes [0, 0, 4, 4]^T
        Assert.True(done);
        Assert.Equal(0, result[0, 0]);
        Assert.Equal(0, result[1, 0]);
        Assert.Equal(4, result[2, 0]);
        Assert.Equal(4, result[3, 0]);
    }

    [Fact]
    public void MoveDown_WithFourIdenticalTilesInColumn_ProducesTwoMergedTilesAtBottom()
    {
        int[,] grid = {
            { 2, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 2, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveDown(grid);

        Assert.True(done);
        Assert.Equal(0, result[0, 0]);
        Assert.Equal(0, result[1, 0]);
        Assert.Equal(4, result[2, 0]);
        Assert.Equal(4, result[3, 0]);
    }

    [Fact]
    public void MoveDown_WhenAlreadyCompactedAtBottom_ReturnsDoneFalse()
    {
        int[,] grid = {
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 2, 0, 0, 0 },
            { 4, 0, 0, 0 }
        };

        var (result, done) = GameEngine.MoveDown(grid);

        Assert.False(done);
        Assert.Equal(2, result[2, 0]);
        Assert.Equal(4, result[3, 0]);
    }

    [Fact]
    public void Move_AcrossMultipleRowsAndColumns_ExecutesIndependently()
    {
        // Given full board with different rows
        int[,] grid = {
            { 2, 2, 0, 0 },  // will merge -> [4, 0, 0, 0]
            { 4, 4, 4, 4 },  // will merge -> [8, 8, 0, 0]
            { 2, 4, 8, 16 }, // no merge   -> [2, 4, 8, 16]
            { 0, 0, 2, 0 }   // shift only -> [2, 0, 0, 0]
        };

        var (result, done) = GameEngine.MoveLeft(grid);

        Assert.True(done);
        Assert.Equal(4, result[0, 0]); Assert.Equal(0, result[0, 1]);
        Assert.Equal(8, result[1, 0]); Assert.Equal(8, result[1, 1]); Assert.Equal(0, result[1, 2]); Assert.Equal(0, result[1, 3]);
        Assert.Equal(2, result[2, 0]); Assert.Equal(4, result[2, 1]); Assert.Equal(8, result[2, 2]); Assert.Equal(16, result[2, 3]);
        Assert.Equal(2, result[3, 0]); Assert.Equal(0, result[3, 1]);
    }
}
