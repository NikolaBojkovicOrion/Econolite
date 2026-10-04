using Econolite_API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Extensions
{
    public static class SqlDatabaseExtension
    {
        public static WebApplicationBuilder AddSqlDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<EconoliteDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            return builder;
        }
    }
}