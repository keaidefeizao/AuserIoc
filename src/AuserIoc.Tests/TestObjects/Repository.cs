using AuserIoc.Common.Attributes;
using System.Diagnostics;

namespace AuserIoc.Tests.TestObjects;

public interface IRepository<T> where T : class
{
    public void Add(T item);

    public void Update(T item);

    public void Delete(T item);

    public T Find(object? id);

    public IEnumerable<T> List();
}

public interface IRepository<TDatabaseContext, T> : IRepository<T> where T : class
{
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(T item)
    {
        Debug.WriteLine($"{nameof(Repository<T>)}：{nameof(Add)} {item}");
    }

    public void Delete(T item)
    {
        Debug.WriteLine($"{nameof(Repository<T>)}：{nameof(Delete)} {item}");
    }

    public T Find(object? id)
    {
        Debug.WriteLine($"{nameof(Repository<T>)}：{nameof(Find)} {id}");
        return null!;
    }

    public void Update(T item)
    {
        Debug.WriteLine($"{nameof(Repository<T>)}：{nameof(Update)} {item}");
    }

    public IEnumerable<T> List()
    {
        Debug.WriteLine($"{nameof(Repository<T>)}：{nameof(List)}");
        return [];
    }
}

public class Repository<TDatabaseContext, T> : IRepository<TDatabaseContext, T> where T : class
{
    private readonly DbContextBase _dbContext;
    public Repository()
    {
        // 根据泛型 TDatabaseContext 赋值 _dbContext

        _dbContext = new SqliteAppDbContext();
    }

    public void Add(T item)
    {
        Debug.WriteLine($"{nameof(Repository<TDatabaseContext, T>)}：{nameof(Add)} {item}");
    }

    public void Delete(T item)
    {
        Debug.WriteLine($"{nameof(Repository<TDatabaseContext, T>)}：{nameof(Delete)} {item}");
    }

    public T Find(object? id)
    {
        Debug.WriteLine($"{nameof(Repository<TDatabaseContext, T>)}：{nameof(Find)} {id}");
        return null!;
    }

    public void Update(T item)
    {
        Debug.WriteLine($"{nameof(Repository<TDatabaseContext, T>)}：{nameof(Update)} {item}");
    }

    public IEnumerable<T> List()
    {
        Debug.WriteLine($"{nameof(Repository<TDatabaseContext, T>)}：{nameof(List)}");
        return [];
    }
}

public class UserEntity : DbEntityBase<int>
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class DocEntity: DbEntityBase<Guid>
{
    public string Title { get; set; } = string.Empty;

    public string Context { get; set; } = string.Empty;

    public int Poster { get; set; }
}

public class DbEntityBase<TKey>
{
    public TKey Id { get; set; } = default!;
}

public class SqliteContextTag { }

public class DefaultContextTag { }

public class DbContextBase
{
}
public class DbContextBase<TDbContext, TTag> : DbContextBase
{
}

public class AppDbContext : DbContextBase<AppDbContext, DefaultContextTag>
{
}

public class SqliteAppDbContext : DbContextBase<SqliteAppDbContext, SqliteContextTag>
{
}
