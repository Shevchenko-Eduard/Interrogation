namespace Infrastructure.Interfaces;

public interface IHostEnvironment
{
    bool IsProduction();
    bool IsDevelopment();
    bool IsStaging();
    string CurrentEnvironment { get; }
}