using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_valkovich.Data;
using wt_lab7_7postgres_efcore_valkovich.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Настройка подключения (локально и на Render) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];

// Если мы на Render (есть DATABASE_URL), преобразуем её в строку подключения
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                       $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Disable";
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// --- 2. Автоматическое применение миграций при запуске ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Создает таблицы, если их нет
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- 3. REST API (CRUD) ---

// Проверка работы
app.MapGet("/", () => "Minimal API работает!");

// Информация о приложении
app.MapGet("/api/info", () => new { Application = "Web Technologies", Version = "7.0" });

// GET: Все книги + поиск по названию
app.MapGet("/api/books", async (string? search, AppDbContext db) =>
{
    var query = db.Books.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(x => x.Title.ToLower().Contains(search.ToLower()));
    }
    return Results.Ok(await query.ToListAsync());
});

// GET: Книга по ID
app.MapGet("/api/books/{id}", async (int id, AppDbContext db) =>
{
    var book = await db.Books.FirstOrDefaultAsync(x => x.Id == id);
    return book == null ? Results.NotFound() : Results.Ok(book);
});

// POST: Создание книги
app.MapPost("/api/books", async (Book book, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(book.Title))
        return Results.BadRequest("Название не может быть пустым");

    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/api/books/{book.Id}", book);
});

// PUT: Обновление книги
app.MapPut("/api/books/{id}", async (int id, Book updatedBook, AppDbContext db) =>
{
    var book = await db.Books.FirstOrDefaultAsync(x => x.Id == id);
    if (book == null) return Results.NotFound();

    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Price = updatedBook.Price;
    book.IsAvailable = updatedBook.IsAvailable;
    // CreatedAt не меняем

    await db.SaveChangesAsync();
    return Results.Ok(book);
});

// DELETE: Удаление книги
app.MapDelete("/api/books/{id}", async (int id, AppDbContext db) =>
{
    var book = await db.Books.FirstOrDefaultAsync(x => x.Id == id);
    if (book == null) return Results.NotFound();

    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// --- 4. LINQ-запросы (для Блока 5) ---

// Фильтрация по цене (книги дороже 500)
app.MapGet("/api/books/expensive", async (AppDbContext db) =>
    Results.Ok(await db.Books.Where(x => x.Price > 500).ToListAsync()));

// Сортировка по цене
app.MapGet("/api/books/sorted", async (AppDbContext db) =>
    Results.Ok(await db.Books.OrderBy(x => x.Price).ToListAsync()));

// Выборка с условием (только в наличии, сортировка по названию)
app.MapGet("/api/books/available", async (AppDbContext db) =>
    Results.Ok(await db.Books.Where(x => x.IsAvailable).OrderBy(x => x.Title).ToListAsync()));

// Собственный запрос: поиск по автору
app.MapGet("/api/search/by-author", async (string author, AppDbContext db) =>
    Results.Ok(await db.Books.Where(x => x.Author.ToLower().Contains(author.ToLower())).ToListAsync()));


// --- 5. Запуск ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");