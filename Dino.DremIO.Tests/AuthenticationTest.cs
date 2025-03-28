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
            //Environment.SetEnvironmentVariable("DremIOOption__TokenStore", "Testing");

            var provider = HostBuilderTest.Create().Provider;
            var auth = provider.GetRequiredService<AuthDremIO>();
            var data = await auth.GetTokenInfoAsync();
        }
        [Fact]
        public async Task GetAccessTokenResponse()
        {
            var provider = HostBuilderTest.Create().Provider;
            var auth = provider.GetRequiredService<AuthDremIO>();
            var data = await auth.GetAccessTokenAsync();
        }

        [Fact]
        public async Task ClientTestAsync()
        {
            var provider = HostBuilderTest.Create().Provider;
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
            var provider = HostBuilderTest.Create().Provider;
            var service = provider.GetRequiredService<DremIOService>();

            var job = service.CreateJob();
            var res = await job.GetAsync("181abcbd-0f74-11c4-e235-007a99974900");
            var res2 = await job.ResultAsync("181abcbd-0f74-11c4-e235-007a99974900",limit:500);
        }
        [Fact]
        public async Task QueryAllAsync()
        {
            var provider = HostBuilderTest.Create().Provider;
            var service = provider.GetRequiredService<DremIOService>();

            var context = service.CreateContext("analytic-space");

            var results =await context.QueryWaitAsync("SELECT tablerv.video_id,tablerv.\"date\",SUM(tablerv.views) AS views,SUM(tablerv.watch_time_minutes) AS watch_time_minutes,SUM(tablerv.estimated_partner_revenue) AS estimated_partner_revenue FROM \"analytic-space\".\"full-network-youtube-revenue\" AS tablerv  WHERE tablerv.video_id IN ('xXdVUvzZFlE','L-b48Yp-tzU','jBw6HCA7qY0','4tnKXqXthX0','SScFFXyyAWg','6FJ2Xru6a3Q','GUGrQjU_OYk','7gkpzqdsZ4M','7Q70XtrZcCw','DrgfcC6Mq20','opGi6OtYoxA','juNHfZ1x8M0','11_ZIFWeol4','U-b1fv1yHMQ','SEFpjQvGs9Y','KqgZskf_APE','Wh5F0AKyK00','6x82Ogs1Ld8','1F0JB_J8SAI','I03Yi1iFli0','wXoU0Il7pMw','w7hL8_OK6LM','FTxIP9pgrsM','pYotg_AO98E','08Rwtmjzncs','A6rk3fxBY_Y','rBxPL92bBMk','uDwtOBr8M5o','iwp4AEwLNJU','VGMJTpdFhrg','dWvG5jp1tDY','t_CyM2F4iCY','HlyeSb-vtGU','lvJTPuNnalU','_OTbjdEeyWg','qgtZ4smvLUw','bxejXu_MP20','WgTQtA3IZRA','nIovXECtjl0','Ltp9DVFui6k','s3H5KKS-u78','Li0n1Gmgjcc','McZrDFxL2Mw','WXgijTD48is','6cXIEsWLqgQ','Rud6jBHlOdA','HxpOKRKyJ5I','RfIau4dYEYM','C53u2uxjuDM','d3eLccTJwIs')  AND tablerv.\"date\" <= '20250326' AND tablerv.\"date\" >= '20250227'\r\nGROUP BY tablerv.video_id,tablerv.\"date\"\r\nORDER BY views DESC OFFSET 0 ROWS  FETCH NEXT 3080 ROWS ONLY ").ToListAsync();
        }
    }
}