using AuthorizationApi.Database;
using Microsoft.EntityFrameworkCore;

namespace AdminClient
{
    internal class Database
    {
        private const string CONNECTION_STRING = "";

        private readonly AuthorizationDbContext ctx;

        public Database()
        {
            DbContextOptionsBuilder<AuthorizationDbContext> builder = new();
            _ = builder.UseNpgsql(CONNECTION_STRING);
            ctx = new AuthorizationDbContext(builder.Options);
        }
    }
}
