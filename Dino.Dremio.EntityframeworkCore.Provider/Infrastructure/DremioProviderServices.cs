using Dino.DremIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

// ── DremioExecutionStrategyFactory ───────────────────────────────────────────

/// <summary>
/// Creates <see cref="DremioExecutionStrategy"/> instances for the EF Core pipeline.
/// </summary>
public sealed class DremioExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly ExecutionStrategyDependencies _dependencies;

    public DremioExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public IExecutionStrategy Create() => new DremioExecutionStrategy(_dependencies);
}

// ── DremioRelationalDatabaseCreator ──────────────────────────────────────────

/// <summary>
/// Minimal database creator for DremIO.
/// DremIO is a query engine — DDL operations are not supported via the REST API.
/// Extends <see cref="RelationalDatabaseCreator"/> so EF Core's abstract members
/// are satisfied; only the two abstract members need overriding.
/// </summary>
public sealed class DremioRelationalDatabaseCreator : RelationalDatabaseCreator
{
    public DremioRelationalDatabaseCreator(RelationalDatabaseCreatorDependencies dependencies)
        : base(dependencies) { }

    /// <summary>Returns <c>true</c> — DremIO is always "exists" from the client's perspective.</summary>
    public override bool Exists() => true;

    /// <summary>DDL not supported via REST API.</summary>
    public override void Create() =>
        throw new NotSupportedException("DremIO does not support CREATE DATABASE via the REST API.");

    /// <summary>DDL not supported.</summary>
    public override bool HasTables() => true;

    /// <summary>DDL not supported.</summary>
    public override void CreateTables() =>
        throw new NotSupportedException("DremIO does not support DDL via the REST API.");

    /// <summary>DDL not supported.</summary>
    public override void Delete() =>
        throw new NotSupportedException("DremIO does not support DROP DATABASE via the REST API.");
}

// ── DremioMigrationsSqlGenerator ─────────────────────────────────────────────

/// <summary>
/// Stub <see cref="MigrationsSqlGenerator"/> — DremIO does not support EF Core migrations.
/// </summary>
public sealed class DremioMigrationsSqlGenerator : MigrationsSqlGenerator
{
    public DremioMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies)
        : base(dependencies) { }
}

// ── DremioUpdateSqlGenerator ──────────────────────────────────────────────────

/// <summary>
/// Stub DML SQL generator for DremIO.
/// DremIO is primarily a query engine; UPDATE/INSERT/DELETE are not supported.
/// </summary>
public sealed class DremioUpdateSqlGenerator : UpdateSqlGenerator
{
    public DremioUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
        : base(dependencies) { }
}

// ── DremioModificationCommandBatchFactory ────────────────────────────────────

/// <summary>
/// Factory that produces single-command DML batches (required by EF Core
/// plumbing even when DML is not used for query workloads).
/// </summary>
public sealed class DremioModificationCommandBatchFactory : IModificationCommandBatchFactory
{
    private readonly ModificationCommandBatchFactoryDependencies _dependencies;

    public DremioModificationCommandBatchFactory(
        ModificationCommandBatchFactoryDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public ModificationCommandBatch Create() =>
        new SingularModificationCommandBatch(_dependencies);
}
