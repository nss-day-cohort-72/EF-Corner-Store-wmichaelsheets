using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//**************************************************** CASHIER API ENDPOINTS *******************************************

//Get all cashiers
app.MapGet("/api/cashiers", async (CornerStoreDbContext context) =>
{
    return await context.Cashiers
        .Select(c => new CashierDTO { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName })
        .ToListAsync();
});

// Add a cashier
app.MapPost("/api/cashiers", (CornerStoreDbContext db, Cashier cashier) =>
{
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/cashiers/{cashier.Id}", cashier);
});

// Get a cashier (including their orders and the orders' products)
app.MapGet("/api/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    var cashier = db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
        .FirstOrDefault(c => c.Id == id);

    if (cashier == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(cashier);
});


//**************************************************** PRODUCTS API ENDPOINTS *******************************************

//Get all products
app.MapGet("/api/products", async (CornerStoreDbContext db, string? search) =>
{
    var query = db.Products
        .Include(p => p.Category)
        .Select(p => new ProductDTO
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Price = p.Price,
            Brand = p.Brand,
            CategoryId = p.CategoryId,

        });

    if (!string.IsNullOrEmpty(search))
    {
        search = search.ToLower();
        query = query.Where(p => p.ProductName.ToLower().Contains(search));
    }

    return await query.ToListAsync();
});

// Add a product
app.MapPost("/api/products", async (CornerStoreDbContext db, ProductDTO productDto) =>
{
    var product = new Product
    {
        ProductName = productDto.ProductName,
        Price = productDto.Price,
        Brand = productDto.Brand,
        CategoryId = productDto.CategoryId
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    productDto.Id = product.Id;
    return Results.Created($"/api/products/{product.Id}", productDto);
});

// Update a product
app.MapPut("/api/products/{id}", async (int id, ProductDTO productDto, CornerStoreDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product == null)
    {
        return Results.NotFound();
    }

    product.ProductName = productDto.ProductName;
    product.Price = productDto.Price;
    product.Brand = productDto.Brand;
    product.CategoryId = productDto.CategoryId;

    await db.SaveChangesAsync();

    return Results.NoContent();
});


//**************************************************** ORDERS API ENDPOINTS *******************************************

// Get an order w/ details
app.MapGet("/api/orders/{id}", async (int id, CornerStoreDbContext db) =>
{
    var order = await db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
});

// Get all orders. 
app.MapGet("/api/orders", async (CornerStoreDbContext db, DateTime? orderDate) =>
{
    var query = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .AsQueryable();

    if (orderDate.HasValue)
    {
        query = query.Where(o => o.PaidOnDate == orderDate.Value.Date);
    }

    var orders = await query.ToListAsync();
    return Results.Ok(orders);
});


// Delete an order
app.MapDelete("/api/orders/{id}", async (int id, CornerStoreDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order == null)
    {
        return Results.NotFound();
    }

    db.Orders.Remove(order);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// Create an Order with products
app.MapPost("/api/orders", async (CornerStoreDbContext db, OrderDTO orderDto) =>
{
    var order = new Order
    {
        CashierId = orderDto.CashierId,
        PaidOnDate = orderDto.PaidOnDate
    };

    if (orderDto.OrderProducts != null)
    {
        foreach (var orderProduct in orderDto.OrderProducts)
        {
            order.OrderProducts.Add(new OrderProduct
            {
                ProductId = orderProduct.ProductId,
                Quantity = orderProduct.Quantity
            });
        }
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    await db.Entry(order)
        .Reference(o => o.Cashier)
        .LoadAsync();

    await db.Entry(order)
        .Collection(o => o.OrderProducts)
        .Query()
        .Include(op => op.Product)
        .LoadAsync();

    var savedOrderDto = new OrderDTO
    {
        Id = order.Id,
        CashierId = order.CashierId,
        OrderProducts = order.OrderProducts.ToArray(),
        PaidOnDate = order.PaidOnDate ?? DateTime.MinValue
    };

    return Results.Created($"/api/orders/{order.Id}", savedOrderDto);
});


app.Run();

//don't move or change this!
public partial class Program { }