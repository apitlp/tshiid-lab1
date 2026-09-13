using System;
using System.Collections.Generic;

namespace Game2048.Models;

public class GameEngine
{
    private readonly int _size;
    private readonly Random _random;
    private int[,] _grid;
    private readonly Stack<int[,]> _history = new();

    public int Size => _size;
    public int[,] Grid => (int[,])_grid.Clone();
    public GameState CurrentState => CheckGameState(_grid);
    public bool CanUndo => _history.Count > 0;

    public GameEngine(int size = GameConstants.GridLen, Random? random = null)
    {
        _size = size;
        _random = random ?? new Random();
        _grid = new int[_size, _size];
        StartNewGame();
    }

    internal GameEngine(int[,] initialGrid, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(initialGrid);
        _size = initialGrid.GetLength(0);
        _random = random ?? new Random();
        _grid = (int[,])initialGrid.Clone();
    }

    public void StartNewGame()
    {
        _grid = new int[_size, _size];
        _history.Clear();
        AddTwo(_grid, _random);
        AddTwo(_grid, _random);
    }

    public bool Move(Direction direction)
    {
        var (newGrid, done) = direction switch
        {
            Direction.Up => MoveUp(_grid),
            Direction.Down => MoveDown(_grid),
            Direction.Left => MoveLeft(_grid),
            Direction.Right => MoveRight(_grid),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

        if (!done)
        {
            return false;
        }

        // Save current board for undo
        _history.Push((int[,])_grid.Clone());

        // Place new 2 tile
        AddTwo(newGrid, _random);

        _grid = newGrid;
        return true;
    }

    public bool Undo()
    {
        if (_history.Count == 0)
        {
            return false;
        }

        _grid = _history.Pop();
        return true;
    }

    public static (int[,] Result, bool Done) MoveLeft(int[,] mat)
    {
        var (compressed, done) = CoverUp(mat);
        (compressed, done) = Merge(compressed, done);
        compressed = CoverUp(compressed).Result;
        return (compressed, done);
    }

    public static (int[,] Result, bool Done) MoveRight(int[,] mat)
    {
        var reversed = Reverse(mat);
        var (compressed, done) = CoverUp(reversed);
        (compressed, done) = Merge(compressed, done);
        compressed = CoverUp(compressed).Result;
        return (Reverse(compressed), done);
    }

    public static (int[,] Result, bool Done) MoveUp(int[,] mat)
    {
        var transposed = Transpose(mat);
        var (compressed, done) = CoverUp(transposed);
        (compressed, done) = Merge(compressed, done);
        compressed = CoverUp(compressed).Result;
        return (Transpose(compressed), done);
    }

    public static (int[,] Result, bool Done) MoveDown(int[,] mat)
    {
        var prep = Reverse(Transpose(mat));
        var (compressed, done) = CoverUp(prep);
        (compressed, done) = Merge(compressed, done);
        compressed = CoverUp(compressed).Result;
        return (Transpose(Reverse(compressed)), done);
    }

    public static (int[,] Result, bool Done) CoverUp(int[,] mat)
    {
        int size = mat.GetLength(0);
        int[,] result = new int[size, size];
        bool done = false;

        for (int i = 0; i < size; i++)
        {
            int count = 0;
            for (int j = 0; j < size; j++)
            {
                if (mat[i, j] != 0)
                {
                    result[i, count] = mat[i, j];
                    if (j != count)
                    {
                        done = true;
                    }
                    count++;
                }
            }
        }

        return (result, done);
    }

    public static (int[,] Result, bool Done) Merge(int[,] mat, bool done)
    {
        int size = mat.GetLength(0);
        int[,] result = (int[,])mat.Clone();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                if (result[i, j] == result[i, j + 1] && result[i, j] != 0)
                {
                    result[i, j] *= 2;
                    result[i, j + 1] = 0;
                    done = true;
                }
            }
        }

        return (result, done);
    }

    public static int[,] Transpose(int[,] mat)
    {
        int rows = mat.GetLength(0);
        int cols = mat.GetLength(1);
        int[,] result = new int[cols, rows];

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                result[i, j] = mat[j, i];
            }
        }

        return result;
    }

    public static int[,] Reverse(int[,] mat)
    {
        int rows = mat.GetLength(0);
        int cols = mat.GetLength(1);
        int[,] result = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = mat[i, cols - j - 1];
            }
        }

        return result;
    }

    public static bool AddTwo(int[,] mat, Random? random = null)
    {
        random ??= new Random();
        int size = mat.GetLength(0);
        List<(int Row, int Col)> emptyCells = new();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (mat[i, j] == 0)
                {
                    emptyCells.Add((i, j));
                }
            }
        }

        if (emptyCells.Count == 0)
        {
            return false;
        }

        int index = random.Next(emptyCells.Count);
        var (row, col) = emptyCells[index];
        mat[row, col] = 2;
        return true;
    }

    public static GameState CheckGameState(int[,] mat)
    {
        int size = mat.GetLength(0);

        // Check for 2048 win cell
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (mat[i, j] == 2048)
                {
                    return GameState.Win;
                }
            }
        }

        // Check for any zero entries
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (mat[i, j] == 0)
                {
                    return GameState.NotOver;
                }
            }
        }

        // Check for horizontally or vertically adjacent same cells
        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                if (mat[i, j] == mat[i + 1, j] || mat[i, j] == mat[i, j + 1])
                {
                    return GameState.NotOver;
                }
            }
        }

        for (int k = 0; k < size - 1; k++)
        {
            if (mat[size - 1, k] == mat[size - 1, k + 1])
            {
                return GameState.NotOver;
            }
        }

        for (int j = 0; j < size - 1; j++)
        {
            if (mat[j, size - 1] == mat[j + 1, size - 1])
            {
                return GameState.NotOver;
            }
        }

        return GameState.Lose;
    }
}
