using CDRSandbox.Repositories.ClickHouse.Entities;
using CDRSandbox.Repositories.Interfaces;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Dapper;
using Microsoft.Extensions.Options;

namespace CDRSandbox.Repositories.ClickHouse;

public class CdrRepositoryClickHouseImpl(IOptions<DbOptionsClickHouse> options) : ICdrRepository, IDisposable
{
    private const string CallerIdColumn = "caller_id";
    private const string RecipientColumn = "recipient";
    private const string CallDateColumn = "call_date";
    private const string EndTimeColumn = "end_time";
    private const string DurationColumn = "duration";
    private const string CostColumn = "cost";
    private const string ReferenceColumn = "reference";
    private const string CurrencyColumn = "currency";
    private const string TypeColumn = "type";


    public const string TableName = "call_detail_record";
    public const int BatchSize = 10000; // TODO: this should be a config

    private static readonly string[] ColumnsName =
    [
        CallerIdColumn,
        RecipientColumn,
        CallDateColumn,
        EndTimeColumn,
        DurationColumn,
        CostColumn,
        ReferenceColumn,
        CurrencyColumn,
        TypeColumn
    ];

    private readonly Lazy<ClickHouseConnection> _connection =
        new(() => new ClickHouseConnection(options.Value.ConnectionString));

    public void Dispose()
    {
        if (_connection.IsValueCreated)
            _connection.Value.Dispose();
    }

    public async Task<long> StoreAsync(IEnumerable<ICdrItemEntity> items)
    {
        return await StoreAsync(items.Select(i => ((CdrItemClickHouseEntity)i).ToObjects()));
    }

    public async Task<long> StoreAsync(IEnumerable<object?[]> items)
    {
        using var bulkCopy = new ClickHouseBulkCopy(options.Value.ConnectionString)
        {
            DestinationTableName = $"{options.Value.Database}.{TableName}",
            ColumnNames = ColumnsName,
            BatchSize = BatchSize
        };
        await bulkCopy.InitAsync(); // Prepares ClickHouseBulkCopy instance by loading target column types
        await bulkCopy.WriteToServerAsync(items);
        return bulkCopy.RowsWritten;
    }

    public async Task<ICdrItemEntity?> FetchItemAsync(string reference)
    {
        var item = await _connection.Value.QueryFirstOrDefaultAsync<CdrItemClickHouseEntity?>(
            $"SELECT * FROM {options.Value.Database}.{TableName} WHERE {ReferenceColumn} = @reference",
            new { reference }
        );

        return item;
    }
}