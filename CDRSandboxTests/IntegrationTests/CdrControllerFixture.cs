using System.Net;
using CDRSandbox.Controllers.Dtos;
using CDRSandbox.Repositories.ClickHouse;
using CDRSandbox.Services.Models;
using CDRSandboxTests.Base;
using CDRSandboxTests.Helpers;
using NUnit.Framework;
using RestSharp;
using Testcontainers.ClickHouse;

namespace CDRSandboxTests.IntegrationTests;

public class CdrControllerFixture : TestServerBase
{
    private const string TestDb = "TestDb";
    private const string TestUsername = "TestUsername";
    private const string TestPassword = "TestPassword";

    private readonly ClickHouseContainer _clickHouse = new ClickHouseBuilder()
        .WithDatabase(TestDb)
        .WithUsername(TestUsername)
        .WithPassword(TestPassword)
        .WithPortBinding(8123, true)
        // can't be the one used on DockerConfigs, Testcontainers don't like that we enable the TABiX
        .WithResourceMapping("Resources/ClickHouseConfig.xml", "/etc/clickhouse-server/config.d/")
        .WithImage("clickhouse/clickhouse-server:24.3.2.23")
        .Build();

    protected string ConnectionString => _clickHouse.GetConnectionString();
    protected RestClient _restClient;

    [OneTimeSetUp]
    protected async Task Setup()
    {
        await _clickHouse.StartAsync();
        // Init here so that GetClient() makes that TestServer initialize and FluentMigrator does the needed DB migrations
        _restClient = new RestClient(GetClient(), true);
    }

    [OneTimeTearDown]
    protected async Task Cleanup()
    {
        _restClient?.Dispose();
        await _clickHouse.DisposeAsync().AsTask();
    }

    protected override void ConfigServices(IServiceCollection services)
    {
        // Let's replace the DB Options to use the Testcontainer instance
        services.Configure<DbOptionsClickHouse>(options => { options.ConnectionString = ConnectionString; });
    }

    [Test]
    public async Task Record_ByReference_Found()
    {
        // Arrange
        var items = RandomCdrCsvItems(5, 5).ToArray();
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        var itemToFetch = items.First();

        // Act 
        var result = _restClient.Get(new RestRequest($"/cdr/{itemToFetch.Reference}"));

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        CdrItemDto? data = null;
        Assert.DoesNotThrow(() => data = Deserialize<CdrItemDto>(result.Content), "Received data must be deserializable to the dto" +
            "");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data, Is.EqualTo(itemToFetch).Using<CdrItemDto, CdrCsvItem>(Comparators.CdrItemDtoEqualsCdrCsvItem),
            "Received data must be the same than the one insert in DB");
    }
    
    [Test]
    public async Task Record_ByReference_NotFound()
    {
        // Arrange
        var items = RandomCdrCsvItems(5, 5).ToArray();
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        var itemToFetch = items.First();

        // Act 
        var result = _restClient.Get(new RestRequest($"/cdr/{RandomReference}"));

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Call must have NotFound status code");
    }
    
    [Test]
    public async Task Record_ByReferenceInvalid_BadRequest()
    {
        // Arrange
        var items = RandomCdrCsvItems(5, 5).ToArray();
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        var itemToFetch = items.First();

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(new RestRequest($"/cdr/INVALID")));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Call must have BadRequest status code");
    }
}