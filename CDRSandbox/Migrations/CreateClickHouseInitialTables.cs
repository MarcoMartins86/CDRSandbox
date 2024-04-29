using CDRSandbox.Repositories.ClickHouse;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using FluentMigrator;
using Microsoft.Extensions.Options;

namespace CDRSandbox.Migrations;

[Migration(202404251107, "Create ClickHouse initial tables (CDR, CurrencyRates")]
public class CreateClickHouseInitialTables(IOptions<DbOptionsClickHouse> options) : Migration
{
    public override void Down()
    {
        throw new NotImplementedException();
    }

    public override async void Up()
    {
        await using var connection = new ClickHouseConnection(options.Value.ConnectionString);

        // Seems that we need to give this role to be able to use dictionaries with a user
        // https://clickhouse.com/docs/en/sql-reference/statements/create/dictionary
        
        // Create a table that will hold the currency conversion rate values
        // code needs to be UInt64 so it can be a dictionary key
        await connection.ExecuteStatementAsync(
            """
            CREATE OR REPLACE TABLE currency_rate
            (
                code UInt64,
                rate Float64
            )
            ENGINE = MergeTree
            PRIMARY KEY (code)
            """
        );
        
        // Add some currency conversion rates
        await connection.ExecuteStatementAsync(
            """
            INSERT INTO currency_rate (code, rate) VALUES
             (reinterpretAsUInt64('AUD'), toFloat64(0.52279068)),
             (reinterpretAsUInt64('EUR'), toFloat64(0.85589298)),
             (reinterpretAsUInt64('CNY'), toFloat64(0.11046396)),
             (reinterpretAsUInt64('GBP'), toFloat64(1)),
             (reinterpretAsUInt64('JPY'), toFloat64(0.0050577936)),
             (reinterpretAsUInt64('USD'), toFloat64(0.80045178));
            """
        );
        
        // Create a dictionary to help us pre-computed some metrics
        await connection.ExecuteStatementAsync(
            $"""
            CREATE OR REPLACE DICTIONARY currency_rate_dictionary
            (
                code UInt64,
                rate Float64
            )
            PRIMARY KEY code
            SOURCE(CLICKHOUSE(TABLE 'currency_rate' USER '{options.Value.Username}' PASSWORD '{options.Value.Password}' DB '{options.Value.Database}'))
            LAYOUT(HASHED())
            LIFETIME(MIN 60 MAX 120)
            """
        );
        
        // create the table that will hold the datasets and pre-computed metrics
        await connection.ExecuteStatementAsync(
            $"""
             CREATE OR REPLACE TABLE {CdrRepositoryClickHouse.TableName}
             (
                 caller_id FixedString(32),
                 recipient FixedString(32),
                 call_date Date,
                 end_time FixedString(8),
                 duration UInt32,
                 cost Float32,
                 reference FixedString(33),
                 currency FixedString(3),
                 type Nullable(Enum8('domestic' = 1, 'international' = 2)),
                 total_cost_default_currency Float64 MATERIALIZED cost * duration * dictGet('currency_rate_dictionary', 'rate', reinterpretAsUInt64(currency))
             )
             ENGINE = MergeTree
             PARTITION BY toYYYYMM(call_date)
             PRIMARY KEY (type, caller_id, recipient)
             ORDER BY (type, caller_id, recipient, total_cost_default_currency)
             """
        );
    }
}