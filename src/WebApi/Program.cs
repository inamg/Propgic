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
builder.Services.AddScoped<IPropertyAnalyserService, PropertyAnalyserService>();

// Register HttpClient for property data fetchers
builder.Services.AddHttpClient();

// Register ChatGPT URL Discovery Service
var openAIApiKey = builder.Configuration["OpenAI:ApiKey"] ?? "";
builder.Services.AddSingleton(new Propgic.Application.Services.PropertyDataFetchers.ChatGptUrlDiscoveryService(openAIApiKey));

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
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Checking database state...");

        // Check if database can be connected
        var canConnect = context.Database.CanConnect();
        logger.LogInformation($"Can connect to database: {canConnect}");

        // Get pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        logger.LogInformation($"Pending migrations count: {pendingMigrations.Count}");
        foreach (var migration in pendingMigrations)
        {
            logger.LogInformation($"Pending migration: {migration}");
        }

        // Get applied migrations
        var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
        logger.LogInformation($"Applied migrations count: {appliedMigrations.Count}");
        foreach (var migration in appliedMigrations)
        {
            logger.LogInformation($"Applied migration: {migration}");
        }

        // If there are no pending migrations but the table doesn't exist, create it manually
        if (pendingMigrations.Count == 0)
        {
            logger.LogWarning("No pending migrations found. Checking if PropertyAnalyses table exists...");

            // Try to query the table to see if it exists
            try
            {
                var tableExists = context.PropertyAnalyses.Any();
                logger.LogInformation($"PropertyAnalyses table exists and has {context.PropertyAnalyses.Count()} records");
            }
            catch (Exception)
            {
                logger.LogError("PropertyAnalyses table does NOT exist. Creating table manually...");

                // Drop table if it exists (to ensure clean state)
                context.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS \"PropertyAnalyses\"");

                // Create the PropertyAnalyses table using raw SQL
                var createTableSql = @"
                    CREATE TABLE ""PropertyAnalyses"" (
                        ""Id"" uuid NOT NULL,
                        ""PropertyAddress"" character varying(500) NOT NULL,
                        ""AnalyserType"" character varying(100) NOT NULL,
                        ""SourceType"" character varying(50) NOT NULL DEFAULT 'Address',
                        ""Status"" character varying(50) NOT NULL,
                        ""AnalysisResult"" text NULL,
                        ""AnalysisScore"" numeric(18,2) NULL,
                        ""Remarks"" character varying(2000) NULL,
                        ""CompletedAt"" timestamp with time zone NULL,
                        ""CreatedAt"" timestamp with time zone NOT NULL,
                        ""UpdatedAt"" timestamp with time zone NULL,
                        CONSTRAINT ""PK_PropertyAnalyses"" PRIMARY KEY (""Id"")
                    );

                    CREATE INDEX ""IX_PropertyAnalyses_AnalyserType"" ON ""PropertyAnalyses"" (""AnalyserType"");
                    CREATE INDEX ""IX_PropertyAnalyses_Status"" ON ""PropertyAnalyses"" (""Status"");
                ";

                context.Database.ExecuteSqlRaw(createTableSql);
                logger.LogInformation("PropertyAnalyses table created successfully.");

                // Clear and update migration history
                context.Database.ExecuteSqlRaw("DELETE FROM \"__EFMigrationsHistory\"");
                context.Database.ExecuteSqlRaw(
                    "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260108000000_InitialCreate', '8.0.0')");
                logger.LogInformation("Migration history updated.");
            }

            // Check and create PropertyDataRecords table if needed
            try
            {
                var propertyDataExists = context.PropertyDataRecords.Any();
                logger.LogInformation($"PropertyDataRecords table exists and has {context.PropertyDataRecords.Count()} records");
            }
            catch (Exception)
            {
                logger.LogWarning("PropertyDataRecords table does NOT exist. Creating table...");

                var createPropertyDataTableSql = @"
                    CREATE TABLE IF NOT EXISTS ""PropertyDataRecords"" (
                        ""Id"" uuid NOT NULL,
                        ""PropertyAddress"" character varying(500) NOT NULL,
                        ""PropertyUrl"" character varying(1000) NULL,
                        ""PropertyType"" character varying(50) NULL,
                        ""LandOwnership"" character varying(50) NULL,
                        ""HasClearTitle"" boolean NULL,
                        ""HasEncumbrances"" boolean NULL,
                        ""Zoning"" character varying(50) NULL,
                        ""LocationCategory"" character varying(50) NULL,
                        ""DistanceToCbdKm"" integer NULL,
                        ""SchoolZoneQuality"" character varying(50) NULL,
                        ""DistanceToPublicTransportMeters"" integer NULL,
                        ""RentalYieldPercentage"" numeric(18,2) NULL,
                        ""CapitalGrowthPercentage"" numeric(18,2) NULL,
                        ""VacancyRatePercentage"" numeric(18,2) NULL,
                        ""LocalDemand"" character varying(50) NULL,
                        ""HasStructuralIssues"" boolean NULL,
                        ""PropertyAgeYears"" integer NULL,
                        ""HasMajorDefects"" boolean NULL,
                        ""MaintenanceLevel"" character varying(50) NULL,
                        ""MeetsCurrentBuildingCodes"" boolean NULL,
                        ""HasRequiredCertificates"" boolean NULL,
                        ""HasLongTermTenants"" boolean NULL,
                        ""HasReliablePaymentHistory"" boolean NULL,
                        ""LeaseRemainingMonths"" integer NULL,
                        ""HasConsistentRentalHistory"" boolean NULL,
                        ""CashFlowCoverageRatio"" numeric(18,2) NULL,
                        ""MeetsServiceabilityRequirements"" boolean NULL,
                        ""LoanToValueRatio"" numeric(18,2) NULL,
                        ""AnnualInsuranceCost"" numeric(18,2) NULL,
                        ""SuitableForCrossCollateral"" boolean NULL,
                        ""EquityAvailable"" numeric(18,2) NULL,
                        ""EligibleForRefinance"" boolean NULL,
                        ""HasStableSaleHistory"" boolean NULL,
                        ""YearsSinceLastSale"" integer NULL,
                        ""DaysOnMarket"" integer NULL,
                        ""HasStrongComparables"" boolean NULL,
                        ""IsUniqueProperty"" boolean NULL,
                        ""AcceptedByMajorLenders"" boolean NULL,
                        ""RiskRating"" character varying(50) NULL,
                        ""HasDevelopmentRisk"" boolean NULL,
                        ""FitsPortfolioDiversity"" boolean NULL,
                        ""ViableForLongTermHold"" boolean NULL,
                        ""DataSource"" character varying(100) NULL,
                        ""CreatedAt"" timestamp with time zone NOT NULL,
                        ""UpdatedAt"" timestamp with time zone NULL,
                        CONSTRAINT ""PK_PropertyDataRecords"" PRIMARY KEY (""Id"")
                    );

                    CREATE INDEX IF NOT EXISTS ""IX_PropertyDataRecords_PropertyAddress"" ON ""PropertyDataRecords"" (""PropertyAddress"");
                ";

                context.Database.ExecuteSqlRaw(createPropertyDataTableSql);
                logger.LogInformation("PropertyDataRecords table created successfully.");
            }
        }
        else
        {
            logger.LogInformation("Applying migrations...");
            context.Database.Migrate();
        }

        logger.LogInformation("Database migration completed successfully.");

        // Verify tables exist now
        try
        {
            var count = context.PropertyAnalyses.Count();
            logger.LogInformation($"Verification: PropertyAnalyses table exists with {count} records");

            var propertyDataCount = context.PropertyDataRecords.Count();
            logger.LogInformation($"Verification: PropertyDataRecords table exists with {propertyDataCount} records");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Verification failed: Tables do not exist!");
            throw;
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw; // Re-throw to prevent app from starting with broken database
    }
}

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
