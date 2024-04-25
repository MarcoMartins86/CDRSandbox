using System.Reflection;
using CDRSandbox.Extensions;
using CDRSandbox.Repositories;
using CDRSandbox.Repositories.Clickhouse;
using CDRSandbox.Repositories.Interfaces;
using FluentMigrator.Runner;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
#region Logging

builder.Logging.AddFluentMigratorConsole();

#endregion

// Service configuration
#region Services 

// Db Migration
builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(config => config
        .AddSQLite(true, true)
        .WithGlobalConnectionString("Data Source=:memory:")
        .WithMigrationsIn(Assembly.GetExecutingAssembly()));

// Db Abstractions initialization
builder.Services.AddOptions<DbOptionsClickhouse>().BindConfiguration("ConnectionStrings").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddScoped<ICdrRepository, CdrRepositoryClickhouseImpl>();

// Web API
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

#endregion
