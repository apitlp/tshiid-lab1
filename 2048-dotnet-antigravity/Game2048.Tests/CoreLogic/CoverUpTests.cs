using Game2048.Models;
using Xunit;

namespace Game2048.Tests.CoreLogic;

public class CoverUpTests
{
    [Fact]
    public void CoverUp_WithAllZeroRow_ReturnsAllZerosAndDoneFalse()
    {
        // Given an entirely empty 4x4 board
        int[,] grid = {
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When CoverUp is executed
        var (result, done) = GameEngine.CoverUp(grid);

        // Then no tiles move and done is false
        Assert.False(done);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(0, result[i, j]);
            }
        }
    }

    [Fact]
    public void CoverUp_WithAlreadyCompressedRow_ReturnsUnchangedAndDoneFalse()
    {
        // Given a row where all non-zero tiles are already on the left
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 2, 4, 8, 0 },
            { 2, 4, 0, 0 },
            { 2, 0, 0, 0 }
        };

        // When CoverUp is executed
        var (result, done) = GameEngine.CoverUp(grid);

        // Then no tiles changed position and done is false
        Assert.False(done);
        Assert.Equal(2, result[0, 0]); Assert.Equal(4, result[0, 1]); Assert.Equal(8, result[0, 2]); Assert.Equal(16, result[0, 3]);
        Assert.Equal(2, result[1, 0]); Assert.Equal(4, result[1, 1]); Assert.Equal(8, result[1, 2]); Assert.Equal(0, result[1, 3]);
        Assert.Equal(2, result[2, 0]); Assert.Equal(4, result[2, 1]); Assert.Equal(0, result[2, 2]); Assert.Equal(0, result[2, 3]);
        Assert.Equal(2, result[3, 0]); Assert.Equal(0, result[3, 1]); Assert.Equal(0, result[3, 2]); Assert.Equal(0, result[3, 3]);
    }

    [Fact]
    public void CoverUp_WithLeadingZeros_ShiftsTilesToLeftAndSetsDoneTrue()
    {
        // Given rows with leading empty cells
        int[,] grid = {
            { 0, 0, 2, 4 },
            { 0, 2, 4, 8 },
            { 0, 0, 0, 2 },
            { 0, 0, 0, 0 }
        };

        // When CoverUp is executed
        var (result, done) = GameEngine.CoverUp(grid);

        // Then tiles shift left into leading slots and done is true
        Assert.True(done);
        Assert.Equal(2, result[0, 0]); Assert.Equal(4, result[0, 1]); Assert.Equal(0, result[0, 2]); Assert.Equal(0, result[0, 3]);
        Assert.Equal(2, result[1, 0]); Assert.Equal(4, result[1, 1]); Assert.Equal(8, result[1, 2]); Assert.Equal(0, result[1, 3]);
        Assert.Equal(2, result[2, 0]); Assert.Equal(0, result[2, 1]); Assert.Equal(0, result[2, 2]); Assert.Equal(0, result[2, 3]);
        Assert.Equal(0, result[3, 0]); Assert.Equal(0, result[3, 1]); Assert.Equal(0, result[3, 2]); Assert.Equal(0, result[3, 3]);
    }

    [Fact]
    public void CoverUp_WithAlternatingZeros_PacksTilesToLeftAndSetsDoneTrue()
    {
        // Given alternating tile and zero patterns
        int[,] grid = {
            { 2, 0, 4, 0 },
            { 0, 2, 0, 4 },
            { 2, 0, 0, 4 },
            { 0, 2, 4, 0 }
        };

        // When CoverUp is executed
        var (result, done) = GameEngine.CoverUp(grid);

        // Then non-zero elements preserve relative order and are packed to the left
        Assert.True(done);
        Assert.Equal(2, result[0, 0]); Assert.Equal(4, result[0, 1]); Assert.Equal(0, result[0, 2]); Assert.Equal(0, result[0, 3]);
        Assert.Equal(2, result[1, 0]); Assert.Equal(4, result[1, 1]); Assert.Equal(0, result[1, 2]); Assert.Equal(0, result[1, 3]);
        Assert.Equal(2, result[2, 0]); Assert.Equal(4, result[2, 1]); Assert.Equal(0, result[2, 2]); Assert.Equal(0, result[2, 3]);
        Assert.Equal(2, result[3, 0]); Assert.Equal(4, result[3, 1]); Assert.Equal(0, result[3, 2]); Assert.Equal(0, result[3, 3]);
    }

    [Fact]
    public void CoverUp_WithSingleTileAtFarRight_MovesToFirstPositionAndSetsDoneTrue()
    {
        // Given single tile at index (0, 3)
        int[,] grid = {
            { 0, 0, 0, 8 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When CoverUp is executed
        var (result, done) = GameEngine.CoverUp(grid);

        // Then tile lands at (0, 0) and done is true
        Assert.True(done);
        Assert.Equal(8, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(0, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void CoverUp_WithArbitraryGridSize_CompressesCorrectly(int size)
    {
        // Given an NxN grid with a single tile at the end of each row
        int[,] grid = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            grid[i, size - 1] = 2;
        }

        // When CoverUp is executed
        var (result, done) = GameEngine.CoverUp(grid);

        // Then tiles move to column 0 and done is true
        Assert.True(done);
        for (int i = 0; i < size; i++)
        {
            Assert.Equal(2, result[i, 0]);
            for (int j = 1; j < size; j++)
            {
                Assert.Equal(0, result[i, j]);
            }
        }
    }
}
