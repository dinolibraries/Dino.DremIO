using Dino.DremIO.Common;
using Dino.DremIO.Options;
using Dino.DremIO.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dino.DremIO.Infrastructures
{
    public static class ConfigurationExtension
    {

        public static IServiceCollection AddDremIoService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(Contants.DremIOClientKey, option =>
            {
            });
            services.Configure<DremIOOption>(configuration.GetSection(nameof(DremIOOption)));
            services.AddSingleton<AuthDremIO>();
            services.AddSingleton<DremIOClient>();
            services.AddSingleton<DremIOService>();
            return services;
        }
    }
}
