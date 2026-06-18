using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.Converter;

public class GuidToStringConverter : ValueConverter<Guid, string>
{
    public GuidToStringConverter()
        : base(
            v => v.ToString(),           // Convert to string
            v => Guid.Parse(v)           // Convert to Guid
    ){}
}