using System.Windows;
using System.Windows.Input;
using _2048_dotnet_antigravity.Models;
using _2048_dotnet_antigravity.ViewModels;

namespace _2048_dotnet_antigravity;

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
                    if (e.PropertyName == nameof(MainViewModel.IsMenuVisible) && !vm.IsMenuVisible)
                    {
                        Focus();
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
            vm.ExecuteQuit();
            e.Handled = true;
            return;
        }

        if (vm.IsMenuVisible)
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