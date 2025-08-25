using snow1.Compressors;
using snow1.enums;
using snow1.Interface;
using snow1.Refrigerant;

public class Compressor : IComponent
{
    private double compressionRatio;
    private ICompressionModel model;

    public double PowerConsumed { get; private set; }

    public string Name => "Compressor";
    public ComponentType Type => ComponentType.Compressor;

    public Compressor(double compressionRatio, ICompressionModel compressionModel)
    {
        this.compressionRatio = compressionRatio;
        this.model = compressionModel;
    }

    public RefrigerantState Process(RefrigerantState input)
    {
        double targetPressure = input.Pressure * compressionRatio;
        RefrigerantState output = model.Compute(input, targetPressure);
        PowerConsumed = input.MassFlowRate * (output.Enthalpy - input.Enthalpy);
        return output;
    }

    public bool CanConnectTo(IComponent next)
    {
        return next.Type == ComponentType.Condenser;
    }
}
