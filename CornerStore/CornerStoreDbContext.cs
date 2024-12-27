using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{

    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
        new Cashier {Id = 1, FirstName = "Jack", LastName = "Sprat"},
        new Cashier {Id = 2, FirstName = "Mike", LastName = "Hunt"},
        new Cashier {Id = 3, FirstName = "Ben", LastName = "Dover"}
        });

        modelBuilder.Entity<Category>().HasData(new Category[]
            {
        new Category {Id = 1, CategoryName = "Food"},
        new Category {Id = 2, CategoryName = "Beverage"},
        new Category {Id = 3, CategoryName = "Misc"}
            });

        modelBuilder.Entity<Order>().HasData(new Order[]
            {
        new Order {Id = 1, CashierId = 1, PaidOnDate = new DateTime(2024, 12, 1)},
        new Order {Id = 2, CashierId = 2, PaidOnDate = new DateTime(2024, 12, 1)},
        new Order {Id = 3, CashierId = 3, PaidOnDate = new DateTime(2024, 12, 1)},
        new Order {Id = 4, CashierId = 1, PaidOnDate = new DateTime(2024, 12, 1)}
            });

        modelBuilder.Entity<Product>().HasData(new Product[]
        {
        new Product {Id = 1, CategoryId = 1, ProductName = "Pizza", Price = 10.99m, Brand = "Domino's"},
        new Product {Id = 2, CategoryId = 1, ProductName = "Burger", Price = 7.99m, Brand = "McDonald's"},
        new Product {Id = 3, CategoryId = 2, ProductName = "Soda", Price = 2.99m, Brand = "Pepsi"},
        new Product {Id = 4, CategoryId = 3, ProductName = "Chocolate Bar", Price = 3.49m, Brand = "Hershey"  }
        });

        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
        new OrderProduct {Id = 1, OrderId = 1, ProductId = 1, Quantity = 2},
        new OrderProduct {Id = 2, OrderId = 1, ProductId = 2, Quantity = 1},
        new OrderProduct {Id = 3, OrderId = 2, ProductId = 3, Quantity = 3},
        new OrderProduct {Id = 4, OrderId = 3, ProductId = 4, Quantity = 1}
        });

    }
}