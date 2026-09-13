using System.Windows;
using System.Windows.Input;
using Game2048.Models;
using Game2048.ViewModels;

namespace Game2048;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (DataContext is MainViewModel vm && vm.IsMenuVisible)
            {
                GridSizeBox.Focus();
                GridSizeBox.SelectAll();
            }
            else
            {
                Focus();
            }
        };

        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.IsMenuVisible))
                    {
                        if (vm.IsMenuVisible)
                        {
                            GridSizeBox.Focus();
                            GridSizeBox.SelectAll();
                        }
                        else
                        {
                            Focus();
                        }
                    }
                    else if (e.PropertyName == nameof(MainViewModel.IsPaused))
                    {
                        if (!vm.IsPaused && !vm.IsMenuVisible)
                        {
                            Focus();
                        }
                    }
                };
            }
        };

        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (vm.IsMenuVisible)
            {
                // In starting menu, Escape quits application
                vm.ExecuteQuit();
            }
            else if (vm.IsPaused)
            {
                // In pause menu, Escape resumes game
                vm.ExecuteResume();
            }
            else
            {
                // During tile matching, Escape opens pause menu
                vm.ExecutePause();
            }

            e.Handled = true;
            return;
        }

        // If either starting menu or pause menu is active, do not process tile movement or undo keys
        if (vm.IsMenuVisible || vm.IsPaused)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.B:
                vm.ExecuteUndo();
                e.Handled = true;
                break;

            case Key.Up:
            case Key.W:
            case Key.I:
                vm.ExecuteMove(Direction.Up);
                e.Handled = true;
                break;

            case Key.Down:
            case Key.S:
            case Key.K:
                vm.ExecuteMove(Direction.Down);
                e.Handled = true;
                break;

            case Key.Left:
            case Key.A:
            case Key.J:
                vm.ExecuteMove(Direction.Left);
                e.Handled = true;
                break;

            case Key.Right:
            case Key.D:
            case Key.L:
                vm.ExecuteMove(Direction.Right);
                e.Handled = true;
                break;
        }
    }
}