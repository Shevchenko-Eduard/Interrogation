namespace Domain.Interfaces;

public interface IClock
{
    DateTimeOffset Now { get; }
}