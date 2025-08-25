using snow1.enums;
using snow1.Interface;
using snow1.Refrigerant;

public class BasicEvaporator : IComponent
{
    private double ambientAirTemp;   // K
    private double heatTransferArea; // m²
    private double uValue;           // kW/m²·K
    private double operatingPressure;
    private RefrigerantProperties props;

    public string Name => Type.ToString();
    public ComponentType Type => ComponentType.Evaporator;

    public BasicEvaporator(double ambientAirTemp, double heatTransferArea, double uValue, double operatingPressure, RefrigerantProperties props)
    {
        this.ambientAirTemp = ambientAirTemp;
        this.heatTransferArea = heatTransferArea;
        this.uValue = uValue;
        this.operatingPressure = operatingPressure;
        this.props = props;
    }

    public double GetPressure()
    {
        return operatingPressure;
    }

    public void SetAmbientTemperature(double newTemp)
    {
        ambientAirTemp = newTemp;
    }

    public RefrigerantState Process(RefrigerantState input)
    {
        double deltaT = ambientAirTemp - input.Temperature;
        double q = uValue * heatTransferArea * deltaT; // Q en kW
        double hOut = input.Enthalpy + q / input.MassFlowRate; // Δh = Q / ṁ

        RefrigerantState output = input.Clone();
        output.Enthalpy = hOut;
        output.Temperature = props.GetTemperatureFromPressure(operatingPressure);
        output.Entropy = props.GetEntropyFromPressure(operatingPressure);
        output.Pressure = operatingPressure;

        return output;
    }

    public bool CanConnectTo(IComponent next)
    {
        return next.Type == ComponentType.Compressor;
    }
}
