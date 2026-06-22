namespace API.Schema;

internal sealed class RedisSchema(IConfiguration configuration)
{
    private readonly IConfigurationSection redisSettings = configuration.GetSection("Redis");

    public string Endpoint => redisSettings["Endpoint"] ?? throw new InvalidOperationException("Redis:Endpoint is missing");
    public string? InstanceName => redisSettings["InstanceName"];
    public string? Password => redisSettings["Password"];
}