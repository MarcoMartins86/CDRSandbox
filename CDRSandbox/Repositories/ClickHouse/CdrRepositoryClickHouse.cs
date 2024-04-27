﻿using CDRSandbox.Repositories.ClickHouse.Entities;
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
            $@"
                SELECT
                    {SqlSelectAllColumns}
                FROM 
                    {options.Value.Database}.{TableName}
                WHERE {ReferenceColumn} = @reference",
            new { reference }
        );

        return item;
    }

    public async Task<IEnumerable<ICdrItemEntity>> FetchItemsFromCallerAsync(string callerId, DateTime from, DateTime to, int? type)
    {
        var sql = $@"
            SELECT
                {SqlSelectAllColumns} 
            FROM 
                {options.Value.Database}.{TableName} 
            WHERE 
                {CallerIdColumn} = toFixedString(@callerId,32)
        ";
        
        var parameters = new DynamicParameters(new { callerId, from, to });
        if (type != null)
        {
            sql = $"{sql} AND type = @type";
            parameters.Add("@type", type);
        }
        
        var items = await _connection.Value.QueryAsync<CdrItemClickHouseEntity>(
            sql, parameters);
        
        return items;
    }
}