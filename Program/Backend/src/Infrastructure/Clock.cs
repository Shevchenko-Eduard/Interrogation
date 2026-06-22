using Domain.Interfaces;

namespace Infrastructure;

public sealed class Clock : IClock
{
    DateTimeOffset IClock.Now => Now();
    public static DateTimeOffset Now() => DateTimeOffset.Now;
}