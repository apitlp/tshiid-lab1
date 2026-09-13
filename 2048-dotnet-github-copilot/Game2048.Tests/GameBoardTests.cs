namespace Game2048.Tests;

public sealed class GameBoardTests
{
    [Fact]
    public void Constructor_WithValidSize_CreatesBoardWithTwoTilesOfTwo()
    {
        var board = new GameBoard(4, new Random(1));

        Assert.Equal(4, board.Size);
        Assert.Equal(2, board.Snapshot().Cast<int>().Count(value => value == 2));
        Assert.Equal(14, board.Snapshot().Cast<int>().Count(value => value == 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-4)]
    public void Constructor_WithSizeLessThanTwo_ThrowsArgumentOutOfRangeException(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameBoard(size));
    }

    [Fact]
    public void MoveLeft_WithSeparatedEqualTiles_CompactsAndMergesTowardLeft()
    {
        var board = BoardFromRows(
            [2, 0, 2, 0],
            [0, 4, 0, 4],
            [2, 2, 2, 2],
            [0, 0, 0, 0]);

        var changed = board.Move(Direction.Left);

        Assert.True(changed);
        Assert.Equal([4, 0, 0], [board[0, 0], board[0, 1], board[0, 2]]);
        Assert.Equal([8, 0, 0], [board[1, 0], board[1, 1], board[1, 2]]);
        Assert.Equal([4, 4, 0], [board[2, 0], board[2, 1], board[2, 2]]);
    }

    [Fact]
    public void MoveRight_WithThreeEqualTiles_MergesOnlyAdjacentPairFromMovementEdge()
    {
        var board = BoardFromRows(
            [2, 2, 2, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0]);

        var changed = board.Move(Direction.Right);

        Assert.True(changed);
        Assert.Equal(2, board[0, 2]);
        Assert.Equal(4, board[0, 3]);
        Assert.Equal(2, board.Snapshot().Cast<int>().Count(value => value == 2));
    }

    [Fact]
    public void MoveUp_WithVerticalTiles_MergesEachColumnIndependently()
    {
        var board = BoardFromRows(
            [2, 0, 4, 0],
            [2, 0, 0, 0],
            [0, 0, 4, 0],
            [0, 0, 0, 0]);

        var changed = board.Move(Direction.Up);

        Assert.True(changed);
        Assert.Equal([4, 0, 8, 0], [board[0, 0], board[0, 1], board[0, 2], board[0, 3]]);
    }

    [Fact]
    public void MoveDown_WithVerticalTiles_MergesTowardBottom()
    {
        var board = BoardFromRows(
            [2, 0, 0, 0],
            [2, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0]);

        var changed = board.Move(Direction.Down);

        Assert.True(changed);
        Assert.Equal([4, 0, 0, 0], [board[3, 0], board[3, 1], board[3, 2], board[3, 3]]);
    }

    [Fact]
    public void Move_WhenBoardCannotChange_ReturnsFalseAndDoesNotAddTile()
    {
        var board = BoardFromRows(
            [2, 4, 8, 16],
            [32, 64, 128, 256],
            [512, 1024, 2, 4],
            [8, 16, 32, 64]);
        var before = board.Snapshot();

        var changed = board.Move(Direction.Left);

        Assert.False(changed);
        Assert.Equal(before, board.Snapshot());
    }

    [Fact]
    public void Move_WhenBoardChanges_AddsExactlyOneTwoTile()
    {
        var board = BoardFromRows(
            [0, 2, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0]);
        var before = board.Snapshot();

        board.Move(Direction.Left);

        Assert.Equal(2, board.Snapshot().Cast<int>().Count(value => value == 2));
        Assert.Equal(1, before.Cast<int>().Count(value => value == 2));
    }

    [Fact]
    public void GetState_With2048AndEmptyCells_ReturnsWon()
    {
        var board = BoardFromRows([2048, 0, 0, 0]);

        Assert.Equal(GameState.Won, board.GetState());
    }

    [Fact]
    public void GetState_WithEmptyCell_ReturnsPlaying()
    {
        var board = BoardFromRows(
            [2, 4, 8, 16],
            [32, 64, 128, 256],
            [512, 1024, 2, 4],
            [8, 16, 32, 0]);

        Assert.Equal(GameState.Playing, board.GetState());
    }

    [Fact]
    public void GetState_WithFullBoardAndAdjacentEqualTiles_ReturnsPlaying()
    {
        var board = BoardFromRows(
            [2, 2, 4, 8],
            [16, 32, 64, 128],
            [256, 512, 1024, 4],
            [8, 16, 32, 64]);

        Assert.Equal(GameState.Playing, board.GetState());
    }

    [Fact]
    public void GetState_WithFullBoardAndNoPossibleMerge_ReturnsLost()
    {
        var board = BoardFromRows(
            [2, 4, 8, 16],
            [32, 64, 128, 256],
            [512, 1024, 2, 4],
            [8, 16, 32, 64]);

        Assert.Equal(GameState.Lost, board.GetState());
    }

    [Fact]
    public void Clone_CopiesBoardStateWithoutSharingMutableStorage()
    {
        var board = BoardFromRows(
            [2, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0]);
        var clone = board.Clone();

        clone.Move(Direction.Right);

        Assert.Equal(2, board[0, 0]);
        Assert.Equal(0, board[0, 3]);
        Assert.Equal(2, clone[0, 3]);
    }

    [Fact]
    public void Constructor_WithCustomSize_CreatesCorrectNumberOfCells()
    {
        var board = new GameBoard(6, new Random(1));

        Assert.Equal(6, board.Size);
        Assert.Equal(36, board.Snapshot().Length);
        Assert.Equal(2, board.Snapshot().Cast<int>().Count(value => value == 2));
    }

    private static GameBoard BoardFromRows(params int[][] rows)
    {
        var size = rows.Length == 4 && rows.All(row => row.Length == 4) ? 4 : rows.Length;
        var snapshot = new int[size, size];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                snapshot[row, column] = rows[row][column];
            }
        }

        return new GameBoard(size, snapshot, new Random(1));
    }

}