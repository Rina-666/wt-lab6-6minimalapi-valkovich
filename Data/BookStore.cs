using wt_lab6_6minimalapi_valkovich.Models;

namespace wt_lab6_6minimalapi_valkovich.Data;

public static class BookStore
{
    public static List<Book> Books { get; } = new()
    {
        new Book { Id = 1, Title = "Гордость и предубеждение", Author = "Джейн Остин", Price = 890, IsAvailable = true },
        new Book { Id = 2, Title = "Джейн Эйр", Author = "Шарлотта Бронте", Price = 750, IsAvailable = true },
        new Book { Id = 3, Title = "1984", Author = "Джордж Оруэлл", Price = 350, IsAvailable = true },
        new Book { Id = 4, Title = "Мастер и Маргарита", Author = "Михаил Булгаков", Price = 920, IsAvailable = false },
        new Book { Id = 5, Title = "Гарри Поттер", Author = "Дж. К. Роулинг", Price = 1100, IsAvailable = true }
    };
}