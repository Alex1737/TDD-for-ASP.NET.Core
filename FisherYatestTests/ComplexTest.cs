using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ControllerTest;
using FisherYates.Models;
using FisherYates.Services;
using FisherYatesTests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Xunit;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace ApplicationText
{

    public class FisherYatesEndpointTests : IClassFixture<WebApplicationFactory<FisherYates.Program>>
    {
        private readonly HttpClient _client;
        private readonly FisherYatesService _service;
        private readonly DefaultRandomNumberGenerator _generator;
        private readonly Mock<IOptions<GlobalSettings>> _globalSettings;
        private readonly ILogger<FisherYatesServiceTests> _logger;
        private readonly ILogger<FisherYatesService> _loggerService;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;
        private readonly IHost _host;

        public FisherYatesEndpointTests()
        {
            _globalSettings = new Mock<IOptions<GlobalSettings>>();
            _globalSettings.Setup(x => x.Value).Returns(new GlobalSettings { DummyResult = false });

            _loggerFactory = CreateLoggerFactory();
            _logger = _loggerFactory.CreateLogger<FisherYatesServiceTests>();
            _loggerService = _loggerFactory.CreateLogger<FisherYatesService>();
            _logger.LogInformation("EndpointTests Test started");

            _generator = new DefaultRandomNumberGenerator(_globalSettings.Object);
            _service = new FisherYatesService(_generator, _globalSettings.Object, _loggerService);

            var hostBuilder = new HostBuilder()
               .ConfigureWebHost(webHost =>
               {
                   webHost.UseTestServer();
                   webHost.ConfigureServices(services =>
                   {
                       services.Configure<GlobalSettings>(options =>
                       {
                           options.DummyResult = false;
                       });

                       services.AddControllers();
                       services.AddMvc();
                       services.AddScoped<IRandomNumberGenerator, DefaultRandomNumberGenerator>();
                       services.AddScoped<IFisherYatesService, FisherYatesService>();
                   });

                   webHost.Configure(app =>
                   {
                       app.UseRouting();
                       app.UseEndpoints(endpoints =>
                       {
                           endpoints.MapControllerRoute(
                               name: "default",
                               pattern: "FisherYates");
                       });
                   });
               });

            _host = hostBuilder.Start();


            var factory = new WebApplicationFactory<FisherYates.Program>()
                            .WithWebHostBuilder(builder =>
                            {
                                builder.UseEnvironment("Development");
                            });

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("http://localhost:7184/")
            });

            _logger.LogInformation("Application endpoint Test started");
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            Console.WriteLine("CreateLoggerFactory call");
            return LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
        }

        [Fact]
        public void EnsureEndpointExists()
        {
            var endpointDataSource = _host.Services.GetRequiredService<EndpointDataSource>();
            Assert.Contains(endpointDataSource.Endpoints,
                e => (e as RouteEndpoint)?.RoutePattern?.RawText?.Contains("FisherYates") == true);
        }


        [Fact]
        public async Task ListEndpoints()
        {
            var endpointDataSource = _host.Services.GetRequiredService<EndpointDataSource>();
            foreach (var endpoint in endpointDataSource.Endpoints)
            {
                var route = (endpoint as RouteEndpoint)?.RoutePattern?.RawText;
                var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.First();
                _logger.LogInformation($"{method} {route}");
            }
        }

        [Fact]
        public async Task GET_ReturnsShuffledResult()
        {
            var response = await _client.GetAsync("/FisherYates?Input=A-B-C-D");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.Matches(@"^[A-D]-[A-D]-[A-D]-[A-D]$", content);
            _logger.LogInformation($"EndpointTests Test completed {content}");
        }
    }
}
