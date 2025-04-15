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

namespace DiContainerTest
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<FisherYates.Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<FisherYates.Program> _factory;
        private readonly FisherYatesService _testService;
        private readonly Mock<IOptions<GlobalSettings>> _mockGlobalSettings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly DefaultRandomNumberGenerator _generator;
        private readonly ILogger<IntegrationTests> _logger;
        private readonly ILogger<FisherYatesService> _loggerService;

        public IntegrationTests()
        {
            _mockGlobalSettings = new Mock<IOptions<GlobalSettings>>();
            _mockGlobalSettings.Setup(x => x.Value).Returns(new GlobalSettings { DummyResult = false });

            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _generator = new DefaultRandomNumberGenerator(_mockGlobalSettings.Object);
            _loggerService = _loggerFactory.CreateLogger<FisherYatesService>();
            _testService = new FisherYatesService(_generator, _mockGlobalSettings.Object, _loggerService);
            _logger = _loggerFactory.CreateLogger<IntegrationTests>();

            _factory = new WebApplicationFactory<FisherYates.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Development");
                    builder.ConfigureTestServices(services =>
                    {
                        var descriptor = services.FirstOrDefault(d =>
                            d.ServiceType == typeof(IFisherYatesService));
                        if (descriptor != null) services.Remove(descriptor);

                        services.AddSingleton<IFisherYatesService>(_testService);
                    });
                });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost:7184"),
                HandleCookies = true
            });
            _logger.LogInformation("Integraion DI container service test started");
        }

        [Fact]
        public async Task GET_ReturnsShuffledResult()
        {
            var response = await _client.GetAsync("/FisherYates?Input=A-B-C-D");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Matches(@"^[A-D]-[A-D]-[A-D]-[A-D]$", content);
            _logger.LogInformation($"Endpoint working {content}");
        }

        [Fact]
        public void ServiceInjection_WorksCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetService<IFisherYatesService>();
            Assert.Same(_testService, service);
            _logger.LogInformation($"ServiceInjection WorksCorrectly");
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }

}
