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
    public int DeveloperId { get; set; }
    public Developer Developer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}

public class Developer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public ICollection<Game> Games { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public ICollection<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public DateTime OrderDate { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }
    public int Quantity { get; set; }
}

public class StoreContext : DbContext
{
    public DbSet<Game> Games { get; set; }
    public DbSet<Developer> Developers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=GameShopDb;Trusted_Connection=True;");
    }
}

class Program
{
    static void Main()
    {
        using (var db = new StoreContext())
        {
            db.Database.EnsureCreated();

            if (!db.Developers.Any())
            {
                var dev1 = new Developer { Name = "Valve", Country = "USA" };
                var dev2 = new Developer { Name = "CD Projekt Red", Country = "Poland" };
                var dev3 = new Developer { Name = "Ubisoft", Country = "France" };
                var dev4 = new Developer { Name = "Rockstar Games", Country = "USA" };
                var dev5 = new Developer { Name = "FromSoftware", Country = "Japan" };
                db.Developers.AddRange(dev1, dev2, dev3, dev4, dev5);
                db.SaveChanges();

                var games = new List<Game>
                {
                    new Game { Title = "CS2", Price = 0, ReleaseYear = 2023, DeveloperId = dev1.Id },
                    new Game { Title = "Dota 2", Price = 0, ReleaseYear = 2013, DeveloperId = dev1.Id },
                    new Game { Title = "Cyberpunk 2077", Price = 59.99m, ReleaseYear = 2020, DeveloperId = dev2.Id },
                    new Game { Title = "Witcher 3", Price = 39.99m, ReleaseYear = 2015, DeveloperId = dev2.Id },
                    new Game { Title = "Far Cry 6", Price = 49.99m, ReleaseYear = 2021, DeveloperId = dev3.Id },
                    new Game { Title = "Assassins Creed Valhalla", Price = 59.99m, ReleaseYear = 2020, DeveloperId = dev3.Id },
                    new Game { Title = "GTA V", Price = 29.99m, ReleaseYear = 2013, DeveloperId = dev4.Id },
                    new Game { Title = "Red Dead Redemption 2", Price = 59.99m, ReleaseYear = 2018, DeveloperId = dev4.Id },
                    new Game { Title = "Elden Ring", Price = 59.99m, ReleaseYear = 2022, DeveloperId = dev5.Id },
                    new Game { Title = "Dark Souls 3", Price = 39.99m, ReleaseYear = 2016, DeveloperId = dev5.Id }
                };
                db.Games.AddRange(games);
                db.SaveChanges();

                var customers = new List<Customer>();
                for (int i = 1; i <= 8; i++)
                {
                    customers.Add(new Customer { FullName = $"User {i}", Email = $"user{i}@gmail.com" });
                }
                db.Customers.AddRange(customers);
                db.SaveChanges();

                var orders = new List<Order>();
                var rand = new Random();
                for (int i = 1; i <= 10; i++)
                {
                    orders.Add(new Order 
                    { 
                        CustomerId = customers[rand.Next(customers.Count)].Id, 
                        OrderDate = DateTime.Now.AddDays(-rand.Next(30)) 
                    });
                }
                db.Orders.AddRange(orders);
                db.SaveChanges();

                var allGames = db.Games.ToList();
                var allOrders = db.Orders.ToList();
                var orderItems = new List<OrderItem>();
                for (int i = 0; i < 20; i++)
                {
                    orderItems.Add(new OrderItem
                    {
                        OrderId = allOrders[rand.Next(allOrders.Count)].Id,
                        GameId = allGames[rand.Next(allGames.Count)].Id,
                        Quantity = rand.Next(1, 3)
                    });
                }
                db.OrderItems.AddRange(orderItems);
                db.SaveChanges();
            }

            Console.WriteLine("--- 1. Ігри з розробниками ---");
            var query1 = db.Games.Include(g => g.Developer).ToList();
            foreach (var g in query1)
                Console.WriteLine($"{g.Title} | {g.Developer.Name}");

            Console.WriteLine("\n--- 2. Замовлення з клієнтами та іграми ---");
            var query2 = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Game)
                .ToList();
            foreach (var o in query2)
            {
                var titles = string.Join(", ", o.OrderItems.Select(oi => oi.Game.Title));
                Console.WriteLine($"Замовлення #{o.Id} | Клієнт: {o.Customer.FullName} | Ігри: {titles}");
            }

            Console.WriteLine("\n--- 3. Сума кожного замовлення ---");
            var query3 = db.Orders
                .Select(o => new {
                    OrderId = o.Id,
                    Total = o.OrderItems.Sum(oi => oi.Quantity * oi.Game.Price)
                }).ToList();
            foreach (var res in query3)
                Console.WriteLine($"Замовлення #{res.OrderId} | Сума: {res.Total}");

            Console.WriteLine("\n--- 4. Топ 3 найдорожчі ігри ---");
            var query4 = db.Games.OrderByDescending(g => g.Price).Take(3).ToList();
            foreach (var g in query4)
                Console.WriteLine($"{g.Title} - {g.Price}");

            Console.WriteLine("\n--- 5. Клієнти з більш ніж 1 замовленням ---");
            var query5 = db.Customers
                .Where(c => c.Orders.Count > 1)
                .ToList();
            foreach (var c in query5)
                Console.WriteLine($"{c.FullName} (Замовлень: {c.Orders.Count})");

            Console.WriteLine("\n--- 6. Загальний дохід магазину ---");
            var query6 = db.OrderItems.Sum(oi => oi.Quantity * oi.Game.Price);
            Console.WriteLine($"Дохід: {query6}");
        }
    }
}
