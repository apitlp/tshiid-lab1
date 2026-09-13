using System.Collections.Concurrent;
using System.Windows.Media;

namespace _2048_dotnet_antigravity.ViewModels;

public static class BrushCache
{
    private static readonly ConcurrentDictionary<string, SolidColorBrush> _cache = new();

    public static SolidColorBrush Get(string hex)
    {
        return _cache.GetOrAdd(hex, h =>
        {
            var color = (Color)ColorConverter.ConvertFromString(h);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        });
    }
}
