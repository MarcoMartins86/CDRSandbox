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

    private string ConnectionString => _clickHouse.GetConnectionString();
    private RestClient _restClient;

    private static readonly Dictionary<string, double> CurrencyRates = new()
    {
        ["AUD"] = 0.52279068d,
        ["EUR"] = 0.85589298d,
        ["CNY"] = 0.11046396d,
        ["GBP"] = 1d,
        ["JPY"] = 0.0050577936d,
        ["USD"] = 0.80045178d
    };

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

    #region Record_ByReference
    
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
        Assert.DoesNotThrow(() => data = Deserialize<CdrItemDto>(result.Content), 
            "Received data must be deserializable to the dto");
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

        // Act 
        var result = _restClient.Get(new RestRequest($"/cdr/{RandomReference}"));

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), 
            "Call must have NotFound status code");
    }
    
    [Test]
    public void Record_ByReferenceInvalid_BadRequest()
    {
        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(new RestRequest($"/cdr/INVALID")));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), 
            "Call must have BadRequest status code");
    }
    
    #endregion

    #region Records_ByCallerId
    
    [Test]
    public async Task Records_ByCallerId_Found()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var callerId = RandomPhoneNumber;
        var items = new List<CdrCsvItem>();
        var itemToFetch = new List<CdrCsvItem>();
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callerId);
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to)
                itemToFetch.Add(newItem);
        }
        // It's not likely but if there aren't items within that period let's create at least three
        if (itemToFetch.Count == 0)
        {
            var one = RandomCdrCsvItem(callerId, callDate:from.AddDays(1));
            var two = RandomCdrCsvItem(callerId, callDate:from.AddDays(2));
            var three = RandomCdrCsvItem(callerId, callDate:from.AddDays(3));
            items.Add(one);
            items.Add(two);
            items.Add(three);
            itemToFetch.Add(one);
            itemToFetch.Add(two);
            itemToFetch.Add(three);
        }
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", callerId);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        var data = new List<CdrItemDto>();
        Assert.DoesNotThrow(() => data = Deserialize<List<CdrItemDto>>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data.Count, Is.EqualTo(itemToFetch.Count), "Number of items must be the same");
        Assert.That(itemToFetch.Count, Is.LessThan(items.Count - 1000), "There must be more records id DB of the caller id");
        Assert.That(data, Is.EquivalentTo(itemToFetch).Using<CdrItemDto, CdrCsvItem>(Comparators.CdrItemDtoEqualsCdrCsvItem),
            "Received data must be the same than the one insert in DB");
    }
    
    [Test]
    public async Task Records_ByCallerIdAndType_Found()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var callerId = RandomPhoneNumber;
        var type = CdrCallTypeEnum.International;
        var items = new List<CdrCsvItem>();
        var itemToFetch = new List<CdrCsvItem>();
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callerId);
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to && type == newItem.Type)
                itemToFetch.Add(newItem);
        }
        // It's not likely but if there aren't items within that period and type let's create at least three
        if (itemToFetch.Count == 0)
        {
            var one = RandomCdrCsvItem(callerId, callDate:from.AddDays(1), type:type);
            var two = RandomCdrCsvItem(callerId, callDate:from.AddDays(2), type:type);
            var three = RandomCdrCsvItem(callerId, callDate:from.AddDays(3), type:type);
            items.Add(one);
            items.Add(two);
            items.Add(three);
            itemToFetch.Add(one);
            itemToFetch.Add(two);
            itemToFetch.Add(three);
        }
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", callerId);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("type", type);

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        var data = new List<CdrItemDto>();
        Assert.DoesNotThrow(() => data = Deserialize<List<CdrItemDto>>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data.Count, Is.EqualTo(itemToFetch.Count), "Number of items must be the same");
        Assert.That(itemToFetch.Count, Is.LessThan(items.Count - 1000), "There must be more records id DB of the caller id");
        Assert.That(data, Is.EquivalentTo(itemToFetch).Using<CdrItemDto, CdrCsvItem>(Comparators.CdrItemDtoEqualsCdrCsvItem),
            "Received data must be the same than the one insert in DB");
    }
    
    [Test]
    public async Task Records_ByCallerId_NotFound()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var callerId = RandomPhoneNumber;
        var items = new List<CdrCsvItem>();
        var itemToFetch = new List<CdrCsvItem>();
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callerId);
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to)
                itemToFetch.Add(newItem);
        }
        Assert.That(itemToFetch.Count, Is.Zero, "Must not be able to fetch any items");
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", callerId);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        var data = new List<CdrItemDto>();
        Assert.DoesNotThrow(() => data = Deserialize<List<CdrItemDto>>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data.Count, Is.Zero, "Number of items must be the same");
    }
    
    [Test]
    public void Records_ByCallerIdInvalid_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", "INVALID");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void Records_ByCallerIdFromBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", DateTime.UtcNow.Date);
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void Records_ByCallerIdToBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", DateTime.UtcNow.Date);

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void Records_ByCallerIdTypeBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("type", 3);

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void Records_ByCallerIdPeriodBigger1Month_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from + TimeSpan.FromDays(60)).Date;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void Records_ByCallerIdPeriodNegative_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from - TimeSpan.FromDays(60)).Date;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void Records_ByCallerIdPeriod0Days_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = from;
        var request = new RestRequest($"/cdr/Records");
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }

    #endregion
    
    #region Records_ByCallerId
    
    [Test]
    public async Task ExpensiveCallsRecords_ByCallerId_Found()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var callerId = RandomPhoneNumber;
        var items = new List<CdrCsvItem>();
        var itemToFetch = new List<CdrCsvItem>();
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callerId);
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to)
                itemToFetch.Add(newItem);
        }
        // It's not likely but if there aren't items within that period let's create at least three
        if (itemToFetch.Count == 0)
        {
            var one = RandomCdrCsvItem(callerId, callDate:from.AddDays(1));
            var two = RandomCdrCsvItem(callerId, callDate:from.AddDays(2));
            var three = RandomCdrCsvItem(callerId, callDate:from.AddDays(3));
            items.Add(one);
            items.Add(two);
            items.Add(three);
            itemToFetch.Add(one);
            itemToFetch.Add(two);
            itemToFetch.Add(three);
        }

        var n = 20;
        itemToFetch = itemToFetch.OrderByDescending(i => i.Cost * i.Duration * CurrencyRates[i.Currency!]).Take(n).ToList();
        
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", n);
        request.AddQueryParameter("callerId", callerId);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        var data = new List<CdrItemDto>();
        Assert.DoesNotThrow(() => data = Deserialize<List<CdrItemDto>>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data.Count, Is.EqualTo(itemToFetch.Count), "Number of items must be the same");
        Assert.That(itemToFetch.Count, Is.LessThan(items.Count - 1000), "There must be more records id DB of the caller id");
        Assert.That(data, Is.EquivalentTo(itemToFetch).Using<CdrItemDto, CdrCsvItem>(Comparators.CdrItemDtoEqualsCdrCsvItem),
            "Received data must be the same than the one insert in DB");
    }
    
    [Test]
    public async Task ExpensiveCallsRecords_ByCallerIdAndType_Found()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var callerId = RandomPhoneNumber;
        var type = CdrCallTypeEnum.International;
        var items = new List<CdrCsvItem>();
        var itemToFetch = new List<CdrCsvItem>();
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callerId);
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to && type == newItem.Type)
                itemToFetch.Add(newItem);
        }
        // It's not likely but if there aren't items within that period and type let's create at least three
        if (itemToFetch.Count == 0)
        {
            var one = RandomCdrCsvItem(callerId, callDate:from.AddDays(1), type:type);
            var two = RandomCdrCsvItem(callerId, callDate:from.AddDays(2), type:type);
            var three = RandomCdrCsvItem(callerId, callDate:from.AddDays(3), type:type);
            items.Add(one);
            items.Add(two);
            items.Add(three);
            itemToFetch.Add(one);
            itemToFetch.Add(two);
            itemToFetch.Add(three);
        }
        
        var n = 20;
        itemToFetch = itemToFetch.OrderByDescending(i => i.Cost * i.Duration * CurrencyRates[i.Currency!]).Take(n).ToList();
        
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", n);
        request.AddQueryParameter("callerId", callerId);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("type", type);

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        var data = new List<CdrItemDto>();
        Assert.DoesNotThrow(() => data = Deserialize<List<CdrItemDto>>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data.Count, Is.EqualTo(itemToFetch.Count), "Number of items must be the same");
        Assert.That(itemToFetch.Count, Is.LessThan(items.Count - 1000), "There must be more records id DB of the caller id");
        Assert.That(data, Is.EquivalentTo(itemToFetch).Using<CdrItemDto, CdrCsvItem>(Comparators.CdrItemDtoEqualsCdrCsvItem),
            "Received data must be the same than the one insert in DB");
    }
    
    [Test]
    public async Task ExpensiveCallsRecords_ByCallerId_NotFound()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var callerId = RandomPhoneNumber;
        var items = new List<CdrCsvItem>();
        var itemToFetch = new List<CdrCsvItem>();
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callerId);
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to)
                itemToFetch.Add(newItem);
        }
        
        var n = 20;
        itemToFetch = itemToFetch.OrderByDescending(i => i.Cost * i.Duration * CurrencyRates[i.Currency!]).Take(n).ToList();
        
        Assert.That(itemToFetch.Count, Is.Zero, "Must not be able to fetch any items");
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", n);
        request.AddQueryParameter("callerId", callerId);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        var data = new List<CdrItemDto>();
        Assert.DoesNotThrow(() => data = Deserialize<List<CdrItemDto>>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data.Count, Is.Zero, "Number of items must be the same");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdInvalid_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", "INVALID");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdFromBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", DateTime.UtcNow.Date);
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdToBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", DateTime.UtcNow.Date);

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdTypeBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("type", 3);

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdPeriodBigger1Month_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from + TimeSpan.FromDays(60)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdPeriodNegative_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from - TimeSpan.FromDays(60)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdPeriod0Days_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = from;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 5);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void ExpensiveCallsRecords_ByCallerIdBadN_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/ExpensiveCallsRecords");
        request.AddQueryParameter("n", 0);
        request.AddQueryParameter("callerId", RandomPhoneNumber);
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }

    #endregion

    #region CountTotalDurationCalls_ByPeriod
    
    [Test]
    public async Task CountTotalDurationCalls_ByPeriod_Found()
    {
        // Arrange
        // let's use a period in the future since RandomDateTime are all in the past, and since we don't filter by id
        // other DB entries would be count
        var from = (DateTime.UtcNow + TimeSpan.FromDays(10)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var items = new List<CdrCsvItem>();
        var count = 0;
        var duration = 0L;
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callDate:from.AddDays(Random.Next(0, 60)));
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to)
            {
                count++;
                duration += newItem.Duration!.Value;
            }
        }
        // It's not likely but if there aren't items within that period let's create at least three
        if (count == 0)
        {
            var one = RandomCdrCsvItem(callDate:from.AddDays(1));
            var two = RandomCdrCsvItem(callDate:from.AddDays(2));
            var three = RandomCdrCsvItem(callDate:from.AddDays(3));
            items.Add(one);
            items.Add(two);
            items.Add(three);
            count += 3;
            duration += one.Duration!.Value;
            duration += two.Duration!.Value;
            duration += three.Duration!.Value;
        }
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        CountTotalDurationDto? data = null;
        Assert.DoesNotThrow(() => data = Deserialize<CountTotalDurationDto>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data!.Count, Is.EqualTo(count), "Number of items must be the same");
        Assert.That(count, Is.LessThan(items.Count), "There must be more records id DB");
        Assert.That(data.TotalDuration, Is.EqualTo(duration),
            "Received duration must be the same");
    }
    
    [Test]
    public async Task CountTotalDurationCalls_ByPeriodAndType_Found()
    {
        // Arrange
        // let's use a period in the future since RandomDateTime are all in the past, and since we don't filter by id
        // other DB entries would be count
        // test above already uses 10 + 60 in the future
        var from = (DateTime.UtcNow + TimeSpan.FromDays(100)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var type = CdrCallTypeEnum.International;
        var items = new List<CdrCsvItem>();
        var count = 0;
        var duration = 0L;
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem(callDate:from.AddDays(Random.Next(0, 60)));
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to && type == newItem.Type)
            {
                count++;
                duration += newItem.Duration!.Value;
            }
        }
        // It's not likely but if there aren't items within that period and type let's create at least three
        if (count == 0)
        {
            var one = RandomCdrCsvItem(callDate:from.AddDays(1), type:type);
            var two = RandomCdrCsvItem(callDate:from.AddDays(2), type:type);
            var three = RandomCdrCsvItem(callDate:from.AddDays(3), type:type);
            items.Add(one);
            items.Add(two);
            items.Add(three);
            count += 3;
            duration += one.Duration!.Value;
            duration += two.Duration!.Value;
            duration += three.Duration!.Value;
        }

        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("type", type);

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        CountTotalDurationDto? data = null;
        Assert.DoesNotThrow(() => data = Deserialize<CountTotalDurationDto>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data!.Count, Is.EqualTo(count), "Number of items must be the same");
        Assert.That(count, Is.LessThan(items.Count), "There must be more records id DB");
        Assert.That(data.TotalDuration, Is.EqualTo(duration),
            "Received duration must be the same");
    }
    
    [Test]
    public async Task CountTotalDurationCalls_ByPeriod_NotFound()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from + TimeSpan.FromDays(30)).Date;
        var items = new List<CdrCsvItem>();
        var count = 0;
        var duration = 0L;
        for (int i = 0; i < 1000; i++)
        {
            var newItem = RandomCdrCsvItem();
            items.Add(newItem);
            var callDateDt = newItem.CallDate!.Value.ToDateTime(TimeOnly.MinValue).Date;
            if (from <= callDateDt && callDateDt < to)
            {
                count++;
                duration += newItem.Duration!.Value;
            }
        }
        Assert.That(count, Is.Zero, "Must not be able to fetch any items");
        //Also let's add other callers
        items.AddRange(RandomCdrCsvItems(1000, 10));
        // Add them to DB
        var inserts = await ClickHouseHelper.Store(items, ConnectionString);
        Assert.That(inserts, Is.GreaterThan(0)); // check that entries are on the DB
        // Build the request
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        var result = _restClient.Get(request);

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Call must have OK status code");
        Assert.That(result.Content, Is.Not.Null.And.Not.Empty, "Content can't be null nor empty");
        CountTotalDurationDto? data = null;
        Assert.DoesNotThrow(() => data = Deserialize<CountTotalDurationDto>(result.Content), 
            "Received data must be deserializable to a list of dtos");
        Assert.That(data, Is.Not.Null, "Deserialized data can't be null");
        Assert.That(data!.Count, Is.EqualTo(count), "Number of items must be the same");
        Assert.That(count, Is.LessThan(items.Count), "There must be more records id DB");
        Assert.That(data.TotalDuration, Is.EqualTo(duration),
            "Received duration must be the same");
    }
    
    
    [Test]
    public void CountTotalDurationCalls_ByPeriodFromBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", DateTime.UtcNow.Date);
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void CountTotalDurationCalls_ByPeriodToBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", DateTime.UtcNow.Date);

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void CountTotalDurationCalls_ByPeriodTypeBadFormat_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(50)).Date;
        var to = (from + TimeSpan.FromDays(20)).Date;
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("type", 3);

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void CountTotalDurationCalls_ByPeriodPeriodBigger1Month_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from + TimeSpan.FromDays(60)).Date;
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void CountTotalDurationCalls_ByPeriodPeriodNegative_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = (from - TimeSpan.FromDays(60)).Date;
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    [Test]
    public void CountTotalDurationCalls_ByPeriodPeriod0Days_BadRequest()
    {
        // Arrange
        var from = (DateTime.UtcNow - TimeSpan.FromDays(365)).Date;
        var to = from;
        var request = new RestRequest($"/cdr/CountTotalDurationCalls");
        request.AddQueryParameter("from", from.ToString(CdrItem.CallDateFormat));
        request.AddQueryParameter("to", to.ToString(CdrItem.CallDateFormat));

        // Act 
        // Assert
        var ex = Assert.Throws<HttpRequestException>(() => _restClient.Get(request));
        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Call must have BadRequest status code");
    }
    
    #endregion
}