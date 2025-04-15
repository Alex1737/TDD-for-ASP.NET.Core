using FisherYates.Models;
using FisherYates.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FisherYatesTests
{
    public class FisherYatesServiceTests : IDisposable
    {
        private readonly FisherYatesService _service;
        private readonly DefaultRandomNumberGenerator _generator;
        private readonly Mock<IOptions<GlobalSettings>> _globalSettings;
        private readonly ILogger<FisherYatesServiceTests> _logger;
        private readonly ILogger<FisherYatesService> _loggerService;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;
        public FisherYatesServiceTests()
        {
            _globalSettings = new Mock<IOptions<GlobalSettings>>();
            _globalSettings.Setup(x => x.Value).Returns(new GlobalSettings { DummyResult = false });
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            _logger = _loggerFactory.CreateLogger<FisherYatesServiceTests>();
            _loggerService = _loggerFactory.CreateLogger<FisherYatesService>();
            _logger.LogInformation("FisherYates Service Test started");
            _generator = new DefaultRandomNumberGenerator(_globalSettings.Object);
            _service = new FisherYatesService(_generator, _globalSettings.Object, _loggerService);

        }

        [Fact]
        public void Shuffle_ReturnsSameLengthAsInput()
        {
            string input = "A-B-C-D";
            string result = _service.Shuffle(input);
            Assert.Equal(input.Length, result.Length);
            _logger.LogInformation($"SameLengthAsInput {input} {result}");
        }

        [Fact]
        public void Shuffle_ContainsAllOriginalElements()
        {
            string input = "X-Y-Z";
            string result = _service.Shuffle(input);
            Assert.Contains("X", result);
            Assert.Contains("Y", result);
            Assert.Contains("Z", result);
            _logger.LogInformation($"ContainsAllOriginalElements {input} {result}");
        }

        [Fact]
        public void Shuffle_ProducesDifferentOrderWithDifferentRandomSeeds()
        {
            string input = "A-B-C-D";

            var mockRandom1 = new Mock<IRandomNumberGenerator>();
            mockRandom1.SetupSequence(r => r.Next(0, It.IsAny<int>()))
                      .Returns(3).Returns(0).Returns(1);

            var mockRandom2 = new Mock<IRandomNumberGenerator>();
            mockRandom2.SetupSequence(r => r.Next(0, It.IsAny<int>()))
                      .Returns(1).Returns(0).Returns(2);

            var service1 = new FisherYatesService(mockRandom1.Object, _globalSettings.Object, _loggerService);
            var service2 = new FisherYatesService(mockRandom2.Object, _globalSettings.Object, _loggerService);

            string result1 = service1.Shuffle(input);
            string result2 = service2.Shuffle(input);

            Assert.NotEqual(result1, result2);
            _logger.LogInformation($"ProducesDifferentOrderWithDifferentRandom {result1} {result2}");
        }

        [Fact]
        public void Shuffle_PreservesSingleElement()
        {
            string input = "A";
            string result = _service.Shuffle(input);
            Assert.Equal("A", result);
            _logger.LogInformation($"PreservesSingleElement {input} {result}");
        }

        [Theory]
        [InlineData("A-B-C-D", @"^[A-D]-[A-D]-[A-D]-[A-D]$")] // Basic
        [InlineData("X-Y", @"^[X-Y]-[X-Y]$")] // Minimal input
        [InlineData("A", "A")] // Single element
        public void Shuffle_Parameterized(string input, string regexPattern)
        {
            var service = new FisherYatesService(_generator, _globalSettings.Object, _loggerService);
            var result = service.Shuffle(input);
            Assert.Matches(regexPattern, result);
            _logger.LogInformation($"Parameterized service Tests {result}");
        }

        [Fact]
        public void Shuffle_ProducesUniformDistribution()
        {
            const string input = "A-B-C";
            var results = new Dictionary<string, int>();

            for (int i = 0; i < 1000; i++)
            {
                var shuffled = _service.Shuffle(input);
                results[shuffled] = results.GetValueOrDefault(shuffled) + 1;
            }

            // Should have ~6 possible permutations
            Assert.Equal(6, results.Count);

            // Each permutation should appear ~16.7% of the time
            foreach (var count in results.Values)
            {
                Assert.InRange(count, 100, 200); // Allow ±10% variance
            }
            _logger.LogInformation($"Statistical Tests sucessfull ");
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }


    }
}
