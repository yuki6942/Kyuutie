using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kyuutie.Database;

public class DesignTimeFactory : IDesignTimeDbContextFactory<KyuutieContext>
{
    public KyuutieContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<KyuutieContext> builder = new();
        builder.UseNpgsql("Host=localhost;Port=5432;Database=kyuutie;Username=kyuutie");

        builder.UseNpgsql().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        return new KyuutieContext(builder.Options);
    }
}