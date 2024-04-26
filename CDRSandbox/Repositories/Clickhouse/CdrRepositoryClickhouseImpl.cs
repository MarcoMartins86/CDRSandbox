using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models;
using ClickHouse.Client.Copy;
using Microsoft.Extensions.Options;

namespace CDRSandbox.Repositories.Clickhouse;

public class CdrRepositoryClickhouseImpl(IOptions<DbOptionsClickhouse> options) : ICdrRepository, IDisposable
{
    public const string TableName = "call_detail_record";
    public const int BatchSize = 10000; // TODO: this should be a config
    private static readonly string[] ColumnsName =
        ["caller_id", "recipient", "call_date", "end_time", "duration", "cost", "reference", "currency", "type"];

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async Task<long> Store(IEnumerable<CdrItem> items)
    {
        using var bulkCopy = new ClickHouseBulkCopy(options.Value.ConnectionString)
        {
            DestinationTableName = $"{options.Value.Database}.{TableName}",
            ColumnNames = ColumnsName,
            BatchSize = BatchSize
        };
        await bulkCopy.InitAsync(); // Prepares ClickHouseBulkCopy instance by loading target column types
        await bulkCopy.WriteToServerAsync(items.Select(i => i.ToObjects()));
        return bulkCopy.RowsWritten;
    }
}