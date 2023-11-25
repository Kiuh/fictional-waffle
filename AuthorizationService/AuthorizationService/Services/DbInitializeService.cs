using AuthorizationApi.Database;
using AuthorizationApi.Database.Models;

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
        List<User> list =
            new()
            {
                new()
                {
                    Login = "Login1",
                    RegistrationDate = DateTime.Now,
                    Email = "Login1@email.com",
                    EmailVerification = EmailVerificationState.Verified,
                    HashedPassword = "TODO: HashedPassword"
                }
            };

        authorizationDbContext.Users.AddRange(list);
    }

    public void MigrateDb()
    {
        throw new NotImplementedException();
    }
}
