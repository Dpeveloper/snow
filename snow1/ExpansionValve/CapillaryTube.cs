using snow1.enums;
using snow1.Interface;
using snow1.Refrigerant;

public class CapillaryTube : IComponent
{
    public string Name => Type.ToString();
    public ComponentType Type => ComponentType.ExpansionValve;

    private double targetPressure;
    private RefrigerantProperties props;

    public CapillaryTube(RefrigerantProperties props)
    {
        this.props = props;
    }

    public void SetTargetPressure(double pressure)
    {
        targetPressure = pressure;
    }

    public RefrigerantState Process(RefrigerantState input)
    {
        if (targetPressure <= 0)
            throw new InvalidOperationException("La presión objetivo no ha sido configurada en el tubo capilar.");

        RefrigerantState output = input.Clone();
        output.Pressure = targetPressure;
        output.Enthalpy = input.Enthalpy;
        output.Temperature = props.GetTemperatureFromPressure(targetPressure);
        output.Entropy = props.GetEntropyFromPressure(targetPressure);

        return output;
    }

    public bool CanConnectTo(IComponent next)
    {
        return next.Type == ComponentType.Evaporator;
    }
}
