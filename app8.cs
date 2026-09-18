using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

public class ShopContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=ShopSeedDb;Trusted_Connection=True;");
    }
}

class Program
{
    static void Main()
    {
        using (var db = new ShopContext())
        {
            db.Database.EnsureCreated();

            if (!db.Categories.Any())
            {
                for (int i = 1; i <= 10; i++)
                {
                    var category = new Category { Name = $"Категорія {i}" };
                    for (int j = 1; j <= 5; j++)
                    {
                        category.Products.Add(new Product
                        {
                            Name = `Товар {j} (Категорія {i})`,
                            Price = i * 100 + j * 25,
                            Description = `Детальний опис товару {j} з категорії {i}.`,
                            ImageUrl = "https://via.placeholder.com/150"
                        });
                    }
                    db.Categories.Add(category);
                }
                db.SaveChanges();
            }

            RenderHomePage(db);
            RenderProductDetailsPage(db);
        }
    }

    static void RenderHomePage(ShopContext db)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("             ГОЛОВНА СТОРІНКА           ");
        Console.WriteLine("========================================");

        var products = db.Products.Include(p => p.Category).Take(10).ToList();
        foreach (var p in products)
        {
            Console.WriteLine($"[{p.Category.Name}] {p.Name} — {p.Price} грн");
            Console.WriteLine($"Опис: {p.Description}");
            Console.WriteLine("----------------------------------------");
        }
    }

    static void RenderProductDetailsPage(ShopContext db)
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("         ДЕТАЛІ ПРО ТОВАР (№1)          ");
        Console.WriteLine("========================================");

        var product = db.Products.Include(p => p.Category).FirstOrDefault();
        if (product != null)
        {
            Console.WriteLine($"Назва: {product.Name}");
            Console.WriteLine($"Категорія: {product.Category.Name}");
            Console.WriteLine($"Ціна: {product.Price} грн");
            Console.WriteLine($"Зображення: {product.ImageUrl}");
            Console.WriteLine($"Повний опис: {product.Description}");
        }
        Console.WriteLine("========================================");
    }
}
