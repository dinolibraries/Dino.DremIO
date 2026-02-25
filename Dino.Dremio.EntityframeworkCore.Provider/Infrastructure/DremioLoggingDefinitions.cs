using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

/// <summary>
/// Dremio-specific logging definitions. EF Core requires a concrete
/// <see cref="LoggingDefinitions"/> (or <see cref="RelationalLoggingDefinitions"/>)
/// to be registered in the provider's DI service container.
/// Extend this class to add custom Dremio event-ID definitions.
/// </summary>
public sealed class DremioLoggingDefinitions : RelationalLoggingDefinitions
{
    // No additional event definitions needed for the baseline provider.
    // Add LogEventDefinition fields here if custom Dremio-specific
    // diagnostics events are required (e.g. REST call tracing).
}
