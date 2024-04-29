using CDRSandbox.Repositories.ClickHouse;
using CDRSandbox.Services.Models;
using ClickHouse.Client.Copy;

namespace CDRSandboxTests.Helpers;

public static class ClickHouseHelper
{
    public static async Task<long> Store(IEnumerable<CdrCsvItem> items, string connectionString)
    {
        using var bulkCopy = new ClickHouseBulkCopy(connectionString)
        {
            DestinationTableName = CdrRepositoryClickHouse.TableName,
            ColumnNames = CdrRepositoryClickHouse.ColumnsName,
            BatchSize = 5000
        };
        await bulkCopy.InitAsync(); // Prepares ClickHouseBulkCopy instance by loading target column types
        await bulkCopy.WriteToServerAsync(items.Select(item => item.ToObjects()));
        return bulkCopy.RowsWritten;
    }
}