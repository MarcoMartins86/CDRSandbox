using System.Reflection;
using FluentMigrator.Runner;

namespace CDRSandbox.Extensions;

public static class MigrationExtension
{
    public static ILoggingBuilder AddMigrateDbLogging(this ILoggingBuilder logging)
    {
        logging.AddFluentMigratorConsole();
        
        return logging;
    }
    
    public static IServiceCollection AddMigrateDbConfigs(this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(config => config
                .AddSQLite(true, true)
                .WithGlobalConnectionString("Data Source=:memory:")
                .WithMigrationsIn(Assembly.GetExecutingAssembly()));
        return services;
    }
    
    public static IApplicationBuilder MigrateDb(this IApplicationBuilder app, long version = long.MaxValue)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
        runner.ListMigrations();
        runner.MigrateUp(version);
        return app;
    }
}