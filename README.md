# CDRSandbox

<!-- TOC start (generated with https://github.com/derlin/bitdowntoc) -->

- [How to run](#how-to-run)
- [Assumptions](#assumptions)
- [Technology Decisions](#technology-decisions)
  * [AspNet Core Web API Project](#aspnet-core-web-api-project)
  * [Layered architecture](#layered-architecture)
  * [Repository pattern](#repository-pattern)
  * [ClickHouse for the database](#clickhouse-for-the-database)
  * [FluentMigrator](#fluentmigrator)
  * [Docker containers](#docker-containers)
  * [Receive files as a stream when uploaded](#receive-files-as-a-stream-when-uploaded)
  * [Using CsvHelper library to parse the CDR dataset file](#using-csvhelper-library-to-parse-the-cdr-dataset-file)
  * [ClickHouse.Client with Dapper libraries to handle ClickHouse data communication](#clickhouseclient-with-dapper-libraries-to-handle-clickhouse-data-communication)
  * [OpenApi for Endpoint documentation](#openapi-for-endpoint-documentation)
  * [NUnit for tests](#nunit-for-tests)
  * [RandomDataGenerator.Net for tests](#randomdatageneratornet-for-tests)
  * [Testcontainers for integration tests](#testcontainers-for-integration-tests)
  * [Microsoft.AspNetCore.Mvc.Testing for integration tests](#microsoftaspnetcoremvctesting-for-integration-tests)
  * [RestSharp for integration tests](#restsharp-for-integration-tests)
- [Future Work](#future-work)

<!-- TOC end -->

## How to run
First, make sure that you have:
* Docker installed
* .Net 8 SDK

Then to compile/start the application run `start.bat`, to stop and remove resources from docker run `stop.bat`.
Otherwise, you can always open the `.sln` file and compile/run it on your favorite IDE.

If wanted to run tests outside of IDE, run `run_tests.bat`.

When started, you can access http://localhost:8080/swagger for viewing/trying the API but also http://localhost:8080/redoc if you prefer.


## Assumptions
* CSV files always contain the header with the columns name, although it can omit the type.
* CSV files are in UTF-8.
* CSV separator is `,`.
* On the specs, it states that the currency is in ISO alpha-3 format, but this does not make any sense since this is for country codes. Most likely it was an overlook so, I'm using ISO 4217 for the currency.
* CSV `caller_id` is at most 32 chars and always present. Can be empty string.
* CSV `recipient` is at most 32 chars and always present. Can be empty string.
* CSV `call_date` is in format dd/MM/yyyy, d/MM/yyyy or dd/M/yyyy and always present.
* CSV `end_time` is in format HH:mm:ss and always present.
* CSV `duration` is an unsigned integer and always present.
* CSV `cost` is a positive floating point with at most 3 decimal places and is always present. Also, it's the cost per second of the call.
* CSV `reference` is a string with a hexadecimal representation of a binary and has at most 33 chars, unique in all data sets and always present.
* CSV `currency` is a ISO 4217 3 letter string, and one of ["AUD", "EUR", "CNY", "GBP", "JPY", "USD"].
* CSV `type` can be null, 1 or 2, because on sample data it does not exist and in specs mentions that it can be 1 or 2.
* The same CSV file is only uploaded once (future improvement: protect this).
* Time frame queries work like from <= [call_date] < to

## Technology Decisions

### AspNet Core Web API Project
This was an easy one, given the requirements (front-end is not required and must be an API).

### Layered architecture
For help decoupling things which improves future reusability, but also for maintainability.
Each layer only knows about its immediate upper layer.
Controllers -> Services -> Repositories

### Repository pattern
So in the future, if needed, we can use multiple repositories or just exchange to another.

### ClickHouse for the database
Since datasets can be huge (GB), certainly it would bring relational databases (RDBMS) like SQL Server, PostgreSQL, and the like to their knees since they are row oriented.
And so, I went with another approach more appropriate for this kind of data.

ClickHouse is a high-performance, column-oriented SQL database management system (DBMS) for online analytical processing (OLAP).
It states that it can handle trillions of rows in and query results in near real-time.
However, ClickHouse has some drawbacks, like it can't handle data updates easily and does not have uniqueness constraints on data.
Nevertheless, given that our datasets are immutable and uniqueness should be correct on datasets to be loaded, those scenarios will not affect us.
Also, usually is used together with another more traditional relational DB like SQL Server.

Table data types selected:
- caller_id: `FixedString(32)` since it's a phone number and although, in the sample dataset they have 12 chars I'm not sure that in others it won't go beyond that.
- recipient: `FixedString(32)` same as above.
- call_date: `Date` since it's just for the date (no time) and the range is [1970-01-01, 2149-06-06].
- end_time: `FixedString(8)` decided to store it as a string and will not use this field in period computation to simplify the logic and keep queries faster. Also, the requirements don't mention that the period must take this property into consideration.
- duration: `UInt32` decided to use UInt32 since UInt16 was only able to store nearly 18h of call duration in seconds and there could be a corner case where it was bigger than that.
- cost: `Float32` since it's a floating point value with three decimal places.
- reference: `FixedString(33)` FixedString can also be used for a binary representation of hashes, although after I tried there were some errors that would take too much time to try to solve, so instead I went with 33 hex chars.
- currency: `FixedString(3)` it's always three chars.
- type: `Nullable(Enum8('domestic' = 1, 'international' = 2))` in the specification it states only two types, but it can be null, at least there are no values in the sample data.

### FluentMigrator
This one was chosen thinking more about future work, to facilitate the maintainability of the database (automatic versioned migrations), and also because no one likes to run SQL scripts by hand.
Although it's not fully compatible with ClickHouse, I'm using SQLite to keep track of the migrations changes.

### Docker containers
To facilitate running with the external dependencies (ClickHouse).

### Receive files as a stream when uploaded
Microsoft states that files could be buffered or streamed and says that for larger files the stream should be used https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0.
It's not the fastest way, but it is the one that consumes fewer resources.

### Using CsvHelper library to parse the CDR dataset file
Why reinvent the wheel? This library looks like it is well maintained and performant and does what we need.

### ClickHouse.Client with Dapper libraries to handle ClickHouse data communication
ClickHouse.Client library is an ADO.NET client for ClickHouse and at first glance, seems to be the best one out there.
The bulk insertion seems to be very well implemented.
Dapper helps out when fetching query results and converting them to objects.

### OpenApi for Endpoint documentation
It integrates well within the code base, and is currently one of the standard ways to do it.

### NUnit for tests
This one was just because I'm familiar with it.

### RandomDataGenerator.Net for tests
Helps to generate various types of data easily.

### Testcontainers for integration tests
I'm a defender that integration tests should be the most similar to the real environment that they can get.
I know they take more time to run in comparison with using mocks, but I find that in the long run they are more reliable this way.

### Microsoft.AspNetCore.Mvc.Testing for integration tests
Load the main Asp.Net Core Server pipeline logic in memory, ideally for integration test purposes.
It even runs the original Program.cs with all its logic.

### RestSharp for integration tests
Helps make calling endpoints an easy task.

## Future Work
* Protect the endpoint from uploading the same CSV file again.
* A way to retrieve records by empty callerId, currently they are only retrieved by reference.
* Improve enum handling from DB to App.
* Improve the reference field to be bytes in the DB instead of Hex String.
* Adjust the callerId, and recipient fields if the size can be smaller.
* Save end_time as a number to save space.
* The currency dictionary should be built by connection to a side-kick traditional relational DB like SQL Server.
* Ability to import CSV file directly onto the Clickhouse from S3 or another URL since it has support for this.
* Fine-tuning Clickhouse logic (queries, PK, partition, engine, etc).
* Use another relational DB to save the migrations versioning or at least make SQLite permanent.
* Change the first script of creating the Clickhouse tables to remove the "OR REPLACE". It's there now just to facilitate the development.
* Create a clickhouse cluster to have resiliency.
* Code performance/load tests, also test with huge data sets.
* Add an observability/traceability/logging stack with alerting/monitoring to proactively solve the problems before they happen or at least faster.
* Change some hard-coded things in code to a DB config.
* The usual stuff, use a compilation pipeline that always runs the test suit, preferably at PR opening and not daily.
* Use something like Sonarqube on the compilation pipeline to enforce code quality and test coverage.
* Use a CVE code analyzer to give visibility to possible serious problems.
* Do a brainstorm to discuss other metrics that would make sense on the existent data and implement them.
* Add the authentication/authorization if this would be open to the "world".
* Add TLS support.
* Improve OpenAPI descriptions, add company iconography, and better examples.
* Tackle compilation warnings.
* Address code TODOs.