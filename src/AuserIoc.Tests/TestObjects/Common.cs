namespace AuserIoc.Tests.TestObjects;

public interface IUniverse
{
    IEarth Earth { get; }

    IMoon Moon { get; }
}

public interface IMoon
{
    string ToString();
}

public interface IEarth
{
    string ToString();
}

public class Earth : IEarth
{
    public override string ToString()
    {
        return $"{nameof(Earth)} id：{GetHashCode()}";
    }
}

public class Moon : IMoon
{
    public override string ToString()
    {
        return $"{nameof(Moon)} id：{GetHashCode()}";
    }
}

public class Universe : IUniverse
{
    public Universe(IEarth earth, IMoon moon)
    {
        Earth = earth;
        Moon = moon;
    }

    public IEarth Earth { get; }
    public IMoon Moon { get; }

    public override string ToString()
    {
        return $"{nameof(Universe)} id：{GetHashCode()}，{Earth}，{Moon}";
    }
}
