namespace Domain.Abstract;

public abstract class StatusObjectAbstract<T> : EnumObjectAbstract<T> where T : StatusObjectAbstract<T>
{
    public string Name { get; }
    protected StatusObjectAbstract(string name) : base() { Name = name; }
    protected StatusObjectAbstract(int id, string name) : base(id) { Name = name; }
    public static StatusObjectAbstract<T> FromName(string name)
    {
        return All.FirstOrDefault(g => g.Name == name)
            ?? throw new Exception($"Invalid gender name: {name}");
    }
    #region Methods
    public override bool Equals(object? obj)
    {
        if (obj is StatusObjectAbstract<T> status) { return Equals(status: status); }
        if (obj is EnumObjectAbstract<T> enumObject) { return Equals(enumObject: enumObject); }
        throw new Exception("Unsupported object type for comparison");
    }
    public bool Equals(StatusObjectAbstract<T> status)
    {
        return status.GetHashCode() == GetHashCode();
    }
    public override int GetHashCode() => Id;
    #endregion
}