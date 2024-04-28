using System.Text;
using CDRSandbox.Repositories.ClickHouse.Entities;
using CDRSandbox.Repositories.ClickHouse.Projections;
using CDRSandbox.Repositories.Interfaces;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Dapper;
using Microsoft.Extensions.Options;

namespace CDRSandbox.Repositories.ClickHouse;

public class CdrRepositoryClickHouse(IOptions<DbOptionsClickHouse> options) : ICdrRepository, IDisposable
{
    public const string CallerIdColumn = "caller_id";
    public const string CallerIdUnpadColumn = CallerIdColumn + "_up";
    public const string RecipientColumn = "recipient";
    public const string RecipientUnpadColumn = RecipientColumn + "_up";
    public const string CallDateColumn = "call_date";
    public const string EndTimeColumn = "end_time";
    public const string DurationColumn = "duration";
    public const string CostColumn = "cost";
    public const string ReferenceColumn = "reference";
    public const string ReferenceColumnUnpadColumn = ReferenceColumn + "_up";
    public const string CurrencyColumn = "currency";
    public const string TypeColumn = "type";

    public const string TotalCostDefaultCurrencyMaterializedColumn = "total_cost_default_currency";

    // when we're calling an sql method on a column, we can't call it the same, because the WHERE cause would not use the indices
    private static readonly string SqlSelectAllColumns = $@"
        toStringCutToZero({CallerIdColumn}) AS {CallerIdUnpadColumn},
        toStringCutToZero({RecipientColumn}) AS {RecipientUnpadColumn},
        {CallDateColumn},
        {EndTimeColumn},
        {DurationColumn},
        {CostColumn},
        toStringCutToZero({ReferenceColumn}) AS {ReferenceColumnUnpadColumn},
        {CurrencyColumn},
        {TypeColumn}
    ";
    
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
            DestinationTableName = $"{TableName}",
            ColumnNames = ColumnsName,
            BatchSize = BatchSize
        };
        await bulkCopy.InitAsync(); // Prepares ClickHouseBulkCopy instance by loading target column types
        await bulkCopy.WriteToServerAsync(items);
        return bulkCopy.RowsWritten;
    }

    public async Task<ICdrItemEntity?> FetchRecordAsync(string reference)
    {
        var item = await _connection.Value.QueryFirstOrDefaultAsync<CdrItemClickHouseEntity?>(
            $@"
                SELECT
                    {SqlSelectAllColumns}
                FROM 
                    {TableName}
                WHERE {ReferenceColumn} = @reference",
            new { reference }
        );

        return item;
    }

    public async Task<IEnumerable<ICdrItemEntity>> FetchRecordsAsync(string callerId, DateTime from, DateTime to, int? type = null, long? nExpensiveCall = null)
    {
        var parameters = new DynamicParameters(new { callerId, from, to });
        var sb = new StringBuilder();

        if (nExpensiveCall != null)
            sb.Append($"SELECT TOP {nExpensiveCall} ");
        else
            sb.Append("SELECT ");

        sb
            .Append(SqlSelectAllColumns)
            .Append(" FROM ")
            .Append(TableName)
            .Append(" WHERE ")
            .Append($"{CallerIdColumn} = toFixedString(@callerId,32)")
            .Append($" AND {CallDateColumn} >= @from AND {CallDateColumn} < @to");
        
        if (type != null)
        {
            sb.Append($" AND {TypeColumn} = @type");
            parameters.Add("@type", type);
        }
        
        if (nExpensiveCall != null)
            sb.Append($"ORDER BY {TotalCostDefaultCurrencyMaterializedColumn} DESC");
        
        var items = await _connection.Value.QueryAsync<CdrItemClickHouseEntity>(
            sb.ToString(), parameters);
        
        return items;
    }

    public async Task<ICdrCountTotalDuration?> FetchCountTotalDurationCalls(DateTime from, DateTime to, int? type = null)
    {
        var parameters = new DynamicParameters(new { from, to });
        var sb = new StringBuilder();
   
        sb
            .Append("WITH ")
            .Append($"{CallDateColumn} >= @from AND {CallDateColumn} < @to");
            
        if (type != null)
        {
            sb.Append($" AND {TypeColumn} = @type");
            parameters.Add("@type", type);
        }
        
        sb
            .Append(" AS condition")
            .Append(" SELECT ")
            .Append($"countIf({DurationColumn},condition) AS count, sumIf({DurationColumn},condition) AS total_duration")
            .Append(" FROM ")
            .Append(TableName);

        var result = await _connection.Value.QueryFirstOrDefaultAsync<CdrCountTotalDuration>(sb.ToString(), parameters);

        return result;
    }
}