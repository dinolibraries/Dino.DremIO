using Dino.DremIO.Models;
using System.Collections;
using System.Data.Common;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// A forward-only <see cref="DbDataReader"/> backed by a <see cref="JobResultReponse"/>.
/// EF Core reads columns by ordinal and then casts to the required CLR type.
/// </summary>
public sealed class DremioDataReader : DbDataReader
{
    private int _rowIndex = -1;
    private bool _closed;
    private readonly IJobResultReader _reader;
    public DremioDataReader(IJobResultReader result)
    {
        _reader = result;
    }

    private object RawValue(int ordinal)
    {
        var name = _reader.Schemas?[ordinal].Name;
        if (name == null) return DBNull.Value;
        return (_reader.CurrentRow?.TryGetValue(name, out var v) ?? false) ? v : DBNull.Value;
    }

    // ── DbDataReader overrides ──────────────────────────────────────────────

    public override int FieldCount => _reader.Schemas?.Count ?? 0;
    public override bool HasRows => _reader.Count > 0;
    public override bool IsClosed => _closed;
    public override int RecordsAffected => -1;
    public override int Depth => 0;

    public override bool Read()
    {
        if (_closed) return false;
        _rowIndex++;
        return _rowIndex < _reader.Count;
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Read());

    public override bool NextResult() => false;

    public override void Close() => _closed = true;

    public override string GetName(int ordinal) => _reader.Schemas?[ordinal].Name ?? "";

    public override int GetOrdinal(string name) =>
        _reader.Schemas?.FindIndex(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) ?? -1;

    public override string GetDataTypeName(int ordinal) =>
        _reader.Schemas?[ordinal].Type?.Name ?? "VARCHAR";

    public override System.Type GetFieldType(int ordinal) => MapType(_reader.Schemas?[ordinal].Type?.Name);

    public override object GetValue(int ordinal)
    {
        var raw = RawValue(ordinal);
        return raw ?? DBNull.Value;
    }

    public override int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, FieldCount);
        for (int i = 0; i < count; i++) values[i] = GetValue(i);
        return count;
    }

    public override bool IsDBNull(int ordinal) => RawValue(ordinal) is null or DBNull;

    public override bool GetBoolean(int ordinal) => Convert.ToBoolean(RawValue(ordinal));
    public override byte GetByte(int ordinal) => Convert.ToByte(RawValue(ordinal));
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
    public override char GetChar(int ordinal) => Convert.ToChar(RawValue(ordinal));
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;
    public override Guid GetGuid(int ordinal) => Guid.Parse(GetString(ordinal));
    public override short GetInt16(int ordinal) => Convert.ToInt16(RawValue(ordinal));
    public override int GetInt32(int ordinal) => Convert.ToInt32(RawValue(ordinal));
    public override long GetInt64(int ordinal) => Convert.ToInt64(RawValue(ordinal));
    public override float GetFloat(int ordinal) => Convert.ToSingle(RawValue(ordinal));
    public override double GetDouble(int ordinal) => Convert.ToDouble(RawValue(ordinal));
    public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(RawValue(ordinal));
    public override DateTime GetDateTime(int ordinal) => Convert.ToDateTime(RawValue(ordinal));
    public override string GetString(int ordinal) => Convert.ToString(RawValue(ordinal)) ?? string.Empty;

    public override IEnumerator GetEnumerator() => new DbEnumerator(this);

    // ── Indexers (required by abstract DbDataReader) ────────────────────────

    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    // ── Type mapping helper ─────────────────────────────────────────────────

    private static System.Type MapType(string? dremioTypeName) => dremioTypeName?.ToUpperInvariant() switch
    {
        "INT" or "INTEGER" => typeof(int),
        "BIGINT" => typeof(long),
        "FLOAT" => typeof(float),
        "DOUBLE" => typeof(double),
        "DECIMAL" => typeof(decimal),
        "BOOLEAN" => typeof(bool),
        "DATE" or "TIME" or "TIMESTAMP" => typeof(DateTime),
        "BINARY" or "VARBINARY" => typeof(byte[]),
        _ => typeof(string)
    };
}
