using Finaco.Api.Infrastructure;
using Finaco.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Npgsql;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection bulunamadı. .env dosyasını yükleyin: set -a; source .env; set +a");
}

try
{
    var parsedConnectionString = new NpgsqlConnectionStringBuilder(connectionString);

    if (string.IsNullOrWhiteSpace(parsedConnectionString.Host) ||
        string.IsNullOrWhiteSpace(parsedConnectionString.Database) ||
        string.IsNullOrWhiteSpace(parsedConnectionString.Username) ||
        string.IsNullOrWhiteSpace(parsedConnectionString.Password))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection eksik. Host, Database, Username ve Password değerlerini kontrol edin.");
    }
}
catch (ArgumentException exception)
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection geçersiz biçimde. .env içindeki bağlantı dizesini tek tırnak içine alın.",
        exception);
}

builder.Services.AddDbContext<FinacoDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<Finaco.Api.Application.Imports.ImportPreviewService>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors("frontend");

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(entry => entry.Key, entry => new { status = entry.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});
app.MapControllers();
app.MapImportEndpoints();

app.Run();

public partial class Program;

