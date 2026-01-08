using Propgic.Application.Interfaces;
using Propgic.Application.Mappings;
using Propgic.Application.Services;
using Propgic.Application.Services.Analysers;
using Propgic.Domain.Interfaces;
using Propgic.Infrastructure.Data;
using Propgic.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Propgic.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("Infrastructure");
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPropertyAnalyserService, PropertyAnalyserService>();

// Register HttpClient for property data fetchers
builder.Services.AddHttpClient();

// Register ChatGPT URL Discovery Service
var openAIApiKey = builder.Configuration["OpenAI:ApiKey"] ?? "";
var azureEndpoint = builder.Configuration["OpenAI:AzureEndpoint"];
var azureDeploymentName = builder.Configuration["OpenAI:AzureDeploymentName"];
builder.Services.AddSingleton(new Propgic.Application.Services.PropertyDataFetchers.ChatGptUrlDiscoveryService(
    openAIApiKey, azureEndpoint, azureDeploymentName));

// Register Selenium Web Scraper Service (Singleton for better performance)
builder.Services.AddSingleton<Propgic.Application.Services.PropertyDataFetchers.SeleniumWebScraperService>();

// Register Property Data Fetchers
builder.Services.AddScoped<Propgic.Application.Services.PropertyDataFetchers.IPropertyDataFetcher, Propgic.Application.Services.PropertyDataFetchers.DomainComAuFetcher>();
builder.Services.AddScoped<Propgic.Application.Services.PropertyDataFetchers.IPropertyDataFetcher, Propgic.Application.Services.PropertyDataFetchers.RealEstateComAuFetcher>();
builder.Services.AddScoped<Propgic.Application.Services.PropertyDataFetchers.IPropertyDataFetcher, Propgic.Application.Services.PropertyDataFetchers.PropertyComAuFetcher>();
builder.Services.AddScoped<Propgic.Application.Services.PropertyDataFetchers.PropertyDataAggregator>();

// Register Property Analysers
builder.Services.AddScoped<IPropertyAnalyser, PropertyAnchorAnalyser>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure database is created and apply migrations
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<ApplicationDbContext>();
//        var logger = services.GetRequiredService<ILogger<Program>>();

//        logger.LogInformation("Ensuring database exists...");
//        context.Database.EnsureCreated();

//        logger.LogInformation("Applying pending migrations...");
//        context.Database.Migrate();

//        logger.LogInformation("Database migration completed successfully.");
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while migrating the database.");
//        throw; // Re-throw to prevent app from starting with broken database
//    }
//}

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
