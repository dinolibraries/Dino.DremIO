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

## DbContext example

You can find a concrete test `DbContext` used in this repository at [Dino.DremIO.Tests/EfCore/DremioTestDbContext.cs](Dino.DremIO.Tests/EfCore/DremioTestDbContext.cs).

Below is a minimal example showing an EF `DbContext` and DI registration using the provider.

```csharp
// Example entity (read-only view) in tests uses attributes:
[Keyless]
[TableContext("youtube-channel-content")]
[Table("youtube-channel-revenue-combine")]
public class RevenueCombine { public Guid ProfileId { get; set; } public string Name { get; set; } /* ... */ }

// Minimal DbContext
public class MyDremioContext : DbContext
{
    public MyDremioContext(DbContextOptions<MyDremioContext> options) : base(options) { }

    public DbSet<RevenueCombine> RevenueCombines { get; set; }
}

// Registering the DbContext in Program.cs / Startup.cs
services.AddDbContext<MyDremioContext>(options =>
    options.UseDremio(new DremIOOption
    {
        EndpointUrl = "https://dremio.example.com:9047",
        UserName = "myuser",
        Password = "mypassword",
        TokenStore = "C:\\path\\to\\token.cache"
    }, dremioOpts =>
    {
        // Optional: set token store path using the fluent builder
        dremioOpts.UseTokenStore("C:\\path\\to\\token.cache");
    })
);

// Or, if you prefer the simple overload:
services.AddDbContext<MyDremioContext>(options =>
    options.UseDremio("https://dremio.example.com:9047", "myuser", "mypassword")
);
```

Notes:
- Entities mapped to Dremio views or datasets in this provider are typically marked with `[Keyless]` because Dremio is often read-only from EF's perspective.
- Use `[TableContext("<space-or-source>")]` and `[Table("<table-or-view>")]` (see tests) to point the provider at the correct dataset/context.
- For integration tests, check the `DremioTestDbContext` implementation in the tests folder for a working example.

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

