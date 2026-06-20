using Microsoft.Extensions.Hosting;

namespace LibWeb;

public class HostEnvironment(IHostEnvironment hostEnvironment) : Infrastructure.Interfaces.IHostEnvironment
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    public string CurrentEnvironment => _hostEnvironment.EnvironmentName;
    public bool IsDevelopment() => _hostEnvironment.IsDevelopment();
    public bool IsProduction() => _hostEnvironment.IsProduction();
    public bool IsStaging() => _hostEnvironment.IsStaging();
}