using Dino.DremIO.Common;
using Dino.DremIO.Models;
using Dino.DremIO.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dino.DremIO.Services
{
    public class DremIOClient
    {
        private readonly HttpClient _httpClient;
        private readonly DremIOOption _option;
        private readonly AuthDremIO _authDremIO;
        public DremIOClient(AuthDremIO authDremIO, IHttpClientFactory httpClientFactory, IOptions<DremIOOption> options)
        {
            _option = options.Value;
            _authDremIO = authDremIO;
            _httpClient = httpClientFactory.CreateClient(Contants.DremIOClientKey);
            _httpClient.BaseAddress = new Uri(_option.EndpointUrl);
        }
        public async Task<HttpClient> GetHttpClientAsync(CancellationToken cancellationToken = default)
        {
            var token = await _authDremIO.GetAccessTokenAsync(cancellationToken);
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return _httpClient;
        }
        public async Task<TModel?> PostAsync<TModel>(string url,object payloadRequest, CancellationToken cancellationToken = default) where TModel : class
        {
            var client = await GetHttpClientAsync(cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var content = new StringContent(JsonConvert.SerializeObject(payloadRequest), null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result == null ? null : typeof(TModel) == typeof(string) ? result as TModel : JsonConvert.DeserializeObject<TModel>(result);
        }
     

        public async Task<TModel?> GetAsync<TModel>(string url, CancellationToken cancellationToken = default) where TModel : class
        {
            var client = await GetHttpClientAsync(cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result == null ? null : typeof(TModel) == typeof(string) ? result as TModel : JsonConvert.DeserializeObject<TModel>(result);
        }

    }
}
