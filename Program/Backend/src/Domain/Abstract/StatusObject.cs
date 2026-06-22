namespace Domain.Abstract;

public abstract class StatusObjectAbstract<T> : EnumObjectAbstract<T> where T : StatusObjectAbstract<T>
{
    public string Name { get; }
    protected StatusObjectAbstract(string name) : base() { Name = name; }
    protected StatusObjectAbstract(int id, string name) : base(id) { Name = name; }
#pragma warning disable CA1000 // Не объявляйте статические члены в универсальных типах
    public static StatusObjectAbstract<T> FromName(string name)
#pragma warning restore CA1000 // Не объявляйте статические члены в универсальных типах
    {
        return All.FirstOrDefault(g => g.Name == name)
            ?? throw new Exception($"Invalid gender name: {name}");
    }
    #region Methods
    public override bool Equals(object? obj)
    {
        if (obj is StatusObjectAbstract<T> status) { return Equals(status: status); }
        if (obj is EnumObjectAbstract<T> enumObject) { return Equals(enumObject: enumObject); }
        return false;
    }
    public bool Equals(StatusObjectAbstract<T> status)
    {
        ArgumentNullException.ThrowIfNull(status);
        return status.GetHashCode() == GetHashCode();
    }
    public override int GetHashCode() => Id;
    #endregion
}