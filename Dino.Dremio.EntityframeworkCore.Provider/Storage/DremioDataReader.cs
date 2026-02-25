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
    private readonly List<Schema> _schema;
    private readonly List<Dictionary<string, object>> _rows;
    private int _rowIndex = -1;
    private bool _closed;

    public DremioDataReader(JobResultReponse result)
    {
        _schema = result.Schema ?? new List<Schema>();
        _rows = result.Rows ?? new List<Dictionary<string, object>>();
    }

    // ── Current row helpers ─────────────────────────────────────────────────

    private Dictionary<string, object> CurrentRow
    {
        get
        {
            if (_rowIndex < 0 || _rowIndex >= _rows.Count)
                throw new InvalidOperationException("No current row.");
            return _rows[_rowIndex];
        }
    }

    private object RawValue(int ordinal)
    {
        var name = _schema[ordinal].Name;
        return CurrentRow.TryGetValue(name, out var v) ? v : DBNull.Value;
    }

    // ── DbDataReader overrides ──────────────────────────────────────────────

    public override int FieldCount => _schema.Count;
    public override bool HasRows => _rows.Count > 0;
    public override bool IsClosed => _closed;
    public override int RecordsAffected => -1;
    public override int Depth => 0;

    public override bool Read()
    {
        if (_closed) return false;
        _rowIndex++;
        return _rowIndex < _rows.Count;
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Read());

    public override bool NextResult() => false;

    public override void Close() => _closed = true;

    public override string GetName(int ordinal) => _schema[ordinal].Name;

    public override int GetOrdinal(string name) =>
        _schema.FindIndex(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    public override string GetDataTypeName(int ordinal) =>
        _schema[ordinal].Type?.Name ?? "VARCHAR";

    public override System.Type GetFieldType(int ordinal) => MapType(_schema[ordinal].Type?.Name);

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
