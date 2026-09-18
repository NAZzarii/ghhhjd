using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public double Rating { get; set; } 
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class ShopContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=ShopCrudImagesDb;Trusted_Connection=True;");
    }
}

class Program
{
    private static readonly string ImagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
    private static bool isAdmin = true; 

    static void Main()
    {
        if (!Directory.Exists(ImagesDirectory))
            Directory.CreateDirectory(ImagesDirectory);

        using (var db = new ShopContext())
        {
            db.Database.EnsureCreated();

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Поточна роль: {(isAdmin ? "Адмін" : "Гість")}");
                Console.WriteLine("----------------------------------------");
                
                if (isAdmin)
                {
                    Console.WriteLine("1. [Товари] Показати товари (з сортуванням)");
                    Console.WriteLine("2. [Товари] Додати товар");
                    Console.WriteLine("3. [Товари] Редагувати товар");
                    Console.WriteLine("4. [Товари] Видалити товар");
                }
                
                Console.WriteLine("5. Змінити роль (Адмін / Користувач)");
                Console.WriteLine("6. Вихід");
                Console.Write("Виберіть дію: ");
                var choice = Console.ReadLine();

                if (choice == "1" && isAdmin) ShowProducts(db);
                else if (choice == "2" && isAdmin) AddProduct(db);
                else if (choice == "3" && isAdmin) UpdateProduct(db);
                else if (choice == "4" && isAdmin) DeleteProduct(db);
                else if (choice == "5") isAdmin = !isAdmin;
                else if (choice == "6") break;
                else if (!isAdmin && (choice == "1" || choice == "2" || choice == "3" || choice == "4"))
                {
                    Console.WriteLine("Доступ заборонено. Тільки для адміністраторів.");
                    Console.ReadKey();
                }
            }
        }
    }
    static void ShowProducts(ShopContext db)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Сортування товарів:");
            Console.WriteLine("1. За ціною (зростання)");
            Console.WriteLine("2. За ціною (спадання)");
            Console.WriteLine("3. За назвою");
            Console.WriteLine("4. За рейтингом (від найбільшого)");
            Console.WriteLine("5. Без сортування (назад / вихід до меню)");
            Console.Write("Виберіть варіант сортування: ");
            var sortChoice = Console.ReadLine();

            IQueryable<Product> query = db.Products.Include(p => p.Category);

            switch (sortChoice)
            {
                case "1": query = query.OrderBy(p => p.Price); break;
                case "2": query = query.OrderByDescending(p => p.Price); break;
                case "3": query = query.OrderBy(p => p.Name); break;
                case "4": query = query.OrderByDescending(p => p.Rating); break;
                default: return;
            }

            Console.Clear();
            var products = query.ToList();
            foreach (var p in products)
            {
                Console.WriteLine($"ID: {p.Id} | [{p.Category?.Name}] {p.Name} — {p.Price} грн | Рейтинг: {p.Rating}");
                Console.WriteLine($"Фото: {p.ImagePath}");
                Console.WriteLine($"Опис: {p.Description}");
                Console.WriteLine(new string('-', 40));
            }
            Console.WriteLine("\nНатисніть будь-яку клавішу для повернення до меню сортування...");
            Console.ReadKey();
        }
    }

    static void AddProduct(ShopContext db)
    {
        Console.Clear();
        Console.Write("Назва товару: ");
        string name = Console.ReadLine();

        Console.Write("Ціна: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price)) return;

        Console.Write("Рейтинг (наприклад, 4.5): ");
        double.TryParse(Console.ReadLine(), out double rating);

        Console.Write("Опис: ");
        string desc = Console.ReadLine();

        Console.Write("Шлях до файлу зображення: ");
        string sourceFilePath = Console.ReadLine()?.Trim('"');

        string savedRelativePath = null;
        if (!string.IsNullOrEmpty(sourceFilePath) && File.Exists(sourceFilePath))
        {
            string ext = Path.GetExtension(sourceFilePath);
            string fileName = Guid.NewGuid().ToString() + ext;
            savedRelativePath = Path.Combine("images", fileName);
            string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savedRelativePath);
            File.Copy(sourceFilePath, destPath);
        }

        var category = db.Categories.FirstOrDefault();
        if (category == null)
        {
            category = new Category { Name = "Загальна" };
            db.Categories.Add(category);
            db.SaveChanges();
        }

        var product = new Product
        {
            Name = name,
            Price = price,
            Rating = rating,
            Description = desc,
            ImagePath = savedRelativePath,
            CategoryId = category.Id
        };

        db.Products.Add(product);
        db.SaveChanges();
        Console.WriteLine("Товар успішно додано!");
        Console.ReadKey();
    }

    static void UpdateProduct(ShopContext db)
    {
        Console.Clear();
        Console.Write("Введіть ID товару для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        var product = db.Products.Find(id);
        if (product == null) { Console.WriteLine("Товар не знайдено."); Console.ReadKey(); return; }

        Console.Write($"Нова назва [{product.Name}]: ");
        string name = Console.ReadLine();
        if (!string.IsNullOrEmpty(name)) product.Name = name;

        Console.Write($"Нова ціна [{product.Price}]: ");
        string priceStr = Console.ReadLine();
        if (decimal.TryParse(priceStr, out decimal price)) product.Price = price;

        Console.Write($"Новий рейтинг [{product.Rating}]: ");
        string ratingStr = Console.ReadLine();
        if (double.TryParse(ratingStr, out double rating)) product.Rating = rating;

        Console.Write($"Новий опис [{product.Description}]: ");
        string desc = Console.ReadLine();
        if (!string.IsNullOrEmpty(desc)) product.Description = desc;

        Console.Write("Бажаєте змінити фото? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            Console.Write("Шлях до нового файлу зображення: ");
            string sourceFilePath = Console.ReadLine()?.Trim('"');

            if (!string.IsNullOrEmpty(sourceFilePath) && File.Exists(sourceFilePath))
            {
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string oldFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, product.ImagePath);
                    if (File.Exists(oldFullPath)) File.Delete(oldFullPath);
                }

                string ext = Path.GetExtension(sourceFilePath);
                string fileName = Guid.NewGuid().ToString() + ext;
                string savedRelativePath = Path.Combine("images", fileName);
                string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savedRelativePath);
                File.Copy(sourceFilePath, destPath);

                product.ImagePath = savedRelativePath;
            }
        }

        db.SaveChanges();
        Console.WriteLine("Товар оновлено!");
        Console.ReadKey();
    }

    static void DeleteProduct(ShopContext db)
    {
        Console.Clear();
        Console.Write("Введіть ID товару для видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        var product = db.Products.Find(id);
        if (product == null) { Console.WriteLine("Товар не знайдено."); Console.ReadKey(); return; }

        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, product.ImagePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        db.Products.Remove(product);
        db.SaveChanges();
        Console.WriteLine("Товар та його фото видалено!");
        Console.ReadKey();
    }
}
