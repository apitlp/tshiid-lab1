using System.Windows.Media;

namespace Game2048.ViewModels;

public class TileViewModel : ViewModelBase
{
    private int _value;
    private string _displayText = string.Empty;
    private Brush _backgroundBrush = Brushes.Transparent;
    private Brush _foregroundBrush = Brushes.Black;

    public int Row { get; }
    public int Column { get; }

    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public string DisplayText
    {
        get => _displayText;
        set => SetProperty(ref _displayText, value);
    }

    public Brush BackgroundBrush
    {
        get => _backgroundBrush;
        set => SetProperty(ref _backgroundBrush, value);
    }

    public Brush ForegroundBrush
    {
        get => _foregroundBrush;
        set => SetProperty(ref _foregroundBrush, value);
    }

    public TileViewModel(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public void Update(int value, string? customText = null, string? customBg = null, string? customFg = null)
    {
        Value = value;
        DisplayText = customText ?? (value == 0 ? string.Empty : value.ToString());

        string bgHex = customBg ?? GameConstants.GetBackgroundColor(value);
        string fgHex = customFg ?? GameConstants.GetForegroundColor(value);

        BackgroundBrush = BrushCache.Get(bgHex);
        ForegroundBrush = BrushCache.Get(fgHex);
    }
}
