using BotCrud;
using Microsoft.EntityFrameworkCore;


public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(@"server=::1; Port=5432; Database=ProductsBot; User Id=postgres; password=postgres;");
    }
}
