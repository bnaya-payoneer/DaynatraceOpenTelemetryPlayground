using Demo123;
namespace Demo123.Tests;

public class LogicLogicTests
{
    [Theory]
    [InlineData(2, "**")]
    [InlineData(0, "")]
    [InlineData(-3, "***")]
    public async Task GetStarsAsyncTest(int count, string expected )
    {
        ILogicLogic logic = new LogicLogic();
        string result = await logic.GetStarsAsync( count, CancellationToken.None);
        Assert.Equal(expected, result);
    }
}