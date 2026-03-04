using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dino.DremIO.Common
{
    public interface IWaitHelper
    {
        Task<TModel?> WaitAsync<TModel>(Func<Task<TModel?>> condition, int timeout = 300, CancellationToken cancellationToken = default) where TModel : class;
    }
}
