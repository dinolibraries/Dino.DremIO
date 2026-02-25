using Microsoft.EntityFrameworkCore.Storage;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// Maps CLR types ↔ Dremio SQL types.
/// Dremio broadly follows SQL/Apache Arrow type naming, so we extend the
/// default relational mapping and override the Dremio-specific names.
/// </summary>
public sealed class DremioTypeMappingSource : RelationalTypeMappingSource
{
    // Dremio type-name constants
    private const string Int = "INTEGER";
    private const string BigInt = "BIGINT";
    private const string Float = "FLOAT";
    private const string Double = "DOUBLE";
    private const string Decimal = "DECIMAL(38,9)";
    private const string Bool = "BOOLEAN";
    private const string Varchar = "VARCHAR";
    private const string Timestamp = "TIMESTAMP";
    private const string Date = "DATE";
    private const string Time = "TIME";
    private const string Varbinary = "VARBINARY";

    // Map from CLR type → default Dremio type name.
    private static readonly Dictionary<Type, RelationalTypeMapping> ClrMappings = new()
    {
        { typeof(bool),     new BoolTypeMapping(Bool) },
        { typeof(byte),     new ByteTypeMapping(Int) },
        { typeof(short),    new ShortTypeMapping(Int) },
        { typeof(int),      new IntTypeMapping(Int) },
        { typeof(long),     new LongTypeMapping(BigInt) },
        { typeof(float),    new FloatTypeMapping(Float) },
        { typeof(double),   new DoubleTypeMapping(Double) },
        { typeof(decimal),  new DecimalTypeMapping(Decimal) },
        { typeof(string),   new StringTypeMapping(Varchar, System.Data.DbType.String) },
        { typeof(DateTime), new DateTimeTypeMapping(Timestamp) },
        { typeof(DateOnly), new DateOnlyTypeMapping(Date) },
        { typeof(TimeOnly), new TimeOnlyTypeMapping(Time) },
        { typeof(Guid),     new GuidTypeMapping(Varchar) },
        { typeof(byte[]),   new ByteArrayTypeMapping(Varbinary) },
    };

    // Map from Dremio type name → mapping (for round-trips from DB metadata).
    private static readonly Dictionary<string, RelationalTypeMapping> StoreMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "INTEGER",   new IntTypeMapping(Int) },
            { "INT",       new IntTypeMapping(Int) },
            { "BIGINT",    new LongTypeMapping(BigInt) },
            { "FLOAT",     new FloatTypeMapping(Float) },
            { "DOUBLE",    new DoubleTypeMapping(Double) },
            { "DECIMAL",   new DecimalTypeMapping(Decimal) },
            { "BOOLEAN",   new BoolTypeMapping(Bool) },
            { "VARCHAR",   new StringTypeMapping(Varchar, System.Data.DbType.String) },
            { "CHAR",      new StringTypeMapping("CHAR", System.Data.DbType.String) },
            { "TIMESTAMP", new DateTimeTypeMapping(Timestamp) },
            { "DATE",      new DateOnlyTypeMapping(Date) },
            { "TIME",      new TimeOnlyTypeMapping(Time) },
            { "VARBINARY", new ByteArrayTypeMapping(Varbinary) },
            { "BINARY",    new ByteArrayTypeMapping("BINARY") },
        };

    public DremioTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies, relationalDependencies) { }

    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        // 1. Try by store type name first.
        if (mappingInfo.StoreTypeName is { } storeName &&
            StoreMappings.TryGetValue(storeName, out var storeMapping))
            return storeMapping;

        // 2. Try by CLR type.
        var clrType = mappingInfo.ClrType;
        if (clrType is not null)
        {
            // Unwrap nullable.
            var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (ClrMappings.TryGetValue(underlyingType, out var clrMapping))
                return clrMapping;

            // Enums → INTEGER
            if (underlyingType.IsEnum)
                return new IntTypeMapping(Int);
        }

        return base.FindMapping(mappingInfo);
    }
}
