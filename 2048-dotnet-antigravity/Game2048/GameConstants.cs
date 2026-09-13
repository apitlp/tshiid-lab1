using System.Collections.Generic;

namespace Game2048;

internal static class GameConstants
{
    public const int Size = 400;
    public const int GridLen = 4;
    public const int GridPadding = 10;

    public const string BackgroundColorGame = "#92877d";
    public const string BackgroundColorCellEmpty = "#9e948a";

    public static readonly IReadOnlyDictionary<int, string> BackgroundColorDict = new Dictionary<int, string>
    {
        { 2,     "#eee4da" },
        { 4,     "#ede0c8" },
        { 8,     "#f2b179" },
        { 16,    "#f59563" },
        { 32,    "#f67c5f" },
        { 64,    "#f65e3b" },
        { 128,   "#edcf72" },
        { 256,   "#edcc61" },
        { 512,   "#edc850" },
        { 1024,  "#edc53f" },
        { 2048,  "#edc22e" },
        { 4096,  "#eee4da" },
        { 8192,  "#edc22e" },
        { 16384, "#f2b179" },
        { 32768, "#f59563" },
        { 65536, "#f67c5f" }
    };

    public static readonly IReadOnlyDictionary<int, string> CellColorDict = new Dictionary<int, string>
    {
        { 2,     "#776e65" },
        { 4,     "#776e65" },
        { 8,     "#f9f6f2" },
        { 16,    "#f9f6f2" },
        { 32,    "#f9f6f2" },
        { 64,    "#f9f6f2" },
        { 128,   "#f9f6f2" },
        { 256,   "#f9f6f2" },
        { 512,   "#f9f6f2" },
        { 1024,  "#f9f6f2" },
        { 2048,  "#f9f6f2" },
        { 4096,  "#776e65" },
        { 8192,  "#f9f6f2" },
        { 16384, "#776e65" },
        { 32768, "#776e65" },
        { 65536, "#f9f6f2" }
    };

    public const string DefaultCellForegroundColor = "#776e65";
    public const string WinLoseForegroundColor = "#776e65";

    public static string GetBackgroundColor(int value)
    {
        if (value == 0)
        {
            return BackgroundColorCellEmpty;
        }

        if (BackgroundColorDict.TryGetValue(value, out var color))
        {
            return color;
        }

        return "#edc22e";
    }

    public static string GetForegroundColor(int value)
    {
        if (CellColorDict.TryGetValue(value, out var color))
        {
            return color;
        }

        return DefaultCellForegroundColor;
    }
}
