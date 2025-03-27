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
    public class AuthDremIO
    {
        private readonly DremIOOption _options;
        private readonly HttpClient _httpClient;
        public AuthDremIO(IOptions<DremIOOption> options, IHttpClientFactory httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient.CreateClient(Contants.DremIOClientKey);
            _httpClient.BaseAddress = new Uri(_options.EndpointUrl);
        }

        public async Task<TokenResponse?> GetTokenInfoAsync(CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/apiv2/login");
            var content = new StringContent(JsonConvert.SerializeObject(new
            {
                userName = _options.UserName,
                password = _options.Password
            }), null, "application/json");
            request.Content = content;
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return result == null ? null : JsonConvert.DeserializeObject<TokenResponse>(result);
        }
        public const string TokenStoreKey = "dremio.token";
        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var store = new JsonStorage(_options.TokenStore);
            var tokenInfo = await store.GetAsync<TokenResponse>(TokenStoreKey);
            if (tokenInfo != null)
            {
                return tokenInfo.Token;
            }
            tokenInfo = await GetTokenInfoAsync(cancellationToken);
            if (tokenInfo == null) throw new ArgumentNullException(nameof(tokenInfo));
            await store.AddAsync(TokenStoreKey, tokenInfo, DateTimeOffset.FromUnixTimeMilliseconds(tokenInfo.Expires) - DateTime.Now.AddHours(+1));
            return tokenInfo.Token;
        }
    }
}
