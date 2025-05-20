using Microsoft.EntityFrameworkCore;
using UWBike.Model;
using UWBike.Data.Mappings;

namespace UWBike.Connection
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Moto> Motos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MotoMapping());
            base.OnModelCreating(modelBuilder);
        }
    }
}
