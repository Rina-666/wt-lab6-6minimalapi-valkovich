using wt_lab6_6minimalapi_valkovich.Models;
using wt_lab6_6minimalapi_valkovich.Data;

var builder = WebApplication.CreateBuilder(args);

// Ќастройка OpenAPI (дл€ документации)
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 1 уровень: базовый GET
app.MapGet("/", () => "Minimal API работает!");

// 3 уровень: GET /api/info с возвратом JSON
app.MapGet("/api/info", () =>
{
    return new
    {
        Application = "Web Technologies",
        Technology = "ASP.NET Core Minimal API",
        Version = "1.0"
    };
});

// GET: список всех книг + поиск (query-параметр search)
app.MapGet("/api/books", (string? search) =>
{
    var books = BookStore.Books.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        books = books.Where(x => x.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
    return Results.Ok(books);
});

// GET: книга по ID
app.MapGet("/api/books/{id}", (int id) =>
{
    var book = BookStore.Books.FirstOrDefault(x => x.Id == id);
    if (book == null) return Results.NotFound();
    return Results.Ok(book);
});

// POST: создание книги (с проверкой на 400 Bad Request)
app.MapPost("/api/books", (Book book) =>
{
    if (string.IsNullOrWhiteSpace(book.Title))
        return Results.BadRequest("Ќазвание книги не может быть пустым");

    book.Id = BookStore.Books.Any() ? BookStore.Books.Max(x => x.Id) + 1 : 1;
    BookStore.Books.Add(book);

    return Results.Created($"/api/books/{book.Id}", book);
});

// PUT: обновление книги
app.MapPut("/api/books/{id}", (int id, Book updatedBook) =>
{
    var book = BookStore.Books.FirstOrDefault(x => x.Id == id);
    if (book == null) return Results.NotFound();

    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Price = updatedBook.Price;
    book.IsAvailable = updatedBook.IsAvailable;

    return Results.Ok(book);
});

// DELETE: удаление книги
app.MapDelete("/api/books/{id}", (int id) =>
{
    var book = BookStore.Books.FirstOrDefault(x => x.Id == id);
    if (book == null) return Results.NotFound();

    BookStore.Books.Remove(book);
    return Results.NoContent(); // 204 No Content
});

// «апуск с поддержкой порта Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");