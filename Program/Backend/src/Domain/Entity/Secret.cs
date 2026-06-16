using Domain.Interfaces;

namespace Domain.Entity;

public sealed class Secret
{
    public int Id { get; init; }
    public string Value { get; init; }
    public Document? Document { get; set; }
    private readonly ISecretGenerator _secretGenerator;
#pragma warning disable CS9264, CS8618
    private Secret() { }
#pragma warning restore CS9264, CS8618
    public Secret(ISecretGenerator secretGenerator)
    {
        _secretGenerator = secretGenerator;
        Value = _secretGenerator.CreateSecret();
    }
}