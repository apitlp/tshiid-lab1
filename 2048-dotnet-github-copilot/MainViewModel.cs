using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Game2048;

public sealed class TileViewModel : INotifyPropertyChanged
{
    private int value;

    public int Value
    {
        get => value;
        set
        {
            if (this.value == value)
            {
                return;
            }

            this.value = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayValue));
            OnPropertyChanged(nameof(Background));
            OnPropertyChanged(nameof(Foreground));
        }
    }

    public string DisplayValue => Value == 0 ? string.Empty : Value.ToString();

    public string Background => Value switch
    {
        0 => "#9e948a",
        2 => "#eee4da",
        4 => "#ede0c8",
        8 => "#f2b179",
        16 => "#f59563",
        32 => "#f67c5f",
        64 => "#f65e3b",
        128 => "#edcf72",
        256 => "#edcc61",
        512 => "#edc850",
        1024 => "#edc53f",
        2048 => "#edc22e",
        _ => "#edc22e"
    };

    public string Foreground => Value is 2 or 4 ? "#776e65" : "#f9f6f2";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly List<GameBoard> history = [];
    private GameBoard? board;
    private string status = string.Empty;
    private string gridSizeInput = "4";
    private string inputError = string.Empty;
    private bool isGameVisible;
    private bool isPaused;

    public MainViewModel()
    {
        UpCommand = new RelayCommand(() => Move(Direction.Up), CanPlay);
        DownCommand = new RelayCommand(() => Move(Direction.Down), CanPlay);
        LeftCommand = new RelayCommand(() => Move(Direction.Left), CanPlay);
        RightCommand = new RelayCommand(() => Move(Direction.Right), CanPlay);
        UndoCommand = new RelayCommand(Undo, () => IsGameVisible && history.Count > 1);
        StartCommand = new RelayCommand(StartGame);
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        TogglePauseCommand = new RelayCommand(TogglePause, () => IsGameVisible);
        ResumeCommand = new RelayCommand(ResumeGame, () => IsPaused);
        ExitToMenuCommand = new RelayCommand(ExitToMenu, () => IsPaused);
    }

    public ObservableCollection<TileViewModel> Tiles { get; } = [];

    public string GridSizeInput
    {
        get => gridSizeInput;
        set
        {
            if (gridSizeInput == value)
            {
                return;
            }

            gridSizeInput = value;
            InputError = string.Empty;
            OnPropertyChanged();
        }
    }

    public string InputError
    {
        get => inputError;
        private set
        {
            if (inputError == value)
            {
                return;
            }

            inputError = value;
            OnPropertyChanged();
        }
    }

    public bool IsMenuVisible => !IsGameVisible;

    public bool IsPauseVisible => IsGameVisible && IsPaused;

    public int BoardSize => board?.Size ?? 4;

    public bool IsGameVisible
    {
        get => isGameVisible;
        private set
        {
            if (isGameVisible == value)
            {
                return;
            }

            isGameVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsMenuVisible));
            OnPropertyChanged(nameof(BoardSize));
            RefreshCommands();
        }
    }

    public bool IsPaused
    {
        get => isPaused;
        private set
        {
            if (isPaused == value)
            {
                return;
            }

            isPaused = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPauseVisible));
            RefreshCommands();
        }
    }

    public string Status
    {
        get => status;
        private set
        {
            if (status == value)
            {
                return;
            }

            status = value;
            OnPropertyChanged();
        }
    }

    public ICommand UpCommand { get; }
    public ICommand DownCommand { get; }
    public ICommand LeftCommand { get; }
    public ICommand RightCommand { get; }
    public RelayCommand UndoCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand ExitCommand { get; }
    public RelayCommand TogglePauseCommand { get; }
    public RelayCommand ResumeCommand { get; }
    public RelayCommand ExitToMenuCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void StartGame()
    {
        if (!int.TryParse(GridSizeInput, out var size) || size is < 2 or > 10)
        {
            InputError = "Enter a whole number from 2 to 10.";
            return;
        }

        board = new GameBoard(size);
        history.Clear();
        history.Add(board.Clone());
        Tiles.Clear();
        for (var index = 0; index < size * size; index++)
        {
            Tiles.Add(new TileViewModel());
        }

        Status = string.Empty;
        InputError = string.Empty;
        IsPaused = false;
        IsGameVisible = true;
        RefreshTiles();
    }

    private void Move(Direction direction)
    {
        if (board is null || !board.Move(direction))
        {
            return;
        }

        history.Add(board.Clone());
        RefreshTiles();
        Status = board.GetState() switch
        {
            GameState.Won => "You Win!",
            GameState.Lost => "You Lose!",
            _ => string.Empty
        };
        RefreshCommands();
    }

    private void Undo()
    {
        if (history.Count <= 1 || board is null)
        {
            return;
        }

        history.RemoveAt(history.Count - 1);
        board = history[^1].Clone();
        Status = string.Empty;
        RefreshTiles();
        RefreshCommands();
    }

    private void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    private void ResumeGame()
    {
        IsPaused = false;
    }

    private void ExitToMenu()
    {
        IsPaused = false;
        IsGameVisible = false;
        board = null;
        history.Clear();
        Tiles.Clear();
        Status = string.Empty;
    }

    private bool CanPlay() => IsGameVisible && !IsPaused;

    private void RefreshCommands()
    {
        ((RelayCommand)UpCommand).Refresh();
        ((RelayCommand)DownCommand).Refresh();
        ((RelayCommand)LeftCommand).Refresh();
        ((RelayCommand)RightCommand).Refresh();
        UndoCommand.Refresh();
        TogglePauseCommand.Refresh();
        ResumeCommand.Refresh();
        ExitToMenuCommand.Refresh();
    }

    private void RefreshTiles()
    {
        if (board is null)
        {
            return;
        }

        for (var row = 0; row < board.Size; row++)
        {
            for (var column = 0; column < board.Size; column++)
            {
                Tiles[row * board.Size + column].Value = board[row, column];
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}