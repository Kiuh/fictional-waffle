using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthorizationApi.Database;
using Microsoft.EntityFrameworkCore;

namespace AdminClient
{
    internal class Database
	{
		const string CONNECTION_STRING = "";

		private AuthorizationDbContext ctx;

		public Database()
		{
			var builder = new DbContextOptionsBuilder<AuthorizationDbContext>();
			builder.UseNpgsql(CONNECTION_STRING);
            ctx = new AuthorizationDbContext(builder.Options);
		}
	}
}
