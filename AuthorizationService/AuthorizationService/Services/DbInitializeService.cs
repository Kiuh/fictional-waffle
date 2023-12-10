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
        _ = authorizationDbContext.Users.Add(
            new User()
            {
                Login = "login",
                RegistrationDate = DateTime.Now,
                Email = "kylturpro@gmail.com",
                EmailVerification = EmailVerificationState.Verified,
                HashedPassword = "428821350e9691491f616b754cd8315fb86d797ab35d843479e732ef90665324"
            }
        );
        _ = authorizationDbContext.SaveChanges();
    }

    public void MigrateDb()
    {
        throw new NotImplementedException();
    }
}
