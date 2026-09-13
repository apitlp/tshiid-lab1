using Game2048.Models;
using Xunit;

namespace Game2048.Tests.CoreLogic;

public class MergeTests
{
    [Fact]
    public void Merge_WithNoEqualAdjacentTiles_ReturnsUnchangedAndDoneUnmodified()
    {
        // Given a row with all distinct numbers
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called with done=false
        var (result, done) = GameEngine.Merge(grid, false);

        // Then no tiles merge and done remains false
        Assert.False(done);
        Assert.Equal(2, result[0, 0]);
        Assert.Equal(4, result[0, 1]);
        Assert.Equal(8, result[0, 2]);
        Assert.Equal(16, result[0, 3]);
    }

    [Fact]
    public void Merge_WithSinglePair_CombinesValuesAndSetsDoneTrue()
    {
        // Given a single pair of equal adjacent tiles
        int[,] grid = {
            { 2, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then tiles combine to 4 and trailing tile is cleared to 0
        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
    }

    [Fact]
    public void Merge_WithTwoDistinctPairs_MergesBothPairsInSinglePass()
    {
        // Given two distinct pairs: [2, 2, 4, 4]
        int[,] grid = {
            { 2, 2, 4, 4 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then both pairs merge into [4, 0, 8, 0]
        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(8, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void Merge_WithFourIdenticalTiles_MergesIntoTwoDoubledTilesWithoutCascading()
    {
        // Given four identical tiles: [2, 2, 2, 2]
        // 2048 rule: a merged tile cannot merge again in the same move
        int[,] grid = {
            { 2, 2, 2, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then pair 0+1 becomes 4, and pair 2+3 becomes 4 (NOT 8)
        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(4, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void Merge_WithThreeIdenticalTilesAtLeft_MergesFirstPairOnly()
    {
        // Given three identical tiles on the left: [2, 2, 2, 0]
        int[,] grid = {
            { 2, 2, 2, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then indices 0 and 1 merge to 4; index 2 remains 2
        Assert.True(done);
        Assert.Equal(4, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(2, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void Merge_WithZerosBetweenIdenticalTiles_DoesNotMergeAcrossZeroGap()
    {
        // Given identical tiles separated by a 0: [2, 0, 2, 0]
        // Raw Merge only checks adjacent cells; non-adjacent equal tiles do not merge
        int[,] grid = {
            { 2, 0, 2, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then no merge happens
        Assert.False(done);
        Assert.Equal(2, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(2, result[0, 2]);
        Assert.Equal(0, result[0, 3]);
    }

    [Fact]
    public void Merge_WithAdjacentZeros_DoesNotMergeZeros()
    {
        // Given adjacent 0s
        int[,] grid = {
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then 0s never merge and done remains false
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
    public void Merge_WhenIncomingDoneIsTrue_PreservesDoneFlagEvenIfNoNewMergesOccurred()
    {
        // Given incoming done = true from a preceding CoverUp pass
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called with done = true
        var (_, done) = GameEngine.Merge(grid, true);

        // Then done remains true
        Assert.True(done);
    }

    [Fact]
    public void Merge_WithLargeTileValues_MergesCorrectlyWithoutOverflow()
    {
        // Given large tiles: 16384 + 16384
        int[,] grid = {
            { 16384, 16384, 0, 0 },
            { 32768, 32768, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        // When Merge is called
        var (result, done) = GameEngine.Merge(grid, false);

        // Then merges produce 32768 and 65536
        Assert.True(done);
        Assert.Equal(32768, result[0, 0]);
        Assert.Equal(0, result[0, 1]);
        Assert.Equal(65536, result[1, 0]);
        Assert.Equal(0, result[1, 1]);
    }
}
