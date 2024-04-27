namespace CDRSandbox.Extensions;

public static class OpenApiExtension
{
    public static IServiceCollection AddOpenApiConfigs(this IServiceCollection services)
    {
        services.AddOpenApiDocument(settings =>
        {
            settings.Title = "Call detail record API";
        });
        return services;
    }
    
    public static IApplicationBuilder UseOpenApiAndReDoc(this IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.UseReDoc(settings =>
        {
            settings.Path = "/redoc";
        });
        return app;
    }
}