namespace Game2048;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum GameState
{
    Playing,
    Won,
    Lost
}

public sealed class GameBoard
{
    private readonly int[,] cells;
    private readonly Random random;

    public GameBoard(int size, Random? random = null)
    {
        if (size < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "The board size must be at least 2.");
        }

        Size = size;
        cells = new int[Size, Size];
        this.random = random ?? Random.Shared;
        AddTwo();
        AddTwo();
    }

    private GameBoard(int size, int[,] snapshot, Random random)
    {
        Size = size;
        cells = new int[Size, Size];
        this.random = random;
        Array.Copy(snapshot, cells, snapshot.Length);
    }

    public int Size { get; }

    public int this[int row, int column] => cells[row, column];

    public GameBoard Clone() => new(Size, cells, random);

    public int[,] Snapshot()
    {
        var snapshot = new int[Size, Size];
        Array.Copy(cells, snapshot, cells.Length);
        return snapshot;
    }

    public bool Move(Direction direction)
    {
        var changed = false;

        for (var line = 0; line < Size; line++)
        {
            var values = ReadLine(direction, line);
            var compacted = values.Where(value => value != 0).ToList();
            var merged = new List<int>(Size);

            for (var index = 0; index < compacted.Count; index++)
            {
                if (index + 1 < compacted.Count && compacted[index] == compacted[index + 1])
                {
                    merged.Add(compacted[index] * 2);
                    index++;
                }
                else
                {
                    merged.Add(compacted[index]);
                }
            }

            while (merged.Count < Size)
            {
                merged.Add(0);
            }

            for (var position = 0; position < Size; position++)
            {
                if (values[position] != merged[position])
                {
                    changed = true;
                }
            }

            WriteLine(direction, line, merged);
        }

        if (changed)
        {
            AddTwo();
        }

        return changed;
    }

    public GameState GetState()
    {
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (cells[row, column] == 2048)
                {
                    return GameState.Won;
                }
            }
        }

        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (cells[row, column] == 0)
                {
                    return GameState.Playing;
                }
            }
        }

        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (row + 1 < Size && cells[row, column] == cells[row + 1, column] ||
                    column + 1 < Size && cells[row, column] == cells[row, column + 1])
                {
                    return GameState.Playing;
                }
            }
        }

        return GameState.Lost;
    }

    private void AddTwo()
    {
        var emptyCells = new List<(int Row, int Column)>();
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (cells[row, column] == 0)
                {
                    emptyCells.Add((row, column));
                }
            }
        }

        if (emptyCells.Count > 0)
        {
            var cell = emptyCells[random.Next(emptyCells.Count)];
            cells[cell.Row, cell.Column] = 2;
        }
    }

    private int[] ReadLine(Direction direction, int line)
    {
        var values = new int[Size];
        for (var position = 0; position < Size; position++)
        {
            var (row, column) = Coordinates(direction, line, position);
            values[position] = cells[row, column];
        }

        return values;
    }

    private void WriteLine(Direction direction, int line, IReadOnlyList<int> values)
    {
        for (var position = 0; position < Size; position++)
        {
            var (row, column) = Coordinates(direction, line, position);
            cells[row, column] = values[position];
        }
    }

    private (int Row, int Column) Coordinates(Direction direction, int line, int position) =>
        direction switch
        {
            Direction.Left => (line, position),
            Direction.Right => (line, Size - position - 1),
            Direction.Up => (position, line),
            Direction.Down => (Size - position - 1, line),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
}