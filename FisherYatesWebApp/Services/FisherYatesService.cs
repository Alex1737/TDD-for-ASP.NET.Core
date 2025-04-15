using System.Security.Cryptography;
using FisherYates.Models;
using Microsoft.Extensions.Options;

namespace FisherYates.Services
{
    public class FisherYatesService : IFisherYatesService
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;
        private readonly GlobalSettings _globalSettings;
        private readonly ILogger<FisherYatesService> _logger;

        public FisherYatesService(IRandomNumberGenerator randomNumberGenerator,
            IOptions<GlobalSettings> globalSettings,
            ILogger<FisherYatesService> logger)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _globalSettings = globalSettings.Value;
            _logger = logger;
        }

        public string Shuffle(string input)
        {
            if (_globalSettings.DummyResult)
            {
                _logger.LogDebug("Dummy result required");
                throw new NotImplementedException();
            }
            else
            {
                var elements = input.Split("-");
                for (int i = elements.Length - 1; i > 0; i--)
                {
                    int j = _randomNumberGenerator.Next(0, i + 1);
                    _logger.LogDebug($"Swapping index {i} ({elements[i]}) with {j} ({elements[j]})");
                    (elements[i], elements[j]) = (elements[j], elements[i]);
                   }
                var result = string.Join("-", elements);
                _logger.LogDebug($"Shuffle completed {result}");
                return result;
            }
        }
    }
}
