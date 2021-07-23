using Elmah.Io.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Elmah.Io.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DevIO.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Elmah.Io.AspNetCore.HealthChecks;

namespace DevIO.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "2d38b04954024e2cb4bc2141b580c56c";
                o.LogId = new Guid("05f4ee70-d0d3-4739-91bf-2277ff3eee1e");
            });

            //Configuração manual do Elmah - Não é necessário pois ele mesmo já gerencia isso.
            //services.AddLogging(builder =>
            //{
            //    builder.AddElmahIo(o =>
            //    {
            //        o.ApiKey = "2d38b04954024e2cb4bc2141b580c56c";
            //        o.LogId = new Guid("05f4ee70-d0d3-4739-91bf-2277ff3eee1e");
            //    });
            //    builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            //});

            services.AddHealthChecks()
                .AddElmahIoPublisher(options =>
                {
                    options.ApiKey = "2d38b04954024e2cb4bc2141b580c56c";
                    options.LogId = new Guid("05f4ee70-d0d3-4739-91bf-2277ff3eee1e");
                    options.HeartbeatId = "API Fornecedores";
                })
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQLServer");

            services.AddHealthChecksUI();

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            return app;
        }
    }
}