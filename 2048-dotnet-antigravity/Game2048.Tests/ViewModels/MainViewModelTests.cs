using Game2048.Models;
using Game2048.ViewModels;
using Xunit;

namespace Game2048.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithStartMenuVisibleAndDefaultGridSize4()
    {
        var vm = new MainViewModel();

        Assert.True(vm.IsMenuVisible);
        Assert.False(vm.IsGameVisible);
        Assert.False(vm.IsPaused);
        Assert.Equal("4", vm.GridSizeInput);
        Assert.Equal(4, vm.GridSize);
        Assert.Equal(16, vm.Tiles.Count);
        Assert.Null(vm.ErrorMessage);
    }

    [Theory]
    [InlineData("2", 2)]
    [InlineData("3", 3)]
    [InlineData("4", 4)]
    [InlineData("5", 5)]
    [InlineData("10", 10)]
    [InlineData("  6  ", 6)]
    public void ExecuteStartGame_WithValidGridSize_SetsBoardAndHidesMenu(string input, int expectedSize)
    {
        var vm = new MainViewModel
        {
            GridSizeInput = input
        };

        vm.ExecuteStartGame();

        Assert.False(vm.IsMenuVisible);
        Assert.True(vm.IsGameVisible);
        Assert.False(vm.IsPaused);
        Assert.Equal(expectedSize, vm.GridSize);
        Assert.Equal(expectedSize * expectedSize, vm.Tiles.Count);
        Assert.Null(vm.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ExecuteStartGame_WithEmptyInput_SetsErrorMessageAndKeepsMenu(string input)
    {
        var vm = new MainViewModel
        {
            GridSizeInput = input
        };

        vm.ExecuteStartGame();

        Assert.True(vm.IsMenuVisible);
        Assert.NotNull(vm.ErrorMessage);
        Assert.Contains("Please enter a grid size", vm.ErrorMessage);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("four")]
    [InlineData("4.5")]
    [InlineData("2,5")]
    [InlineData("999999999999999999999999999999999999")]
    public void ExecuteStartGame_WithNonNumericInput_SetsErrorMessageAndKeepsMenu(string input)
    {
        var vm = new MainViewModel
        {
            GridSizeInput = input
        };

        vm.ExecuteStartGame();

        Assert.True(vm.IsMenuVisible);
        Assert.NotNull(vm.ErrorMessage);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("11")]
    [InlineData("100")]
    public void ExecuteStartGame_WithOutOfRangeInput_SetsErrorMessageAndKeepsMenu(string input)
    {
        var vm = new MainViewModel
        {
            GridSizeInput = input
        };

        vm.ExecuteStartGame();

        Assert.True(vm.IsMenuVisible);
        Assert.NotNull(vm.ErrorMessage);
        Assert.Contains("Grid size must be between 2 and 10", vm.ErrorMessage);
    }

    [Fact]
    public void ExecutePause_DuringActiveGame_SetsIsPausedTrue()
    {
        var vm = new MainViewModel();
        vm.ExecuteStartGame();
        Assert.False(vm.IsPaused);

        vm.ExecutePause();

        Assert.True(vm.IsPaused);
    }

    [Fact]
    public void ExecutePause_WhileInStartMenu_DoesNotPause()
    {
        var vm = new MainViewModel();
        Assert.True(vm.IsMenuVisible);

        vm.ExecutePause();

        Assert.False(vm.IsPaused);
    }

    [Fact]
    public void ExecuteResume_WhenPaused_ResumesGameplay()
    {
        var vm = new MainViewModel();
        vm.ExecuteStartGame();
        vm.ExecutePause();
        Assert.True(vm.IsPaused);

        vm.ExecuteResume();

        Assert.False(vm.IsPaused);
        Assert.False(vm.IsMenuVisible);
    }

    [Fact]
    public void ExecuteExitToMenu_WhenPaused_ResetsPauseAndShowsStartMenu()
    {
        var vm = new MainViewModel();
        vm.ExecuteStartGame();
        vm.ExecutePause();
        Assert.True(vm.IsPaused);

        vm.ExecuteExitToMenu();

        Assert.False(vm.IsPaused);
        Assert.True(vm.IsMenuVisible);
    }

    [Fact]
    public void ExecuteMove_WhilePaused_IsIgnored()
    {
        var vm = new MainViewModel();
        vm.ExecuteStartGame();
        vm.ExecutePause();

        // Record tile values
        int[] originalValues = new int[vm.Tiles.Count];
        for (int i = 0; i < vm.Tiles.Count; i++)
            originalValues[i] = vm.Tiles[i].Value;

        vm.ExecuteMove(Direction.Up);
        vm.ExecuteMove(Direction.Down);
        vm.ExecuteMove(Direction.Left);
        vm.ExecuteMove(Direction.Right);

        for (int i = 0; i < vm.Tiles.Count; i++)
            Assert.Equal(originalValues[i], vm.Tiles[i].Value);
    }

    [Fact]
    public void ExecuteUndo_WhilePaused_IsIgnored()
    {
        var vm = new MainViewModel();
        vm.ExecuteStartGame();

        // Make one valid move while unpaused
        vm.ExecuteMove(Direction.Down);
        vm.ExecuteMove(Direction.Right);

        // Now pause
        vm.ExecutePause();

        int[] valuesWhenPaused = new int[vm.Tiles.Count];
        for (int i = 0; i < vm.Tiles.Count; i++)
            valuesWhenPaused[i] = vm.Tiles[i].Value;

        vm.ExecuteUndo();

        for (int i = 0; i < vm.Tiles.Count; i++)
            Assert.Equal(valuesWhenPaused[i], vm.Tiles[i].Value);
    }

    [Fact]
    public void UpdateBoard_WhenGameWon_DisplaysYouAndWinCells()
    {
        // Board with 2048 tile to trigger Win state
        int[,] winningGrid = {
            { 2, 4, 8, 16 },
            { 32, 2048, 128, 256 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var engine = new GameEngine(winningGrid);
        var vm = new MainViewModel(engine, isMenuVisible: false);

        // Win state should configure cell (1, 1) to "You" and cell (1, 2) to "Win!"
        var youCell = vm.Tiles[1 * 4 + 1];
        var winCell = vm.Tiles[1 * 4 + 2];

        Assert.Equal(GameState.Win, vm.GameState);
        Assert.Equal("You", youCell.DisplayText);
        Assert.Equal("Win!", winCell.DisplayText);
    }

    [Fact]
    public void UpdateBoard_WhenGameLost_DisplaysYouAndLoseCells()
    {
        int[,] loseGrid = {
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 },
            { 2, 4, 2, 4 },
            { 4, 2, 4, 2 }
        };

        var engine = new GameEngine(loseGrid);
        var vm = new MainViewModel(engine, isMenuVisible: false);

        var youCell = vm.Tiles[1 * 4 + 1];
        var loseCell = vm.Tiles[1 * 4 + 2];

        Assert.Equal(GameState.Lose, vm.GameState);
        Assert.Equal("You", youCell.DisplayText);
        Assert.Equal("Lose!", loseCell.DisplayText);
    }

    [Fact]
    public void ExecuteNewGame_ResetsBoard()
    {
        var vm = new MainViewModel();
        vm.ExecuteStartGame();

        vm.ExecuteMove(Direction.Down);
        vm.ExecuteMove(Direction.Right);

        vm.ExecuteNewGame();

        Assert.Equal(GameState.NotOver, vm.GameState);
        Assert.False(vm.UndoCommand.CanExecute(null));
    }
}
