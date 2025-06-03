Ideally we should use a MemoryCache for the user accounts that is loaded on startup and only accesses the database if an AccountId cannot be found in the cache.

WebApi.Client can be tested by mocking the HttpClientHandler. Also HttpClientFactory should be used.

Aspire can be used to synchronize the client and WebApi.

The meter read value is stored as a string. Should be an int, but had no time.

Used CsvReader, as I've used it before and it's not easy reading files manually.

Used EF Core scaffolding as it's quite simple. Could have also used something lightweight like Dapper.

I assumed that the MeterReadings table can only hold one row by any particular AccountId.