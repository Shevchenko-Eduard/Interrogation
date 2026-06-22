namespace LibWeb;

#pragma warning disable CA1812 // Избегайте внутренних классов, не имеющих экземпляры
internal sealed class HostEnvironment(IHostEnvironment hostEnvironment) : Infrastructure.Interfaces.IHostEnvironment
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    public string CurrentEnvironment => _hostEnvironment.EnvironmentName;
    public bool IsDevelopment() => _hostEnvironment.IsDevelopment();
    public bool IsProduction() => _hostEnvironment.IsProduction();
    public bool IsStaging() => _hostEnvironment.IsStaging();
}