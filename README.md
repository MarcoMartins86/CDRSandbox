# CDRSandbox

## Assumptions
* CSV files always contains the header with the columns name, although it can omit some of them.
* CSV files are in UTF-8.
* CSV separator is `,`.
* On the specs it states that the currency is in ISO alpha-3 format, but this does not make any sense since this is for country codes. Most likely it was an overlook so, I'm using ISO 4217 for the currency.
* CSV `caller_id` is at most 32 chars and always present. Can be empty string.
* CSV `recipient` is at most 32 chars and always present. Can be empty string.
* CSV `call_date` is in format dd/MM/yyyy, d/MM/yyyy or dd/M/yyyy and always present.
* CSV `end_time` is in format hh:mm:ss and always present.
* CSV `duration` is an unsigned integer and always present.
* CSV `cost` is a positive floating point and always present.
* CSV `reference` is a string with hexadecimal representation of a binary of at most 17 bytes, unique in all data sets and always present.
* CSV `currency` is a ISO 4217 3 letter string, and one of ["AUD", "EUR", "CNY", "GBP", "JPY", "USD"]. 
* CSV `type` can be null, 1 or 2, because on sample data it does not exist and in specs mention that can be 1 or 2.

* As for cu

## Technology Decisions

### 1. AspNet Core Web API Project
This was an easy one, given the requirements (front-end is not required and must be an API).

### 2. Clickhouse for database
Since datasets can be huge (GB), certainly it would bring relational databases (RDBMS) like SQL Server, PostgreSQL, and the like to their knees since they are row oriented. 
And so, I went with another approach more appropriate for this kind of data.

Clickhouse is a high-performance, column-oriented SQL database management system (DBMS) for online analytical processing (OLAP).
It states that it can handle trillions of rows in and query results in near real-time.
However, Clickhouse has at least one drawback, it can't handle data updates easily. 
Nevertheless, given that our datasets are immutable that scenario will not affect us.

Table data types selected:
- caller_id: `FixedString(32)` since it's a phone number and although, in the sample dataset they have 12 chars I'm not sure that in others it won't go beyond that.
- recipient: `FixedString(32)` same as above.
- call_date: `Date` since it's just for the date (no time) and the range is [1970-01-01, 2149-06-06].
- end_time: `FixedString(8)` decided to store it as a string and will not use this field in period computation to simplify the logic and keep queries faster. Also, the requirements don't mention that the period must consider this time.
- duration: `UInt32` decided to use UInt32 since UInt16 was only able to store near 18h of call duration in seconds and there could be a corner case where it was bigger than that.
- cost: `Float32` since it's a floating point value with 3 decimal places.
- reference: `FixedString(17)` FixedString can also be used for a binary representation of hashes, this one seems to have 132 bits (not UUID 128 bits) and so, 132/8=16.5=17. Also, make an assumption that datasets are correct and that they are all unique.
- currency: `FixedString(3)` it's always 3 chars.
- type: `Nullable(Enum8('domestic' = 1, 'international' = 2))` in the specification it states only two types but it can be null, at least there's values in the sample data.

### 3. FluentMigrator
This one was chosen thinking more in future work, to ease the maintainability of the database (automatic versioned migrations), and also because no one likes to run SQL scripts by hand.
Although, it's not fully compatible with Clickhouse, I'm using a SQLite to keep tracking of the migrations changes.

### 4. Docker containers
To facilitate running with the external dependencies (Clickhouse).

### 5. Receive files as a stream when uploaded
Microsoft states that files could be buffered or streamed and says that for larger files the stream should be used https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0.
It's not the fastest way, but it is the one that consumes less resources.

### 6. Using CsvHelper library to parse the CDR file
Why reinvent the wheel? This library looks like it is well maintained and performant and does what we need.

### 7. ClickHouse.Client library to handle Clickhouse data communication
This library is a ADO.NET client for ClickHouse and at first glance, seems to be the best one out there.
The bulk insertion seems to be very well implemented.


