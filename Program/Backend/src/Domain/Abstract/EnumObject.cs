using System.Collections.ObjectModel;
using System.Reflection;

namespace Domain.Abstract;

public abstract class EnumObjectAbstract<T> where T : EnumObjectAbstract<T>
{
    #region Fields
    public int Id { get; }
#pragma warning disable CA1000 // Не объявляйте статические члены в универсальных типах
    public static ReadOnlyCollection<T> All => new(_all.Value.ToList());
#pragma warning restore CA1000 // Не объявляйте статические члены в универсальных типах
    private static Lazy<HashSet<T>> _all = UpdateAll();
    private static int _nextId;
    #endregion
    #region Constructors
    protected EnumObjectAbstract()
    {
        UpdateAll();
        Id = GetNextId();
    }
    protected EnumObjectAbstract(int id)
    {
        UpdateAll();
        Id = id;
    }
    #endregion
    #region Methods
    private static Lazy<HashSet<T>> UpdateAll()
    {
        _all = new(() =>
        {
            return typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(T) && f.IsInitOnly)
                .Select(f => (T)(f.GetValue(null) ?? throw new Exception($"Field {f.Name} returned null")))
                .ToHashSet();
        });
        return _all;
    }
    private static int GetNextId() => Interlocked.Increment(ref _nextId);
#pragma warning disable CA1000 // Не объявляйте статические члены в универсальных типах
    public static EnumObjectAbstract<T> FromId(ushort id)
#pragma warning restore CA1000 // Не объявляйте статические члены в универсальных типах
    {
        return All.FirstOrDefault(g => g.Id == id)
            ?? throw new Exception($"Invalid gender id: {id}");
    }
    public override bool Equals(object? obj)
    {
        return obj is EnumObjectAbstract<T> enumObject && Equals(enumObject: enumObject);
    }
    public bool Equals(EnumObjectAbstract<T> enumObject)
    {
        ArgumentNullException.ThrowIfNull(enumObject);
        return enumObject.GetHashCode() == GetHashCode();
    }
    public override int GetHashCode() => Id;
    #endregion
}