namespace Demo123;

using static System.Math;

internal class LogicLogic: ILogicLogic
{
    async Task<string> ILogicLogic.GetStarsAsync(int count, CancellationToken cancellation) 
    {
        int stars = Abs(count);
        TimeSpan delay = TimeSpan.FromSeconds(stars % 5);
        await Task.Delay(delay, cancellation);
        return new string('*', stars);
    }
}