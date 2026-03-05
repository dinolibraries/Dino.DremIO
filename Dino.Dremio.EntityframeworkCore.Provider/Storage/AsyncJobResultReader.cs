using Dino.DremIO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage
{
    internal class AsyncJobResultReader : IJobResultReader
    {
        private readonly IAsyncEnumerator<(List<Schema>, Dictionary<string, object>)> _enumerator;
        private readonly long _count;
        public AsyncJobResultReader(long count,IAsyncEnumerable<(List<Schema>, Dictionary<string, object>)> data)
        {
            _enumerator = data.GetAsyncEnumerator();
            _count = count;
        }
        public (List<Schema>, Dictionary<string, object>)? Current;
        public Dictionary<string, object>? CurrentRow { get => Current?.Item2; }

        public long Count { get => _count; }
        public List<Schema>? Schemas { get => Current?.Item1; }

        public async Task<bool> MoveNextAsync()
        {
            if (await _enumerator.MoveNextAsync())
            {
                Current = _enumerator.Current;
                return true;
            }
            return false;
        }
    }
}
