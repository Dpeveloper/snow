using snow1.Refrigerant;

namespace snow1.Compressors
{
    public class IsentropicCompressionModel : ICompressionModel
    {
        private double efficiency;
        private RefrigerantProperties props;

        public IsentropicCompressionModel(double isentropicEfficiency, RefrigerantProperties props)
        {
            this.efficiency = isentropicEfficiency;
            this.props = props;
        }

        public RefrigerantState Compute(RefrigerantState input, double targetPressure)
        {
            double h2s = props.GetEnthalpyAt(targetPressure, input.Entropy);

            double h2 = input.Enthalpy + (h2s - input.Enthalpy) / efficiency;

            // 3. Crear nuevo estado con datos interpolados
            return new RefrigerantState
            {
                Pressure = targetPressure,
                Enthalpy = h2,
                Temperature = props.GetTemperatureFromPressure(targetPressure),
                Entropy = props.GetEntropyFromPressure(targetPressure),
                MassFlowRate = input.MassFlowRate
            };
        }
    }
}
