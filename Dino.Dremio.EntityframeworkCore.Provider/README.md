# Dino.Dremio.EntityframeworkCore.Provider

## Purpose

This EF Core provider enables querying Dremio using the repository's client, SQL generator and storage layers. It implements the necessary extensions, SQL generation, and connection/reader classes to make EF Core work with Dremio.

## Main folders

- `Extensions/` – contains the `UseDremio(...)` extension used to configure a `DbContext`.
- `Query/` – SQL generator, method translators, and expression-to-SQL factories.
- `Storage/` – result readers, connection, command and parameter implementations.
- `Infrastructure/` – EF Core internals such as option extensions and convention set builders.

## Usage (example)

Registering a `DbContext` in `Program` or `Startup`:

```csharp
// Using DremIOOption
services.AddDbContext<MyContext>(options =>
    options.UseDremio(new DremIOOption
    {
        EndpointUrl = "https://dremio.example.com:9047",
        UserName = "myuser",
        Password = "mypassword",
        TokenStore = null // or a path to store auth token
    }, dremioOpts =>
    {
        dremioOpts.UseTokenStore("C:\\path\\to\\token.cache");
    })
);

// Or use the overload that takes endpoint, username and password
services.AddDbContext<MyContext>(options =>
    options.UseDremio("https://dremio.example.com:9047", "myuser", "mypassword")
);

// If a DremIOService is registered in DI, you can pass it directly:
// services.AddSingleton<DremIOService>(...);
// options.UseDremio(dremioService);
```

The `DremioDbContextOptionsBuilder.UseTokenStore(string path)` method lets you specify where the authentication token is cached.

## Important classes

- `DremioDbContextOptionsExtensions` (Extensions) — entry points for `UseDremio` and the fluent builder.
- `DremioOptionsExtension`, `DremioRelationalOptionsExtension` (Infrastructure) — EF Core options extensions.
- `DremioQuerySqlGenerator`, `DremioSqlExpressionFactory` (Query) — SQL generation tailored for Dremio.
- `DremioDbConnection`, `DremioDbCommand`, `DremioDataReader` (Storage) — connection and result-reading implementations.

## Tests

See the integration tests under the `Dino.DremIO.Tests` project and the `EfCore` test folder for examples showing how to run provider integration tests.

## Notes

- `DremIOOption` is defined in the main project under `Dino.DremIO/Options`.
- For real-world testing against a running Dremio instance, provide a valid endpoint and credentials.

