namespace Game2048.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public void InitialState_ShowsStartingMenuWithDefaultGridSize()
    {
        var viewModel = new MainViewModel();

        Assert.True(viewModel.IsMenuVisible);
        Assert.False(viewModel.IsGameVisible);
        Assert.False(viewModel.IsPauseVisible);
        Assert.Equal("4", viewModel.GridSizeInput);
        Assert.Empty(viewModel.Tiles);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1")]
    [InlineData("11")]
    public void Start_WithInvalidGridSize_StaysOnMenuAndReportsValidationError(string input)
    {
        var viewModel = new MainViewModel
        {
            GridSizeInput = input
        };

        viewModel.StartCommand.Execute(null);

        Assert.True(viewModel.IsMenuVisible);
        Assert.False(viewModel.IsGameVisible);
        Assert.Equal("Enter a whole number from 2 to 10.", viewModel.InputError);
        Assert.Empty(viewModel.Tiles);
    }

    [Theory]
    [InlineData("2", 4)]
    [InlineData("4", 16)]
    [InlineData("10", 100)]
    public void Start_WithValidGridSize_ShowsGameWithRequestedNumberOfTiles(string input, int tileCount)
    {
        var viewModel = new MainViewModel
        {
            GridSizeInput = input
        };

        viewModel.StartCommand.Execute(null);

        Assert.False(viewModel.IsMenuVisible);
        Assert.True(viewModel.IsGameVisible);
        Assert.False(viewModel.IsPaused);
        Assert.Equal(int.Parse(input), viewModel.BoardSize);
        Assert.Equal(tileCount, viewModel.Tiles.Count);
        Assert.Equal(string.Empty, viewModel.InputError);
    }

    [Fact]
    public void EscapeCommand_DuringGame_ShowsPauseMenuAndDisablesGameplayCommands()
    {
        var viewModel = StartedGame();

        viewModel.TogglePauseCommand.Execute(null);

        Assert.True(viewModel.IsPaused);
        Assert.True(viewModel.IsPauseVisible);
        Assert.False(viewModel.UpCommand.CanExecute(null));
        Assert.False(viewModel.DownCommand.CanExecute(null));
        Assert.False(viewModel.LeftCommand.CanExecute(null));
        Assert.False(viewModel.RightCommand.CanExecute(null));
        Assert.False(viewModel.UndoCommand.CanExecute(null));
        Assert.True(viewModel.ResumeCommand.CanExecute(null));
        Assert.True(viewModel.ExitToMenuCommand.CanExecute(null));
    }

    [Fact]
    public void ResumeCommand_WhenPaused_HidesPauseMenuAndReEnablesGameplay()
    {
        var viewModel = StartedGame();
        viewModel.TogglePauseCommand.Execute(null);

        viewModel.ResumeCommand.Execute(null);

        Assert.False(viewModel.IsPaused);
        Assert.False(viewModel.IsPauseVisible);
        Assert.True(viewModel.UpCommand.CanExecute(null));
        Assert.False(viewModel.ResumeCommand.CanExecute(null));
    }

    [Fact]
    public void ExitToMenuCommand_WhenPaused_ReturnsToStartingMenuAndClearsBoard()
    {
        var viewModel = StartedGame();
        viewModel.TogglePauseCommand.Execute(null);

        viewModel.ExitToMenuCommand.Execute(null);

        Assert.True(viewModel.IsMenuVisible);
        Assert.False(viewModel.IsGameVisible);
        Assert.False(viewModel.IsPaused);
        Assert.False(viewModel.IsPauseVisible);
        Assert.Empty(viewModel.Tiles);
    }

    private static MainViewModel StartedGame()
    {
        var viewModel = new MainViewModel();
        viewModel.StartCommand.Execute(null);
        return viewModel;
    }
}