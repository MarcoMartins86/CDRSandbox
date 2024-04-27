using CDRSandbox.Repositories.ClickHouse;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using FluentMigrator;
using Microsoft.Extensions.Options;

namespace CDRSandbox.Migrations;

[Migration(202404251107)]
public class CreateCdrClickHouseTable(IOptions<DbOptionsClickHouse> options) : Migration
{
    public override void Down()
    {
        throw new NotImplementedException();
    }

    public override async void Up()
    {
        await using var connection = new ClickHouseConnection(options.Value.ConnectionString);

        await connection.ExecuteStatementAsync(
            $"""
             CREATE OR REPLACE TABLE {options.Value.Database}.{CdrRepositoryClickHouseImpl.TableName}
             (
                 caller_id FixedString(32),
                 recipient FixedString(32),
                 call_date Date,
                 end_time FixedString(8),
                 duration UInt32,
                 cost Float32,
                 reference FixedString(33),
                 currency FixedString(3),
                 type Nullable(Enum8('domestic' = 1, 'international' = 2))
             )
             ENGINE = MergeTree
             PARTITION BY toYYYYMM(call_date)
             PRIMARY KEY (type, caller_id, recipient)
             ORDER BY (type, caller_id, recipient, call_date)
             """
        );
    }
}