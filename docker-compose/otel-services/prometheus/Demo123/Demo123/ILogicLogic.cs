namespace Demo123;

public interface ILogicLogic
{
    Task<string> GetStarsAsync(int count, CancellationToken cancellation = default);
}