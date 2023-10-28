using AuthorizationApi.Database;

namespace AuthorizationService.Services;

public interface IDbInitializeService
{
    public void InitializeDb();
    public void MigrateDb();
}

public class DbInitializeService : IDbInitializeService
{
    private readonly AuthorizationDbContext authorizationDbContext;

    public DbInitializeService(AuthorizationDbContext dbContext)
    {
        authorizationDbContext = dbContext;
    }

    public void InitializeDb()
    {
        _ = authorizationDbContext.Database.EnsureDeleted();
        _ = authorizationDbContext.Database.EnsureCreated();
    }

    public void MigrateDb()
    {
        throw new NotImplementedException();
    }
}
