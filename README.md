# CDRSandbox

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
- reference: `FixedString(17)` FixedString can also be used for a binary representation of hashes, this one seems to have 132 bits (not UUID 128 bits) and so, 132/8=16.5=17.
- currency: `FixedString(3)` it's always 3 chars.
- type: `Nullable(Enum8('domestic' = 1, 'international' = 2))` in the specification it states only two types but it can be null, at least there's values in the sample data.

### 3. FluentMigrator
This one was chosen thinking more in future work, to ease the maintainability of the database (automatic versioned migrations), and also because no one likes to run SQL scripts by hand.

### 4. Docker containers
To facilitate running with the external dependencies (Clickhouse).

