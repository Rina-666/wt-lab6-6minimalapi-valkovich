using wt_lab6_6minimalapi_valkovich.Models;
using wt_lab6_6minimalapi_valkovich.Data;
using Microsoft.AspNetCore.OpenApi; // Обязательно для AddOpenApi/MapOpenApi

var builder = WebApplication.CreateBuilder(args);

// Используем встроенный OpenAPI для .NET 9
builder.Services.AddOpenApi();

var app = builder.Build();

// Включаем документацию всегда (и локально, и на Render)
app.MapOpenApi();

// --- Ваши endpoint'ы ---

app.MapGet("/", () => "Minimal API работает!");

app.MapGet("/api/info", () =>
{
    return new
    {
        Application = "Web Technologies",
        Technology = "ASP.NET Core Minimal API",
        Version = "1.0"
    };
});

app.MapGet("/api/books", (string? search) =>
{
    var books = BookStore.Books.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        books = books.Where(x => x.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
    return Results.Ok(books);
});

app.MapGet("/api/books/{id}", (int id) =>
{
    var book = BookStore.Books.FirstOrDefault(x => x.Id == id);
    if (book == null) return Results.NotFound();
    return Results.Ok(book);
});

app.MapPost("/api/books", (Book book) =>
{
    if (string.IsNullOrWhiteSpace(book.Title))
        return Results.BadRequest("Название книги не может быть пустым");

    book.Id = BookStore.Books.Any() ? BookStore.Books.Max(x => x.Id) + 1 : 1;
    BookStore.Books.Add(book);

    return Results.Created($"/api/books/{book.Id}", book);
});

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

app.MapDelete("/api/books/{id}", (int id) =>
{
    var book = BookStore.Books.FirstOrDefault(x => x.Id == id);
    if (book == null) return Results.NotFound();

    BookStore.Books.Remove(book);
    return Results.NoContent();
});

// Запуск с поддержкой порта Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");