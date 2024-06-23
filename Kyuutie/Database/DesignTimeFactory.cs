using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Kyuutie.Database;

public class DesignTimeFactory : IDesignTimeDbContextFactory<KyuutieContext>
{
    public KyuutieContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<KyuutieContext> builder = new();
        builder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=kyuutie;Username=postgres");

        builder.UseNpgsql().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        return new KyuutieContext(builder.Options);
    }
}
