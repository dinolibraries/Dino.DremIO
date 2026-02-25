using Dino.DremIO.Options;
using Dino.DremIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// EF Core's <see cref="IRelationalConnection"/> for DremIO.
/// Wraps <see cref="DremioDbConnection"/> (the "fake" ADO.NET connection) and
/// integrates it with the EF Core relational pipeline.
/// </summary>
public sealed class DremioRelationalConnection : RelationalConnection
{
    private readonly DremIOService _dremioService;
    private readonly DremIOOption _dremioOption;

    public DremioRelationalConnection(
        RelationalConnectionDependencies dependencies,
        DremIOService dremioService,
        DremIOOption dremioOption)
        : base(dependencies)
    {
        _dremioService = dremioService;
        _dremioOption = dremioOption;
    }

    /// <summary>
    /// EF Core calls this to create the underlying ADO.NET connection.
    /// We return our <see cref="DremioDbConnection"/> which in turn executes
    /// queries via the DremIO REST API.
    /// </summary>
    protected override DbConnection CreateDbConnection() =>
        new DremioDbConnection(_dremioService, _dremioOption);
}
