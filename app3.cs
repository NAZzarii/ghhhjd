public class Game
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Studio { get; set; }
    public string Genre { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string GameMode { get; set; }
    public long SoldCopies { get; set; }
}

using Microsoft.EntityFrameworkCore;

public class GameContext : DbContext
{
    public DbSet<Game> Games { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=GameStoreDb;Trusted_Connection=True;");
    }
}

using System;
using System.Linq;

class Program
{
    static void Main()
    {
        using (var db = new GameContext())
        {
            db.Database.EnsureCreated();

            if (!db.Games.Any())
            {
                db.Games.Add(new Game 
                { 
                    Title = "The Witcher 3", 
                    Studio = "CD Projekt Red", 
                    Genre = "RPG", 
                    ReleaseDate = new DateTime(2015, 5, 19),
                    GameMode = "Однокористувацький",
                    SoldCopies = 50000000
                });
                
                db.Games.Add(new Game 
                { 
                    Title = "Dota 2", 
                    Studio = "Valve", 
                    Genre = "MOBA", 
                    ReleaseDate = new DateTime(2013, 7, 9),
                    GameMode = "Багатокористувацький",
                    SoldCopies = 0
                });

                db.SaveChanges();
            }

            foreach (var g in db.Games.ToList())
            {
                Console.WriteLine($"{g.Title} | {g.Studio} | {g.Genre} | {g.ReleaseDate.ToShortDateString()} | {g.GameMode} | {g.SoldCopies}");
            }
        }
    }
}
