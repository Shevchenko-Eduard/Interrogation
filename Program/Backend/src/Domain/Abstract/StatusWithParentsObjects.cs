namespace Domain.Abstract;

public abstract class StatusWithParentsObjectsAbstract<T> : StatusObjectAbstract<T> where T : StatusWithParentsObjectsAbstract<T>
{
    #region Fields
    public IEnumerable<T>? Parents { get; }
    #endregion
    #region Constructors
    protected StatusWithParentsObjectsAbstract(string name) : base(name) { }
    protected StatusWithParentsObjectsAbstract(byte id, string name) : base(id, name) { }
    protected StatusWithParentsObjectsAbstract(string name, params IEnumerable<T> parents) : base(name) { Parents = parents; }
    protected StatusWithParentsObjectsAbstract(byte id, string name, params IEnumerable<T> parents) : base(id, name) { Parents = parents; }
    #endregion
    #region Methods
    /// <summary>
    /// Текущий объект проверяют на соответствие входящему аргументу.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is StatusWithParentsObjectsAbstract<T> statusWithParents) { return Equals(statusWithParents: statusWithParents); }
        if (obj is StatusObjectAbstract<T> status) { return Equals(status: status); }
        if (obj is EnumObjectAbstract<T> enumObject) { return Equals(enumObject: enumObject); }
        return false;
    }
    /// <summary>
    /// Текущий объект проверяют на соответствие входящему аргументу.
    /// </summary>
    public bool Equals(StatusWithParentsObjectsAbstract<T> statusWithParents)
    {
        ArgumentNullException.ThrowIfNull(statusWithParents);
        bool result = GetHashCode() == statusWithParents.GetHashCode();
        if (result) { return result; }
        if (Parents?.Any() == true)
        {
            return Parents.Any(p => p.Equals(statusWithParents));
        }
        return result;
    }
    public static bool operator ==(StatusWithParentsObjectsAbstract<T> left, StatusWithParentsObjectsAbstract<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Equals(right);
    }
    public static bool operator !=(StatusWithParentsObjectsAbstract<T> left, StatusWithParentsObjectsAbstract<T> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return !left.Equals(right);
    }
    public override int GetHashCode() => Id;
    #endregion
}