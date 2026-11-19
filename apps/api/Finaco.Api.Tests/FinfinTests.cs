using Finaco.Api.Application.Finfin;
using Xunit;

namespace Finaco.Api.Tests;

public sealed class FinfinTests
{
    [Fact]
    public void Demo_response_does_not_invent_financial_amounts_when_data_is_missing()
    {
        var response = FinfinDemoResponder.Create("başlangıç bakiyesi eksik", null, [], ["başlangıç bakiyesi"]);

        Assert.True(response.IsDemo);
        Assert.Equal("demo", response.Mode);
        Assert.Contains("eksik", response.Answer);
        Assert.DoesNotContain("₺", response.Answer);
        Assert.Contains("başlangıç bakiyesi", response.MissingData);
    }
}
