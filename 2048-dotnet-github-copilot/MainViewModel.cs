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
    private GameBoard board = new();
    private string status = string.Empty;

    public MainViewModel()
    {
        for (var index = 0; index < GameBoard.Size * GameBoard.Size; index++)
        {
            Tiles.Add(new TileViewModel());
        }

        UpCommand = new RelayCommand(() => Move(Direction.Up));
        DownCommand = new RelayCommand(() => Move(Direction.Down));
        LeftCommand = new RelayCommand(() => Move(Direction.Left));
        RightCommand = new RelayCommand(() => Move(Direction.Right));
        UndoCommand = new RelayCommand(Undo, () => history.Count > 1);
        QuitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
        RefreshTiles();
    }

    public ObservableCollection<TileViewModel> Tiles { get; } = [];

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
    public ICommand QuitCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Move(Direction direction)
    {
        if (!board.Move(direction))
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
        UndoCommand.Refresh();
    }

    private void Undo()
    {
        if (history.Count <= 1)
        {
            return;
        }

        history.RemoveAt(history.Count - 1);
        board = history[^1].Clone();
        Status = string.Empty;
        RefreshTiles();
        UndoCommand.Refresh();
    }

    private void RefreshTiles()
    {
        for (var row = 0; row < GameBoard.Size; row++)
        {
            for (var column = 0; column < GameBoard.Size; column++)
            {
                Tiles[row * GameBoard.Size + column].Value = board[row, column];
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}