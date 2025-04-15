using FisherYates.Models;
using FisherYates.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace RandomNumberService
{
    public class DefaultRandomNumberGeneratorTests : IDisposable
    {
        private readonly Mock<IOptions<GlobalSettings>> _globalSettings;
        private readonly DefaultRandomNumberGenerator _generator;
        private readonly ILogger<DefaultRandomNumberGeneratorTests> _logger;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public DefaultRandomNumberGeneratorTests()
        {

            _globalSettings = new Mock<IOptions<GlobalSettings>>();
            _globalSettings.Setup(x => x.Value).Returns(new GlobalSettings { DummyResult = false });
            _generator = new DefaultRandomNumberGenerator(_globalSettings.Object);
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            _logger = _loggerFactory.CreateLogger<DefaultRandomNumberGeneratorTests>();
            _logger.LogInformation("RandomNumberGenerator Service Test started");
        }

        [Fact]
        public void Next_ReturnsValueInSpecifiedRange()
        {
            int min = 1;
            int max = 5;
            int result = _generator.Next(min, max);
            Assert.InRange(result, min, max - 1);
            _logger.LogInformation($"ReturnsValueInSpecifiedRange finished {result}");
        }

[Fact]
public void Next_ReturnsDifferentValuesOnSubsequentCalls()
{
    int min = 0;
    int max = 10;
    var results = new HashSet<int>();

    // Try multiple times to account for randomness
    for (int i = 0; i < 10; i++)
    {
        results.Add(_generator.Next(min, max));
    }

    Assert.True(results.Count > 1, 
        $"Expected varied outputs but got {results.Count} unique values");
    
    _logger.LogInformation($"Got {results.Count} unique values: {string.Join(",", results)}");
}

        [Theory]
        [InlineData(0, 1)] 
        [InlineData(5, 10)] 
        [InlineData(-5, 0)] 
        public void Next_HandlesVariousRanges(int min, int max)
        {
            int result = _generator.Next(min, max);
            Assert.InRange(result, min, max - 1);
            _logger.LogInformation($"HandlesVariousRanges finished {result}");
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}
