using Game2048.Models;
using Xunit;

namespace Game2048.Tests.CoreLogic;

public class MatrixTransformationTests
{
    [Fact]
    public void Transpose_SquareMatrix_SwapsRowsAndColumns()
    {
        // Given a 4x4 matrix with distinct elements
        int[,] grid = {
            { 1,  2,  3,  4 },
            { 5,  6,  7,  8 },
            { 9,  10, 11, 12 },
            { 13, 14, 15, 16 }
        };

        // When Transposed
        int[,] result = GameEngine.Transpose(grid);

        // Then rows become columns
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(grid[i, j], result[j, i]);
            }
        }
    }

    [Fact]
    public void Transpose_AppliedTwice_RestoresOriginalMatrix()
    {
        int[,] grid = {
            { 2, 4, 8, 16 },
            { 32, 64, 128, 256 },
            { 0, 2, 4, 8 },
            { 1024, 2048, 4096, 8192 }
        };

        int[,] twice = GameEngine.Transpose(GameEngine.Transpose(grid));

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(grid[i, j], twice[i, j]);
            }
        }
    }

    [Fact]
    public void Reverse_HorizontallyFlipsEachRow()
    {
        int[,] grid = {
            { 1, 2, 3, 4 },
            { 5, 6, 7, 8 },
            { 9, 10, 11, 12 },
            { 13, 14, 15, 16 }
        };

        int[,] reversed = GameEngine.Reverse(grid);

        Assert.Equal(4, reversed[0, 0]); Assert.Equal(3, reversed[0, 1]); Assert.Equal(2, reversed[0, 2]); Assert.Equal(1, reversed[0, 3]);
        Assert.Equal(8, reversed[1, 0]); Assert.Equal(7, reversed[1, 1]); Assert.Equal(6, reversed[1, 2]); Assert.Equal(5, reversed[1, 3]);
    }

    [Fact]
    public void Reverse_AppliedTwice_RestoresOriginalMatrix()
    {
        int[,] grid = {
            { 2, 0, 4, 8 },
            { 16, 32, 64, 128 },
            { 0, 0, 2, 2 },
            { 512, 1024, 2048, 4096 }
        };

        int[,] twice = GameEngine.Reverse(GameEngine.Reverse(grid));

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(grid[i, j], twice[i, j]);
            }
        }
    }

    [Fact]
    public void MoveDownTransforms_FormExactMathematicalInverse()
    {
        // Down move in logic.py is: prep = Reverse(Transpose(mat)), then later Transpose(Reverse(result))
        int[,] original = {
            { 1,  2,  3,  4 },
            { 5,  6,  7,  8 },
            { 9,  10, 11, 12 },
            { 13, 14, 15, 16 }
        };

        int[,] prep = GameEngine.Reverse(GameEngine.Transpose(original));
        int[,] restored = GameEngine.Transpose(GameEngine.Reverse(prep));

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(original[i, j], restored[i, j]);
            }
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void Transformations_SupportArbitraryGridSizes(int size)
    {
        int[,] grid = new int[size, size];
        int val = 1;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                grid[i, j] = val++;

        int[,] transposedTwice = GameEngine.Transpose(GameEngine.Transpose(grid));
        int[,] reversedTwice = GameEngine.Reverse(GameEngine.Reverse(grid));

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Assert.Equal(grid[i, j], transposedTwice[i, j]);
                Assert.Equal(grid[i, j], reversedTwice[i, j]);
            }
        }
    }
}
