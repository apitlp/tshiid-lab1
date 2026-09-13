using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using _2048_dotnet_antigravity.Models;

namespace _2048_dotnet_antigravity.ViewModels;

public class MainViewModel : ViewModelBase
{
    public const int MinGridSize = 2;
    public const int MaxGridSize = 10;
    public const int DefaultGridSize = 4;

    private GameEngine _engine;
    private GameState _gameState;
    private bool _isMenuVisible = true;
    private bool _isPaused;
    private string _gridSizeInput = DefaultGridSize.ToString();
    private string? _errorMessage;

    public ObservableCollection<TileViewModel> Tiles { get; } = new();

    public int GridSize => _engine.Size;

    public bool IsMenuVisible
    {
        get => _isMenuVisible;
        set
        {
            if (SetProperty(ref _isMenuVisible, value))
            {
                OnPropertyChanged(nameof(IsGameVisible));
            }
        }
    }

    public bool IsPaused
    {
        get => _isPaused;
        set => SetProperty(ref _isPaused, value);
    }

    public bool IsGameVisible => !_isMenuVisible;

    public string GridSizeInput
    {
        get => _gridSizeInput;
        set => SetProperty(ref _gridSizeInput, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public GameState GameState
    {
        get => _gameState;
        private set => SetProperty(ref _gameState, value);
    }

    public ICommand StartGameCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand ExitToMenuCommand { get; }
    public ICommand MoveCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand QuitCommand { get; }

    public MainViewModel() : this(new GameEngine(DefaultGridSize), isMenuVisible: true)
    {
    }

    public MainViewModel(GameEngine engine, bool isMenuVisible = true)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _isMenuVisible = isMenuVisible;

        InitializeTiles(_engine.Size);

        StartGameCommand = new RelayCommand(ExecuteStartGame);
        PauseCommand = new RelayCommand(ExecutePause, () => !IsMenuVisible && !IsPaused);
        ResumeCommand = new RelayCommand(ExecuteResume, () => IsPaused);
        ExitToMenuCommand = new RelayCommand(ExecuteExitToMenu, () => IsPaused);

        MoveCommand = new RelayCommand<Direction?>(dir =>
        {
            if (dir.HasValue && !IsMenuVisible && !IsPaused)
            {
                ExecuteMove(dir.Value);
            }
        });

        UndoCommand = new RelayCommand(ExecuteUndo, () => !IsMenuVisible && !IsPaused && _engine.CanUndo);
        NewGameCommand = new RelayCommand(ExecuteNewGame);
        QuitCommand = new RelayCommand(ExecuteQuit);

        UpdateBoard();
    }

    public void ExecuteStartGame()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(GridSizeInput))
            {
                ErrorMessage = $"Please enter a grid size between {MinGridSize} and {MaxGridSize}.";
                return;
            }

            if (!int.TryParse(GridSizeInput.Trim(), out int size))
            {
                ErrorMessage = "Invalid number format. Please enter an integer.";
                return;
            }

            if (size < MinGridSize || size > MaxGridSize)
            {
                ErrorMessage = $"Grid size must be between {MinGridSize} and {MaxGridSize}.";
                return;
            }

            ErrorMessage = null;
            IsPaused = false;
            _engine = new GameEngine(size);
            InitializeTiles(size);
            OnPropertyChanged(nameof(GridSize));
            UpdateBoard();
            IsMenuVisible = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error starting game: {ex.Message}";
        }
    }

    public void ExecutePause()
    {
        if (!IsMenuVisible)
        {
            IsPaused = true;
        }
    }

    public void ExecuteResume()
    {
        IsPaused = false;
    }

    public void ExecuteExitToMenu()
    {
        IsPaused = false;
        IsMenuVisible = true;
    }

    private void InitializeTiles(int size)
    {
        Tiles.Clear();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Tiles.Add(new TileViewModel(i, j));
            }
        }
    }

    public void ExecuteMove(Direction direction)
    {
        if (IsMenuVisible || IsPaused) return;

        bool moved = _engine.Move(direction);
        if (moved)
        {
            UpdateBoard();
        }
    }

    public void ExecuteUndo()
    {
        if (IsMenuVisible || IsPaused) return;

        if (_engine.Undo())
        {
            UpdateBoard();
        }
    }

    public void ExecuteNewGame()
    {
        _engine.StartNewGame();
        UpdateBoard();
    }

    public void ExecuteQuit()
    {
        Application.Current?.Shutdown();
    }

    private void UpdateBoard()
    {
        int[,] grid = _engine.Grid;
        GameState = _engine.CurrentState;
        int size = _engine.Size;

        int winRow = size >= 3 ? 1 : 0;
        int youCol = size >= 3 ? 1 : 0;
        int textCol = size >= 3 ? 2 : 1;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int index = i * size + j;
                var tile = Tiles[index];
                int val = grid[i, j];

                if (GameState == GameState.Win && i == winRow && j == youCol)
                {
                    tile.Update(val, customText: "You", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else if (GameState == GameState.Win && i == winRow && j == textCol)
                {
                    tile.Update(val, customText: "Win!", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else if (GameState == GameState.Lose && i == winRow && j == youCol)
                {
                    tile.Update(val, customText: "You", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else if (GameState == GameState.Lose && i == winRow && j == textCol)
                {
                    tile.Update(val, customText: "Lose!", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else
                {
                    tile.Update(val);
                }
            }
        }
    }
}
