namespace FisherYates.Services
{
    public interface IRandomNumberGenerator
    {
        int Next(int minValue, int maxValue);
    }
}
