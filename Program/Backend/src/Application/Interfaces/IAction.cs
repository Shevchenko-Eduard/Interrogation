namespace Application.Interfaces;

public interface IAction{}

public interface IAction<TInput> : IAction
{
    Task Execute(TInput input);
}

public interface IAction<TInput, TOutput> : IAction
{
    Task<TOutput> Execute(TInput input);
}