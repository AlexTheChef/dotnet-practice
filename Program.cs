
using MySqlConnector;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "dotnet-practice API",
        Version = "v1"
    });
});

builder.Services.AddMySqlDataSource(
    builder.Configuration.GetConnectionString("MariaDb")
    ?? throw new InvalidOperationException("MariaDb konekcija nije podešena."));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/db-check", async (
    MySqlConnection connection,
    CancellationToken cancellationToken) =>
{
    await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT 1";

    var value = await command.ExecuteScalarAsync(cancellationToken);

    return Results.Ok(new { connected = Convert.ToInt32(value) == 1 });
});

app.Run();
