using Microsoft.EntityFrameworkCore.Storage;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// Generates Dremio-flavoured SQL helper strings.
/// Dremio uses ANSI double-quote delimiters for identifiers.
/// </summary>
public sealed class DremioSqlGenerationHelper : RelationalSqlGenerationHelper
{
    public DremioSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies) { }

    // Dremio uses double quotes for identifiers (ANSI standard).
    public override string DelimitIdentifier(string identifier)
        => $"\"{EscapeIdentifier(identifier)}\"";

    public override void DelimitIdentifier(System.Text.StringBuilder builder, string identifier)
    {
        builder.Append('"');
        EscapeIdentifier(builder, identifier);
        builder.Append('"');
    }
}
