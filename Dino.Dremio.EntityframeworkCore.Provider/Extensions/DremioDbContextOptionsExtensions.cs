using Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;
using Dino.DremIO.Options;
using Dino.DremIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dino.Dremio.EntityframeworkCore.Provider.Extensions;

/// <summary>
/// Extension methods that wire up the DremIO EF Core provider.
/// </summary>
public static class DremioDbContextOptionsExtensions
{
    // ── Core internal helper ─────────────────────────────────────────────────

    /// <summary>
    /// Registers both <see cref="DremioOptionsExtension"/> and
    /// <see cref="DremioRelationalOptionsExtension"/> (the latter is required so
    /// EF Core's relational APIs, e.g. <c>Database.GetDbConnection()</c>, are usable).
    /// </summary>
    private static void AddDremioExtensions(
        DbContextOptionsBuilder builder,
        DremioOptionsExtension dremioExt,
        string? connectionString = null)
    {
        var infra = (IDbContextOptionsBuilderInfrastructure)builder;
        infra.AddOrUpdateExtension(dremioExt);

        // Ensure a RelationalOptionsExtension is present so EF Core's relational
        // facade does not throw "No relational database providers are configured."
        var relExt = builder.Options.FindExtension<DremioRelationalOptionsExtension>()
                     ?? new DremioRelationalOptionsExtension();

        if (connectionString is not null)
            relExt = (DremioRelationalOptionsExtension)relExt.WithConnectionString(connectionString);

        infra.AddOrUpdateExtension(relExt);
    }
    /// <summary>
    /// Configures the <see cref="DbContext"/> to connect to a DremIO server.
    /// </summary>
    /// <param name="optionsBuilder">The options builder being configured.</param>
    /// <param name="dremioOption">
    ///     DremIO connection settings (endpoint URL, credentials, token store).
    /// </param>
    /// <param name="dremioOptionsAction">
    ///     An optional action that allows additional Dremio-specific configuration.
    /// </param>
    /// <returns>The builder so further calls can be chained.</returns>
    public static DbContextOptionsBuilder UseDremio(
        this DbContextOptionsBuilder optionsBuilder,
        DremIOOption dremioOption,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(dremioOption);

        var extension = GetOrCreateExtension(optionsBuilder)
            .WithDremioOption(dremioOption);

        AddDremioExtensions(optionsBuilder, extension,
            connectionString: $"Endpoint={dremioOption.EndpointUrl};User={dremioOption.UserName}");

        dremioOptionsAction?.Invoke(new DremioDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the <see cref="DbContext"/> to connect to a DremIO server
    /// using inline settings.
    /// </summary>
    public static DbContextOptionsBuilder UseDremio(
        this DbContextOptionsBuilder optionsBuilder,
        string endpointUrl,
        string username,
        string password,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
    {
        return optionsBuilder.UseDremio(
            new DremIOOption { EndpointUrl = endpointUrl, UserName = username, Password = password },
            dremioOptionsAction);
    }

    // ── Generic overloads ────────────────────────────────────────────────────

    /// <inheritdoc cref="UseDremio(DbContextOptionsBuilder, DremIOOption, Action{DremioDbContextOptionsBuilder}?)"/>
    public static DbContextOptionsBuilder<TContext> UseDremio<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DremIOOption dremioOption,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseDremio(
            (DbContextOptionsBuilder)optionsBuilder, dremioOption, dremioOptionsAction);

    /// <inheritdoc cref="UseDremio(DbContextOptionsBuilder, string, string, string, Action{DremioDbContextOptionsBuilder}?)"/>
    public static DbContextOptionsBuilder<TContext> UseDremio<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string endpointUrl,
        string username,
        string password,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseDremio(
            (DbContextOptionsBuilder)optionsBuilder, endpointUrl, username, password, dremioOptionsAction);

    // ── Service-based overloads (preferred when DremIOService is in the DI container) ──

    /// <summary>
    /// Configures the <see cref="DbContext"/> using a fully-constructed
    /// <see cref="DremIOService"/> (e.g. resolved from the application's DI container).
    /// </summary>
    public static DbContextOptionsBuilder UseDremio(
        this DbContextOptionsBuilder optionsBuilder,
        DremIOService dremioService,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(dremioService);

        var extension = GetOrCreateExtension(optionsBuilder)
            .WithDremioService(dremioService);

        AddDremioExtensions(optionsBuilder, extension);

        dremioOptionsAction?.Invoke(new DremioDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <summary>
    /// Configures the <see cref="DbContext"/> with both a <see cref="DremIOOption"/>
    /// and a pre-built <see cref="DremIOService"/>. Use this overload when both
    /// are already available from the application's DI container.
    /// </summary>
    public static DbContextOptionsBuilder UseDremio(
        this DbContextOptionsBuilder optionsBuilder,
        DremIOOption dremioOption,
        DremIOService dremioService,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(dremioOption);
        ArgumentNullException.ThrowIfNull(dremioService);

        var extension = GetOrCreateExtension(optionsBuilder)
            .WithDremioOption(dremioOption)
            .WithDremioService(dremioService);

        AddDremioExtensions(optionsBuilder, extension,
            connectionString: $"Endpoint={dremioOption.EndpointUrl};User={dremioOption.UserName}");

        dremioOptionsAction?.Invoke(new DremioDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    /// <inheritdoc cref="UseDremio(DbContextOptionsBuilder, DremIOService, Action{DremioDbContextOptionsBuilder}?)"/>
    public static DbContextOptionsBuilder<TContext> UseDremio<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DremIOService dremioService,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseDremio(
            (DbContextOptionsBuilder)optionsBuilder, dremioService, dremioOptionsAction);

    /// <inheritdoc cref="UseDremio(DbContextOptionsBuilder, DremIOOption, DremIOService, Action{DremioDbContextOptionsBuilder}?)"/>
    public static DbContextOptionsBuilder<TContext> UseDremio<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DremIOOption dremioOption,
        DremIOService dremioService,
        Action<DremioDbContextOptionsBuilder>? dremioOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseDremio(
            (DbContextOptionsBuilder)optionsBuilder, dremioOption, dremioService, dremioOptionsAction);

    // ── Internals ────────────────────────────────────────────────────────────

    private static DremioOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<DremioOptionsExtension>()
           ?? new DremioOptionsExtension();
}

/// <summary>
/// Fluent builder for DremIO-specific <see cref="DbContextOptions"/>.
/// Passed to the optional <c>dremioOptionsAction</c> callback of
/// <see cref="DremioDbContextOptionsExtensions.UseDremio"/>.
/// </summary>
public sealed class DremioDbContextOptionsBuilder
{
    private readonly DbContextOptionsBuilder _optionsBuilder;

    internal DremioDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        _optionsBuilder = optionsBuilder;
    }

    /// <summary>Sets the path where the DremIO auth token is cached on disk.</summary>
    public DremioDbContextOptionsBuilder UseTokenStore(string path)
    {
        var extension = _optionsBuilder.Options
            .FindExtension<DremioOptionsExtension>()!
            .WithDremioOption(new DremIOOption
            {
                EndpointUrl = _optionsBuilder.Options.FindExtension<DremioOptionsExtension>()!.DremioOption.EndpointUrl,
                UserName    = _optionsBuilder.Options.FindExtension<DremioOptionsExtension>()!.DremioOption.UserName,
                Password    = _optionsBuilder.Options.FindExtension<DremioOptionsExtension>()!.DremioOption.Password,
                TokenStore  = path
            });

        ((IDbContextOptionsBuilderInfrastructure)_optionsBuilder).AddOrUpdateExtension(extension);
        return this;
    }
}
