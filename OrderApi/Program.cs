var builder = WebApplication.CreateBuilder(args);

// Register HttpClient to call InventoryApi
builder.Services.AddHttpClient("InventoryClient", client =>
{
    // We will use K8s DNS or localhost depending on environment
    var inventoryUrl = builder.Configuration["InventoryApiUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(inventoryUrl);
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/api/orders", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("InventoryClient");
    var productId = Guid.NewGuid().ToString().Substring(0, 8);

    // Simulate some local processing time
    Thread.Sleep(Random.Shared.Next(5, 20));

    // Call the Inventory service
    var response = await client.GetAsync($"/api/inventory/{productId}");

    if (response.IsSuccessStatusCode)
    {
        return Results.Ok(new { OrderId = Guid.NewGuid(), Status = "Created" });
    }

    return Results.StatusCode(500);
});

app.Run();