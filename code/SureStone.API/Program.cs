using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Insurance.Infrastructure.Mappings;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

// Add services to the container.
services.AddControllers();

// Dependency injection
services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
services.AddScoped<ISharepointService, SharepointService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Set the app settings on different environment
var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{enviroment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("Default");
services.AddHangfire(config =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseStorage(
            new MySqlStorage(
                connectionString,
                new MySqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(10),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 25000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "hangfire_",
                }))
        .UseJobsLogger()
        .UseColouredConsoleLogProvider();
});

services.AddDbContext<SureStoneDbContext>(async optionsBuilder =>
{
#pragma warning disable CS8604 // Possible null reference argument.
    optionsBuilder.UseMySQL(connectionString);
#pragma warning restore CS8604 // Possible null reference argument.

    // Make sure that the database is created
    var dbContextOptions = (DbContextOptions<SureStoneDbContext>)optionsBuilder.Options;
    using var dbContext = new SureStoneDbContext(dbContextOptions);
    await dbContext.Database.EnsureCreatedAsync();
});

services.AddHangfireServer(x =>
{
    x.ServerName = Environment.MachineName;
});

// Adding auto-mapper
services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire");

app.Run();
