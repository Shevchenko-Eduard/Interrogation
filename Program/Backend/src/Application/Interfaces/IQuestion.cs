namespace Application.Interfaces;

public interface IQuestion{}

public interface IQuestion<TOutput> : IQuestion
{
    Task<TOutput> Ask();
}

public interface IQuestion<TOutput, TInput> : IQuestion
{
    Task<TOutput> Ask(TInput input);
}