using Microsoft.AspNetCore.Mvc;
using FisherYates.Services;
using FisherYates.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FisherYatesTests;


namespace ControllerTest
{
    public class ControllerTest
    {
        private readonly Mock<IFisherYatesService> _mockService;
        private readonly FisherYatesWebApp.Controllers.FisherYates _controller;
        private readonly Mock<IOptions<GlobalSettings>> _globalSettings;
        private readonly ILogger<ControllerTest> _logger;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public ControllerTest()
        {
            _globalSettings = new Mock<IOptions<GlobalSettings>>();
            _globalSettings.Setup(x => x.Value).Returns(new GlobalSettings { DummyResult = false });
            _mockService = new Mock<IFisherYatesService>();
            _controller = new FisherYatesWebApp.Controllers.FisherYates(_mockService.Object);

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            _logger = _loggerFactory.CreateLogger<ControllerTest>();
            _logger.LogInformation("Controller Test started");
        }


        [Fact]
        public void ValidInput_ReturnsShuffledResult()
        {
            var input = new FisherYates.Models.ValidRequest { input = "A-B-C-D" };
            _mockService.Setup(x => x.Shuffle("A-B-C-D")).Returns("D-C-B-A");
            var result = _controller.Index(input);
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain; charset=utf-8", contentResult.ContentType);
            Assert.Equal("D-C-B-A", contentResult.Content);
            _mockService.Verify(x => x.Shuffle("A-B-C-D"), Times.Once);
            _logger.LogInformation($"ValidInput working {input.input} {ControllerResult.Print(result)}");

        }

        [Fact]
        public void InvalidModel_ReturnsBadRequest()
        {
            var invalidInput = new FisherYates.Models.ValidRequest { input = "-" };
            _controller.ModelState.AddModelError("Input", "Required");
            var result = _controller.Index(invalidInput);
            Assert.IsType<BadRequestObjectResult>(result);
            _mockService.Verify(x => x.Shuffle(It.IsAny<string>()), Times.Never);
            _logger.LogInformation($"InvalidInput working {ControllerResult.Print(result)}");

        }

        [Fact]
        public void EmptyInput_ReturnsBadRequest()
        {
            var emptyInput = new ValidRequest { input = null };
            _controller.ModelState.AddModelError("Input", "Required");
            var result = _controller.Index(emptyInput);
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.NotNull(badRequestResult);
            _logger.LogInformation($"EmptyInput working {ControllerResult.Print(result)}");
        }

    }
}
