using snow1.enums;
using snow1.Interface;
using snow1.Refrigerant;

namespace snow1.Condenser
{
    public class BasicCondenser : IComponent
    {
        private double ambientAirTemp;     // Temperatura del aire ambiente (K)
        private double heatTransferArea;   // Área de transferencia (m²)
        private double uValue;             // Coef. global de transferencia térmica (kW/m²·K)
        private double operatingPressure;  // Presión de condensación (Pa)
        private RefrigerantProperties props;

        public string Name => Type.ToString();
        public ComponentType Type => ComponentType.Condenser;

        public BasicCondenser(double ambientAirTemp, double heatTransferArea, double uValue, double operatingPressure, RefrigerantProperties props)
        {
            this.ambientAirTemp = ambientAirTemp;
            this.heatTransferArea = heatTransferArea;
            this.uValue = uValue;
            this.operatingPressure = operatingPressure;
            this.props = props;
        }

        public RefrigerantState Process(RefrigerantState input)
        {
            double deltaT = input.Temperature - ambientAirTemp;               // Diferencia de temperatura (K)
            double q = uValue * heatTransferArea * deltaT;                    // Transferencia de calor (kW)
            double hOut = input.Enthalpy - q / input.MassFlowRate;           // Enthalpía de salida

            RefrigerantState output = input.Clone();
            output.Enthalpy = hOut;
            output.Temperature = props.GetTemperatureFromPressure(operatingPressure);
            output.Entropy = props.GetEntropyFromPressure(operatingPressure);
            output.Pressure = operatingPressure;

            return output;
        }

        public bool CanConnectTo(IComponent next)
        {
            return next.Type == ComponentType.ExpansionValve;
        }
    }

}
