namespace Infrastructure.Database.Converter;

public static class FactoryConverter
{
    public static void UseConverter(IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> mutableEntityTypes)
    {
        foreach (var entityType in mutableEntityTypes)
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(Guid))
                {
                    property.SetValueConverter(new GuidToStringConverter());
                    property.SetMaxLength(36);
                }
            }
        }
    }
}