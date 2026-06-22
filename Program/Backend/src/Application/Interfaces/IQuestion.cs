namespace Application.Interfaces;

public interface IQuestion<TOutput>
{
    Task<TOutput> Ask();
}

public interface IQuestion<TOutput, TInput>
{
    Task<TOutput> Ask(TInput input);
}