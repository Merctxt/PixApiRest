using System.Reflection;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PixApiRest.Data;
using PixApiRest.Middleware;
using PixApiRest.Services;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure server address and port from environment
var serverAddress = Environment.GetEnvironmentVariable("SERVER_ADDRESS") ?? "0.0.0.0";
var serverPort = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "8080";
builder.WebHost.UseUrls($"http://{serverAddress}:{serverPort}");

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure PostgreSQL Database
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var connectionString = ConvertPostgresUrlToConnectionString(databaseUrl);
    builder.Services.AddDbContext<PixDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    throw new InvalidOperationException("DATABASE_URL environment variable is not set");
}

// Register services
builder.Services.AddScoped<PixPayloadService>();
builder.Services.AddScoped<QrCodeService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddSingleton<RateLimitService>();

// Register background service for cleaning expired payments
builder.Services.AddHostedService<PaymentCleanupService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PIX API REST",
        Description = "API para gerenciamento de pagamentos PIX",
        Contact = new OpenApiContact
        {
            Name = "Contact Support",
            Url = new Uri("https://github.com/Merctxt")
        }
    });

    // Include XML comments for Swagger documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Apply migrations and create database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PixDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PIX API REST v1");
    options.RoutePrefix = string.Empty; // Swagger at root
});

app.UseAuthorization();
app.MapControllers();

app.Run();

// Convert PostgreSQL URL format to Npgsql connection string
static string ConvertPostgresUrlToConnectionString(string databaseUrl)
{
    // Format: postgresql://user:password@host:port/database
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}