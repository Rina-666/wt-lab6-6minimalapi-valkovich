using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_valkovich.Data;
using wt_lab7_7postgres_efcore_valkovich.Models;
using Npgsql;
using System;

// --- 0. ЖЕСТКАЯ ОПТИМИЗАЦИЯ ПАМЯТИ ---
AppContext.SetSwitch("System.GC.Server", false);
AppContext.SetSwitch("System.GC.Concurrent", false);
AppContext.SetSwitch("System.GC.HeapHardLimit", true);

var builder = WebApplication.CreateBuilder(args);

// --- 1. Настройка подключения ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    if (databaseUrl.StartsWith("postgres://"))
    {
        databaseUrl = databaseUrl.Replace("postgres://", "postgresql://");
    }
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);

    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                       $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Disable;";
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Строка подключения не найдена!");
}

// --- Ограничиваем пул соединений до 1 для экономии памяти ---
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 1;
dataSourceBuilder.ConnectionStringBuilder.MinPoolSize = 1;
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource)
);

// --- 2. НАСТРОЙКА ПОРТА (ИСПРАВЛЕНО) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// --- 3. Применение миграций с повторными попытками ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retryCount = 3;
    while (retryCount > 0)
    {
        try
        {
            Console.WriteLine($"Попытка подключения к БД. Осталось попыток: {retryCount}");
            db.Database.Migrate();
            Console.WriteLine("Миграции успешно применены!");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка БД: {ex.Message}");
            retryCount--;
            if (retryCount == 0) throw;
            Thread.Sleep(2000);
        }
    }
}

// --- 4. REST API ---
app.MapGet("/", () => "Minimal API работает!");
app.MapGet("/api/info", () => new { Application = "Web Technologies", Version = "7.0" });

app.MapGet("/api/books", async (string? search, AppDbContext db) =>
{
    var query = db.Books.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(x => x.Title.ToLower().Contains(search.ToLower()));
    return Results.Ok(await query.ToListAsync());
});

app.MapGet("/api/books/{id}", async (int id, AppDbContext db) =>
{
    var book = await db.Books.FirstOrDefaultAsync(x => x.Id == id);
    return book == null ? Results.NotFound() : Results.Ok(book);
});

app.MapPost("/api/books", async (Book book, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(book.Title))
        return Results.BadRequest("Название не может быть пустым");
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/api/books/{book.Id}", book);
});

app.MapPut("/api/books/{id}", async (int id, Book updatedBook, AppDbContext db) =>
{
    var book = await db.Books.FirstOrDefaultAsync(x => x.Id == id);
    if (book == null) return Results.NotFound();
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Price = updatedBook.Price;
    book.IsAvailable = updatedBook.IsAvailable;
    await db.SaveChangesAsync();
    return Results.Ok(book);
});

app.MapDelete("/api/books/{id}", async (int id, AppDbContext db) =>
{
    var book = await db.Books.FirstOrDefaultAsync(x => x.Id == id);
    if (book == null) return Results.NotFound();
    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// --- 5. LINQ-запросы ---
app.MapGet("/api/books/expensive", async (AppDbContext db) =>
    Results.Ok(await db.Books.Where(x => x.Price > 500).ToListAsync()));
app.MapGet("/api/books/sorted", async (AppDbContext db) =>
    Results.Ok(await db.Books.OrderBy(x => x.Price).ToListAsync()));
app.MapGet("/api/books/available", async (AppDbContext db) =>
    Results.Ok(await db.Books.Where(x => x.IsAvailable).OrderBy(x => x.Title).ToListAsync()));
app.MapGet("/api/search/by-author", async (string author, AppDbContext db) =>
    Results.Ok(await db.Books.Where(x => x.Author.ToLower().Contains(author.ToLower())).ToListAsync()));

// --- 6. Запуск ---
app.Run();