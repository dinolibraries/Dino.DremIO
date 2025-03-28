using Dino.DremIO.Common;
using Dino.DremIO.Models;
using Dino.DremIO.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dino.DremIO.Services
{
    public class DremIOClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DremIOOption _option;
        private readonly AuthDremIO _authDremIO;

        public DremIOClient(AuthDremIO authDremIO, IHttpClientFactory httpClientFactory, IOptions<DremIOOption> options)
        {
            _authDremIO = authDremIO;
            _httpClientFactory = httpClientFactory;
            _option = options.Value;
        }

        public async Task<HttpClient> GetHttpClientAsync(CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient(Contants.DremIOClientKey);
            client.BaseAddress = new Uri(_option.EndpointUrl);

            var token = await _authDremIO.GetAccessTokenAsync(cancellationToken);
            if (client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
            }
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            return client;
        }

        public async Task<TModel?> PostAsync<TModel>(string url, object payloadRequest, CancellationToken cancellationToken = default) where TModel : class
        {
            var client = await GetHttpClientAsync(cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payloadRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"POST {url} failed: {response.StatusCode} - {result}");
            }

            return string.IsNullOrWhiteSpace(result) ? null : (typeof(TModel) == typeof(string) ? result as TModel : JsonConvert.DeserializeObject<TModel>(result));
        }

        public async Task<TModel?> GetAsync<TModel>(string url, CancellationToken cancellationToken = default) where TModel : class
        {
            var client = await GetHttpClientAsync(cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await client.SendAsync(request, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GET {url} failed: {response.StatusCode} - {result}");
            }

            return string.IsNullOrWhiteSpace(result) ? null : (typeof(TModel) == typeof(string) ? result as TModel : JsonConvert.DeserializeObject<TModel>(result));
        }
    }
}
