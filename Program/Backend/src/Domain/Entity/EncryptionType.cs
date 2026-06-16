using Domain.Abstract;

namespace Domain.Entity;

public sealed class EncryptionType : StatusObjectAbstract<EncryptionType>
{
    public ICollection<Document>? Documents { get; set; }
    private EncryptionType(string name) : base(name) { }
    public static readonly EncryptionType Full = new(nameof(Full));
    public static readonly EncryptionType Part = new(nameof(Part));
    public static readonly EncryptionType None = new(nameof(None));
}