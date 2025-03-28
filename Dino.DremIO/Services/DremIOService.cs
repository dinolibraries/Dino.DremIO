using Dino.DremIO.Common;
using Dino.DremIO.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dino.DremIO.Services
{
    public class DremIOService
    {
        private readonly DremIOClient _dremIOClient;
        public DremIOService(DremIOClient dremIOClient)
        {
            _dremIOClient = dremIOClient;
        }

        public DremIOClient Client => _dremIOClient;

        public Task<TModel?> QueryAsync<TModel>(PayloadSqlRequest payloadSqlRequest, CancellationToken cancellationToken = default) where TModel : class
        {
            return _dremIOClient.PostAsync<TModel>(DremIOUrlHelper.SqlQuery, payloadSqlRequest, cancellationToken);
        }

        public DremIOContext CreateContext(params string[] contexts)
        {
            return new DremIOContext(this, contexts);
        }

        public DremIOJob CreateJob()
        {
            return new DremIOJob(this);
        }
    }

    public class DremIOContext
    {
        private readonly string[] _contexts;
        private readonly DremIOService _service;

        public DremIOContext(DremIOService dremIOService, params string[] contexts)
        {
            _contexts = contexts;
            _service = dremIOService;
        }

        public async Task<string?> QueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            var result = await _service.QueryAsync<Dictionary<string, string>>(new PayloadSqlRequest
            {
                Sql = sqlQuery,
                Context = _contexts
            }, cancellationToken);

            if (result?.TryGetValue("id", out var id) ?? false)
            {
                return id;
            }

            throw new InvalidOperationException("Failed to retrieve query ID from result.");
        }

        public IAsyncEnumerable<Dictionary<string, object>> QueryWaitAsync(string sqlQuery, int timeOut = 30, CancellationToken cancellationToken = default)
        {
            return QueryWaitAsync<Dictionary<string, object>>(sqlQuery, timeOut, cancellationToken);
        }

        public async IAsyncEnumerable<TModel> QueryWaitAsync<TModel>(string sqlQuery, int timeOut = 30, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TModel : class
        {
            var jobId = await QueryAsync(sqlQuery, cancellationToken);
            if (string.IsNullOrEmpty(jobId))
            {
                throw new Exception("Query failed to start!");
            }

            var job = _service.CreateJob();
            var res = await job.WaitAsync(jobId, timeOut, cancellationToken);
            if (res.JobState == JobState.COMPLETED)
            {
                await foreach (var item in job.ResultAllAsync<TModel>(jobId, cancellationToken))
                {
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                throw new Exception($"Job failed to complete, state: {res.JobState}");
            }
        }
    }

    public class DremIOJob
    {
        private readonly DremIOService _service;

        public DremIOJob(DremIOService dremIOService)
        {
            _service = dremIOService;
        }

        public Task<JobGetResponse?> GetAsync(string jobId, CancellationToken cancellationToken = default)
        {
            return _service.Client.GetAsync<JobGetResponse>(string.Format(DremIOUrlHelper.JobGet, jobId), cancellationToken);
        }

        public Task<JobResultReponse<TModel>?> ResultAsync<TModel>(string jobId, int limit = 100, int offset = 0, CancellationToken cancellationToken = default) where TModel : class
        {
            return _service.Client.GetAsync<JobResultReponse<TModel>>(string.Format(DremIOUrlHelper.JobResult, jobId, limit, offset), cancellationToken);
        }

        public Task<JobResultReponse<Dictionary<string, object>>?> ResultAsync(string jobId, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
        {
            return ResultAsync<Dictionary<string, object>>(jobId, limit, offset, cancellationToken);
        }

        public async IAsyncEnumerable<TModel> ResultAllAsync<TModel>(string jobId, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TModel : class
        {
            var result = await ResultAsync<TModel>(jobId, 500, 0, cancellationToken);
            if (result != null)
            {
                var count = result.Rows.Count;
                do
                {
                    foreach (var row in result.Rows)
                    {
                        yield return row;
                    }
                    result = await ResultAsync<TModel>(jobId, 500, count, cancellationToken);
                }
                while (result != null && count < result.RowCount);
            }
        }

        public async Task<JobGetResponse> WaitAsync(string jobId, int timeout = 300, CancellationToken cancellationToken = default)
        {
            var waitHelper = new WaitHelper();
            var result = await waitHelper.WaitAsync(async () =>
            {
                var res = await GetAsync(jobId, cancellationToken);
                return res?.JobState != JobState.COMPLETED && res?.JobState != JobState.CANCELED && res?.JobState != JobState.FAILED ? null : res;
            }, timeout);

            if (result != null)
            {
                return result;
            }

            throw new TimeoutException("Job wait timeout!");
        }
    }
}
