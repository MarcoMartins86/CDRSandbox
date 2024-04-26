namespace CDRSandbox.Extensions;

public static class OpenApiExtension
{
    public static IServiceCollection AddOpenApiConfigs(this IServiceCollection services)
    {
        services.AddOpenApiDocument();
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