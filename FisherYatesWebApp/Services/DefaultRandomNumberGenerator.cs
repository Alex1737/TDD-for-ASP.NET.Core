using FisherYates.Models;
using Microsoft.Extensions.Options;

namespace FisherYates.Services
{
    public class DefaultRandomNumberGenerator : IRandomNumberGenerator
    {
        private readonly Random _random = new Random();
        private readonly GlobalSettings _globalSettings;

        public DefaultRandomNumberGenerator()
        {
        }

        public DefaultRandomNumberGenerator(IOptions<GlobalSettings> globalSettings)
        {
            _globalSettings = globalSettings.Value;
        }

        public int Next(int minValue, int maxValue)
        {
            if (_globalSettings.DummyResult)
                throw new NotImplementedException();
            else
                return _random.Next(minValue, maxValue);
        }
    }
}