using snow1.Refrigerant;

namespace snow1.Compressors
{
    public interface ICompressionModel
    {
        RefrigerantState Compute(RefrigerantState input, double targetPressure);
    }
}