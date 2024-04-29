using Microsoft.AspNetCore.Mvc.Testing;

namespace CDRSandboxTests.Base;

// Code based on
// https://www.camiloterevinto.com/post/asp-net-core-integration-tests-with-nunit-and-moq
public class CustomWebApplicationFactory(Action<IServiceCollection> configureServices) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");
        base.ConfigureWebHost(builder);

        // let's configure them in the Fixture as needed
        builder.ConfigureServices(configureServices);
    }
}