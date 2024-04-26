using CDRSandbox.Extensions;
using CDRSandbox.Repositories.Clickhouse;
using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
#region Logging

builder.Logging.AddMigrateDbLogging();

#endregion

// Service configuration
#region Services 

// Db Migration
builder.Services.AddMigrateDbConfigs();

// Db Abstractions initialization
builder.Services.AddOptions<DbOptionsClickhouse>().BindConfiguration("ConnectionStrings").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddScoped<ICdrRepository, CdrRepositoryClickhouseImpl>();

// Our Services
builder.Services.AddScoped<CdrService>(); // Scoped because ICdrRepository TODO: if ICdrRepository can be singleton change this

// Web API
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiConfigs();

#endregion

// Application configuration
#region Application

var app = builder.Build();

// Apply new DB migrations
app.MigrateDb();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseOpenApiAndReDoc();

app.UseAuthorization();

app.MapControllers();

app.Run();

#endregion
