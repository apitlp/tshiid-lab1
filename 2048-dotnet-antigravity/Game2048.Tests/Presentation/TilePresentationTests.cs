using Game2048;
using Game2048.ViewModels;
using Xunit;

namespace Game2048.Tests.Presentation;

public class TilePresentationTests
{
    [Fact]
    public void TileViewModel_Update_WhenValueIsZero_SetsEmptyDisplayTextAndEmptyBackground()
    {
        var tile = new TileViewModel(0, 0);

        tile.Update(0);

        Assert.Equal(0, tile.Value);
        Assert.Equal(string.Empty, tile.DisplayText);
        Assert.NotNull(tile.BackgroundBrush);
        Assert.NotNull(tile.ForegroundBrush);
    }

    [Theory]
    [InlineData(2, "2")]
    [InlineData(4, "4")]
    [InlineData(8, "8")]
    [InlineData(16, "16")]
    [InlineData(2048, "2048")]
    public void TileViewModel_Update_WhenValueIsPositive_SetsNumberAndColors(int value, string expectedText)
    {
        var tile = new TileViewModel(1, 1);

        tile.Update(value);

        Assert.Equal(value, tile.Value);
        Assert.Equal(expectedText, tile.DisplayText);
        Assert.NotNull(tile.BackgroundBrush);
        Assert.NotNull(tile.ForegroundBrush);
    }

    [Fact]
    public void TileViewModel_Update_WithCustomTextAndColors_AppliesCustomValues()
    {
        var tile = new TileViewModel(1, 1);

        tile.Update(0, customText: "You", customBg: GameConstants.BackgroundColorCellEmpty, customFg: "#776e65");

        Assert.Equal("You", tile.DisplayText);
        Assert.NotNull(tile.BackgroundBrush);
        Assert.NotNull(tile.ForegroundBrush);
    }

    [Theory]
    [InlineData(2, "#eee4da")]
    [InlineData(4, "#ede0c8")]
    [InlineData(8, "#f2b179")]
    [InlineData(16, "#f59563")]
    [InlineData(32, "#f67c5f")]
    [InlineData(64, "#f65e3b")]
    [InlineData(128, "#edcf72")]
    [InlineData(256, "#edcc61")]
    [InlineData(512, "#edc850")]
    [InlineData(1024, "#edc53f")]
    [InlineData(2048, "#edc22e")]
    public void GameConstants_GetBackgroundColor_ReturnsExpectedColorsForStandardTiles(int value, string expectedHex)
    {
        string color = GameConstants.GetBackgroundColor(value);
        Assert.Equal(expectedHex, color);
    }

    [Fact]
    public void GameConstants_GetBackgroundColor_ReturnsFallbackForUnknownOrZeroValues()
    {
        Assert.Equal(GameConstants.BackgroundColorCellEmpty, GameConstants.GetBackgroundColor(0));
        Assert.Equal("#edc22e", GameConstants.GetBackgroundColor(131072));
    }

    [Theory]
    [InlineData(2, "#776e65")]
    [InlineData(4, "#776e65")]
    [InlineData(8, "#f9f6f2")]
    [InlineData(16, "#f9f6f2")]
    [InlineData(2048, "#f9f6f2")]
    public void GameConstants_GetForegroundColor_ReturnsExpectedContrastColors(int value, string expectedHex)
    {
        string color = GameConstants.GetForegroundColor(value);
        Assert.Equal(expectedHex, color);
    }

    [Fact]
    public void BrushCache_Get_ReturnsSameFrozenBrushInstanceForSameHex()
    {
        var brush1 = BrushCache.Get("#92877d");
        var brush2 = BrushCache.Get("#92877d");

        Assert.Same(brush1, brush2);
        Assert.True(brush1.IsFrozen);
    }
}
