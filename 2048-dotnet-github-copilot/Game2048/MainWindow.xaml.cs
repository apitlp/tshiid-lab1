namespace Game2048;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        Focusable = true;
        Loaded += (_, _) => Focus();
    }
}