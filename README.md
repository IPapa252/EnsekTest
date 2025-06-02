Ideally we should use a MemoryCache for the user accounts that is loaded on startup and only accesses the database if an AccountId cannot be found in the cache.

WebApi.Client can be tested by mocking the HttpClientHandler. Also HttpClientFactory should be used.

Aspire can be used to synchronize the client and WebApi.