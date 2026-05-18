var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/api/inventory/{productId}", (string productId) =>
{
    // Simulate a random delay to make our traces look realistic
    Thread.Sleep(Random.Shared.Next(10, 100));

    // Simulate a random failure (10% chance) to trigger alerts later
    if (Random.Shared.Next(1, 10) == 1)
    {
        return Results.StatusCode(500);
    }

    return Results.Ok(new { ProductId = productId, InStock = true, Quantity = Random.Shared.Next(1, 50) });
});

app.Run();