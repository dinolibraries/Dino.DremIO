using Dino.DremIO.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Dino.DremIO.Tests
{
    public class AuthenticationTest
    {
        [Fact]
        public async Task GetTokenResponse()
        {
            var provider = HostBuilder.Create().Provider;
            var auth = provider.GetRequiredService<AuthDremIO>();
            var data = await auth.GetTokenInfoAsync();
        }
        [Fact]
        public async Task GetAccessTokenResponse()
        {
            var provider = HostBuilder.Create().Provider;
            var auth = provider.GetRequiredService<AuthDremIO>();
            var data = await auth.GetAccessTokenAsync();
        }

        [Fact]
        public async Task ClientTestAsync()
        {
            var provider = HostBuilder.Create().Provider;
            var client = provider.GetRequiredService<DremIOClient>();

            var httpBase =await client.GetHttpClientAsync();


            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v3/job/181abcbd-0f74-11c4-e235-007a99974900/results");
            var content = new StringContent(string.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            var response = await httpBase.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
        }
        [Fact]
        public async Task JobAsync()
        {
            var provider = HostBuilder.Create().Provider;
            var service = provider.GetRequiredService<DremIOService>();

            var job = service.CreateJob();
            var res = await job.GetAsync("181abcbd-0f74-11c4-e235-007a99974900");
            var res2 = await job.ResultAsync("181abcbd-0f74-11c4-e235-007a99974900",limit:500);
        }
        [Fact]
        public async Task QueryAllAsync()
        {
            var provider = HostBuilder.Create().Provider;
            var service = provider.GetRequiredService<DremIOService>();

            var context = service.CreateContext("analytic-store");

            var results =await context.QueryWaitAsync("SELECT * FROM \"dremio-store\" LIMIT 100").ToListAsync();
        }
    }
}