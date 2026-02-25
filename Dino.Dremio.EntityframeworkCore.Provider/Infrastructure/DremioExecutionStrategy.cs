using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

/// <summary>
/// Execution strategy for DremIO.
/// DremIO's REST API is stateless (no real transactions) so the strategy
/// does not retry on transient failures by default.  Subclass and override
/// <see cref="ShouldRetryOn"/> to add retry logic if needed.
/// </summary>
public sealed class DremioExecutionStrategy : ExecutionStrategy
{
    /// <summary>Initialises the strategy with no automatic retries.</summary>
    public DremioExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, maxRetryCount: 0, maxRetryDelay: TimeSpan.Zero) { }

    /// <summary>Initialises the strategy with configurable retries.</summary>
    public DremioExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay)
        : base(context, maxRetryCount, maxRetryDelay) { }

    /// <inheritdoc/>
    protected override bool ShouldRetryOn(Exception exception) =>
        // No known retriable DremIO exceptions at the REST layer;
        // extend this method if HTTP 429 / 503 transient errors should be retried.
        false;
}
