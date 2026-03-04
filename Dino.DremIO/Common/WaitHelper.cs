using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dino.DremIO.Common
{
    public class WaitHelper : IWaitHelper
    {
        public async Task<TModel?> WaitAsync<TModel>(Func<Task<TModel?>> condition, int timeout = 300, CancellationToken cancellationToken = default) where TModel : class
        {
            int elapsed = 0;
            int delay = 1000; // Thời gian chờ giữa các lần kiểm tra (ms)
            cancellationToken.ThrowIfCancellationRequested();
            while (elapsed < timeout * 1000)
            {
                var result = await condition().ConfigureAwait(false);
                if (result != null)
                {
                    return result; // Trả về nếu điều kiện thỏa mãn
                }

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false); // Chờ một khoảng thời gian ngắn
                elapsed += delay;
            }

            return default; // Hết thời gian chờ, trả về null hoặc giá trị mặc định
        }
    }
}
