using Microsoft.EntityFrameworkCore;


namespace Kyuutie.Database;
public class KyuutieContext : DbContext
{
    public KyuutieContext(DbContextOptions<KyuutieContext> options) : base(options)
    { }
    
}
