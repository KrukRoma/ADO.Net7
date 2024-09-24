using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp2
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<City> Cities { get; set; } = new List<City>(); 
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public ICollection<Shop> Shops { get; set; } = new List<Shop>(); 
    }

    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public string ParkingArea { get; set; }
        public ICollection<Worker> Workers { get; set; } = new List<Worker>(); 
        public ICollection<Product> Products { get; set; } = new List<Product>(); 
    }

    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Worker> Workers { get; set; } = new List<Worker>(); 
    }

    public class Worker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public decimal Salary { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int PositionId { get; set; }
        public Position Position { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
    }

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
        public decimal Discount { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int Quantity { get; set; }
        public bool IsInStock { get; set; }
        public int ShopId { get; set; } 
        public Shop Shop { get; set; } 
    }

    public class ShopContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=ShopDatabase.db"); 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>()
                .HasMany(c => c.Cities)
                .WithOne(c => c.Country)
                .HasForeignKey(c => c.CountryId);

            modelBuilder.Entity<City>()
                .HasMany(c => c.Shops)
                .WithOne(s => s.City)
                .HasForeignKey(s => s.CityId);

            modelBuilder.Entity<Position>()
                .HasMany(p => p.Workers)
                .WithOne(w => w.Position)
                .HasForeignKey(w => w.PositionId);

            modelBuilder.Entity<Shop>()
                .HasMany(s => s.Workers)
                .WithOne(w => w.Shop)
                .HasForeignKey(w => w.ShopId);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);
        }
    }

    public class ShopSeeder
    {
        public static void Initialize(ShopContext context)
        {
            context.Database.EnsureCreated();

            if (context.Countries.Any())
            {
                return; 
            }

            var countries = new List<Country>
{
    new Country { Name = "Ukraine" },
    new Country { Name = "Poland" }
};

            var cities = new List<City>
{
    new City { Name = "Kyiv", Country = countries[0] },
    new City { Name = "Warsaw", Country = countries[1] }
};

            var positions = new List<Position>
{
    new Position { Name = "Manager" },
    new Position { Name = "Cashier" }
};

            context.Countries.AddRange(countries);
            context.Cities.AddRange(cities);
            context.Positions.AddRange(positions);
            context.SaveChanges(); 

            var shops = new List<Shop>
{
    new Shop { Name = "Shop A", Address = "Street 1", CityId = cities[0].Id, ParkingArea = "Yes" },
    new Shop { Name = "Shop B", Address = "Street 2", CityId = cities[1].Id, ParkingArea = "No" }
};

            context.Shops.AddRange(shops);
            context.SaveChanges(); 

            var workers = new List<Worker>
{
    new Worker { Name = "John", Surname = "Doe", Salary = 5000, Email = "john@example.com", PhoneNumber = "123456789", PositionId = positions[0].Id, ShopId = shops[0].Id },
    new Worker { Name = "Jane", Surname = "Doe", Salary = 4000, Email = "jane@example.com", PhoneNumber = "987654321", PositionId = positions[1].Id, ShopId = shops[1].Id }
};

            context.Workers.AddRange(workers);
            context.SaveChanges(); 

            var categories = new List<Category>
{
    new Category { Name = "Electronics" },
    new Category { Name = "Clothing" }
};

            context.Categories.AddRange(categories);
            context.SaveChanges(); 

            var products = new List<Product>
{
    new Product { Name = "Laptop", Price = 1000, Discount = 100, CategoryId = categories[0].Id, Quantity = 10, IsInStock = true, ShopId = shops[0].Id },
    new Product { Name = "T-Shirt", Price = 20, Discount = 5, CategoryId = categories[1].Id, Quantity = 50, IsInStock = true, ShopId = shops[1].Id }
};

            context.Products.AddRange(products);
            context.SaveChanges(); 
        }

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using (var context = new ShopContext())
            {
                ShopSeeder.Initialize(context);

                var shops = context.Shops
    .Include(s => s.Workers)
    .Include(s => s.Products)
        .ThenInclude(p => p.Category)
    .Include(s => s.City) 
    .ToList();

                foreach (var shop in shops)
                {
                    Console.WriteLine($"Shop: {shop.Name}, Address: {shop.Address}, City: {shop.City?.Name ?? "Unknown"}");

                    Console.WriteLine("Workers:");
                    if (shop.Workers.Any())
                    {
                        foreach (var worker in shop.Workers)
                        {
                            Console.WriteLine($"- {worker.Name} {worker.Surname}, Position: {worker.Position?.Name ?? "Unknown"}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("- No workers available.");
                    }

                    Console.WriteLine("Products:");
                    if (shop.Products.Any())
                    {
                        foreach (var product in shop.Products)
                        {
                            Console.WriteLine($"- {product.Name}, Price: {product.Price}, In Stock: {product.IsInStock}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("- No products available.");
                    }

                    Console.WriteLine(); 
                }

            }
        }
    }
}
