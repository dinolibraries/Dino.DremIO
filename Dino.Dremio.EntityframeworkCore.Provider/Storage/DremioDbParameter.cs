using System.Collections;
using System.Data;
using System.Data.Common;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>Minimal <see cref="DbParameter"/> for DremIO commands.</summary>
public sealed class DremioDbParameter : DbParameter
{
    public override string ParameterName { get; set; } = string.Empty;
    public override object? Value { get; set; }
    public override DbType DbType { get; set; } = DbType.String;
    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    public override bool IsNullable { get; set; } = true;
    public override string SourceColumn { get; set; } = string.Empty;
    public override bool SourceColumnNullMapping { get; set; }
    public override int Size { get; set; }
    public override void ResetDbType() => DbType = DbType.String;
}

/// <summary>Minimal <see cref="DbParameterCollection"/> for DremIO commands.</summary>
public sealed class DremioDbParameterCollection : DbParameterCollection
{
    private readonly List<DremioDbParameter> _items = new();

    public override int Count => _items.Count;
    public override object SyncRoot => _items;

    public override int Add(object value)
    {
        _items.Add((DremioDbParameter)value);
        return _items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (object v in values) Add(v);
    }

    public override void Clear() => _items.Clear();

    public override bool Contains(object value) => _items.Contains((DremioDbParameter)value);
    public override bool Contains(string value) =>
        _items.Any(p => string.Equals(p.ParameterName, value, StringComparison.OrdinalIgnoreCase));

    public override void CopyTo(Array array, int index) =>
        ((ICollection)_items).CopyTo(array, index);

    public override System.Collections.IEnumerator GetEnumerator() => _items.GetEnumerator();

    public override int IndexOf(object value) => _items.IndexOf((DremioDbParameter)value);
    public override int IndexOf(string parameterName) =>
        _items.FindIndex(p => string.Equals(p.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase));

    public override void Insert(int index, object value) => _items.Insert(index, (DremioDbParameter)value);

    public override void Remove(object value) => _items.Remove((DremioDbParameter)value);
    public override void RemoveAt(int index) => _items.RemoveAt(index);
    public override void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));

    protected override DbParameter GetParameter(int index) => _items[index];
    protected override DbParameter GetParameter(string parameterName) => _items[IndexOf(parameterName)];

    protected override void SetParameter(int index, DbParameter value) => _items[index] = (DremioDbParameter)value;
    protected override void SetParameter(string parameterName, DbParameter value) =>
        _items[IndexOf(parameterName)] = (DremioDbParameter)value;
}
