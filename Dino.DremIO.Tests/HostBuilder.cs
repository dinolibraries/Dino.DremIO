using Dino.DremIO.Infrastructures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.DremIO.Tests
{
    class HostBuilder
    {
        private readonly IHost _host;
        private readonly IServiceScope _serviceScope;
        public HostBuilder()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddUserSecrets<HostBuilder>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.AddDremIoService(hostContext.Configuration);
                }).Build();
            _serviceScope = _host.Services.CreateScope();
        }
        public IServiceProvider Provider { get => _serviceScope.ServiceProvider; }

        public static HostBuilder Create()
        {
            return new HostBuilder();
        }
    }
}
