using Microsoft.EntityFrameworkCore;

namespace CurrencyWatcher.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Currency> Currencies { get; set; }
    }
}
