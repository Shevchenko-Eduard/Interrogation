namespace Application.Interfaces;


public interface IAction<TInput>
{
    Task Execute(TInput input);
}

public interface IAction<TInput, TOutput>
{
    Task<TOutput> Execute(TInput input);
}