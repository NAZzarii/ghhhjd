using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public int ReleaseYear { get; set; }
    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; }
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
}

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string AgeRating { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();
}

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public int FoundedYear { get; set; }
    public string Website { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();
}

public class AppContextDb : DbContext
{
    public DbSet<Game> Games { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Publisher> Publishers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=GameStoreCrudDb;Trusted_Connection=True;");
    }
}

class Program
{
    static void Main()
    {
        using (var db = new AppContextDb())
        {
            db.Database.EnsureCreated();

            // CRUD & Custom Queries Demo
            CreateSampleData(db);

            Console.WriteLine("--- Eager Loading (Games & Genres) ---");
            var gamesWithGenres = db.Games.Include(g => g.Genres).ToList();
            foreach (var g in gamesWithGenres)
                Console.WriteLine($"{g.Title} | Жанри: {string.Join(", ", g.Genres.Select(x => x.Name))}");

            Console.WriteLine("\n--- Explicit Loading (Games & Publishers) ---");
            var gameToLoad = db.Games.FirstOrDefault();
            if (gameToLoad != null)
            {
                db.Entry(gameToLoad).Reference(g => g.Publisher).Load();
                Console.WriteLine($"Гра: {gameToLoad.Title} | Видавець: {gameToLoad.Publisher?.Name}");
            }

            Console.WriteLine("\n--- Всі ігри вказаного жанру ---");
            GetGamesByGenre(db, "Action");

            Console.WriteLine("\n--- Жанри вказаної гри ---");
            GetGenresByGame(db, "Cyberpunk 2077");

            Console.WriteLine("\n--- Всі ігри видавця ---");
            GetGamesByPublisher(db, "CD Projekt Red");
        }
    }

    static void CreateSampleData(AppContextDb db)
    {
        if (db.Publishers.Any()) return;

        var pub = new Publisher { Name = "CD Projekt Red", Country = "Poland", FoundedYear = 1994, Website = "cdprojekt.com" };
        var genre1 = new Genre { Name = "Action", Description = "Fast paced", AgeRating = "18+" };
        var genre2 = new Genre { Name = "RPG", Description = "Role play", AgeRating = "16+" };

        var game = new Game
        {
            Title = "Cyberpunk 2077",
            Price = 59.99m,
            ReleaseYear = 2020,
            Publisher = pub,
            Genres = new List<Genre> { genre1, genre2 }
        };

        db.Games.Add(game);
        db.SaveChanges();
    }

    static void GetGamesByGenre(AppContextDb db, string genreName)
    {
        var games = db.Genres
            .Where(g => g.Name == genreName)
            .SelectMany(g => g.Games)
            .ToList();

        foreach (var g in games)
            Console.WriteLine($"- {g.Title}");
    }

    static void GetGenresByGame(AppContextDb db, string gameTitle)
    {
        var game = db.Games.Include(g => g.Genres).FirstOrDefault(g => g.Title == gameTitle);
        if (game != null)
        {
            foreach (var genre in game.Genres)
                Console.WriteLine($"- {genre.Name}");
        }
    }

    static void GetGamesByPublisher(AppContextDb db, string publisherName)
    {
        var games = db.Games
            .Include(g => g.Publisher)
            .Where(g => g.Publisher.Name == publisherName)
            .ToList();

        foreach (var g in games)
            Console.WriteLine($"- {g.Title}");
    }
}
