using Dino.DremIO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage
{
    public interface IJobResultReader : IAsyncDisposable
    {
         long Count { get; }
         List<Schema>? Schemas { get; }
         Dictionary<string, object>? CurrentRow { get; }
        Task<bool> MoveNextAsync();   
    }
}
