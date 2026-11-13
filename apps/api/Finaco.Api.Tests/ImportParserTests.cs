using Finaco.Api.Application.Imports;
using Xunit;

namespace Finaco.Api.Tests;

public sealed class ImportParserTests
{
    [Theory]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("-42,50", -42.50)]
    [InlineData("12.50", 12.50)]
    public void Parses_turkish_and_invariant_amounts(string input, decimal expected)
    {
        Assert.True(ImportPreviewService.TryParseAmount(input, out var actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("12.11.2026", "2026-11-12")]
    [InlineData("2026-11-12", "2026-11-12")]
    public void Parses_supported_dates(string input, string expected)
    {
        Assert.True(ImportPreviewService.TryParseDate(input, out var date));
        Assert.Equal(expected, date.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void Preserves_commas_inside_quoted_csv_values()
    {
        var values = ImportPreviewService.ParseCsvLine("12.11.2026,\"MARKET, SUBE\",\"1.234,56\"");

        Assert.Equal(3, values.Count);
        Assert.Equal("MARKET, SUBE", values[1]);
        Assert.Equal("1.234,56", values[2]);
    }
}
