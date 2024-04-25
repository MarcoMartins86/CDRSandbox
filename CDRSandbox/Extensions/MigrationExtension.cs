using FluentMigrator.Runner;

namespace CDRSandbox.Extensions;

public static class MigrationExtension
{
    public static IApplicationBuilder MigrateDb(this IApplicationBuilder app, long version = long.MaxValue)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
        runner.ListMigrations();
        runner.MigrateUp(version);
        return app;
    }
}