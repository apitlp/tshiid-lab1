using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using _2048_dotnet_antigravity.Models;

namespace _2048_dotnet_antigravity.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly GameEngine _engine;
    private GameState _gameState;

    public ObservableCollection<TileViewModel> Tiles { get; } = new();

    public GameState GameState
    {
        get => _gameState;
        private set => SetProperty(ref _gameState, value);
    }

    public ICommand MoveCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand QuitCommand { get; }

    public MainViewModel() : this(new GameEngine())
    {
    }

    public MainViewModel(GameEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));

        // Initialize 4x4 tiles in row-major order
        for (int i = 0; i < _engine.Size; i++)
        {
            for (int j = 0; j < _engine.Size; j++)
            {
                Tiles.Add(new TileViewModel(i, j));
            }
        }

        MoveCommand = new RelayCommand<Direction?>(dir =>
        {
            if (dir.HasValue)
            {
                ExecuteMove(dir.Value);
            }
        });

        UndoCommand = new RelayCommand(ExecuteUndo, () => _engine.CanUndo);
        NewGameCommand = new RelayCommand(ExecuteNewGame);
        QuitCommand = new RelayCommand(ExecuteQuit);

        UpdateBoard();
    }

    public void ExecuteMove(Direction direction)
    {
        bool moved = _engine.Move(direction);
        if (moved)
        {
            UpdateBoard();
        }
    }

    public void ExecuteUndo()
    {
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

        for (int i = 0; i < _engine.Size; i++)
        {
            for (int j = 0; j < _engine.Size; j++)
            {
                int index = i * _engine.Size + j;
                var tile = Tiles[index];
                int val = grid[i, j];

                if (GameState == GameState.Win && i == 1 && j == 1)
                {
                    tile.Update(val, customText: "You", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else if (GameState == GameState.Win && i == 1 && j == 2)
                {
                    tile.Update(val, customText: "Win!", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else if (GameState == GameState.Lose && i == 1 && j == 1)
                {
                    tile.Update(val, customText: "You", customBg: GameConstants.BackgroundColorCellEmpty, customFg: GameConstants.WinLoseForegroundColor);
                }
                else if (GameState == GameState.Lose && i == 1 && j == 2)
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
