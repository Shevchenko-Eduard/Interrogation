using System.Collections.ObjectModel;
using System.Reflection;

namespace Domain.Abstract;

public abstract class EnumObjectAbstract<T> where T : EnumObjectAbstract<T>
{
    #region Fields
    public int Id { get; }
    public static ReadOnlyCollection<T> All { get { return new(_all.Value.ToList()); } }
    private static Lazy<HashSet<T>> _all = UpdateAll();
    private static int _nextId = 0;
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
    public static EnumObjectAbstract<T> FromId(ushort id)
    {
        return All.FirstOrDefault(g => g.Id == id)
            ?? throw new Exception($"Invalid gender id: {id}");
    }
    public override bool Equals(object? obj)
    {
        if (obj is EnumObjectAbstract<T> enumObject) { return Equals(enumObject: enumObject); }
        throw new Exception("Unsupported object type for comparison");
    }
    public bool Equals(EnumObjectAbstract<T> enumObject)
    {
        return enumObject.GetHashCode() == GetHashCode();
    }
    public override int GetHashCode() => Id;
    #endregion
}