using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_valkovich.Models;

namespace wt_lab7_7postgres_efcore_valkovich.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Гордость и предубеждение", Author = "Джейн Остин", Price = 890, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 2, Title = "Джейн Эйр", Author = "Шарлотта Бронте", Price = 750, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 3, Title = "1984", Author = "Джордж Оруэлл", Price = 350, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}