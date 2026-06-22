namespace API.Schema;

internal sealed class MinIoSchema(IConfiguration configuration)
{
    private readonly IConfigurationSection minioSettings = configuration.GetSection("Minio");

    public string Endpoint => minioSettings["Endpoint"] ?? throw new InvalidOperationException("Minio:Endpoint is missing");
    public string Username => minioSettings["Username"] ?? throw new InvalidOperationException("Minio:Username is missing");
    public string Password => minioSettings["Password"] ?? throw new InvalidOperationException("Minio:Password is missing");
    public bool Https => bool.TryParse(minioSettings["HTTPS"], out var result) && result;
}