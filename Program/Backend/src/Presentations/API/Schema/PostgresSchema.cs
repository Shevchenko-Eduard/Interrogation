namespace API.Schema;

public class PostgresSchema(IConfiguration configuration)
{
    private readonly IConfigurationSection dbSettings = configuration.GetSection("DB");

    public string Host => dbSettings["Host"] ?? throw new InvalidOperationException("Postgres:Host is missing");
    public int Port => int.Parse(dbSettings["Port"] ?? throw new InvalidOperationException("Postgres:Port is missing"));
    public string Database => dbSettings["Database"] ?? throw new InvalidOperationException("Postgres:Database is missing");
    public string Username => dbSettings["Username"] ?? throw new InvalidOperationException("Postgres:Username is missing");
    public string Password => dbSettings["Password"] ?? throw new InvalidOperationException("Postgres:Password is missing");
}