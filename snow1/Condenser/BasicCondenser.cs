using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Condenser
{
    public class BasicCondenser : ICondenser
    {
        private double ambientAirTemp;     // K
        private double heatTransferArea;   // m²
        private double uValue;             // kW/m²·K
        private double operatingPressure;  // Pa

        public BasicCondenser(double ambientAirTemp, double heatTransferArea, double uValue, double operatingPressure)
        {
            this.ambientAirTemp = ambientAirTemp;
            this.heatTransferArea = heatTransferArea;
            this.uValue = uValue;
            this.operatingPressure = operatingPressure;
        }

        public RefrigerantState Condense(RefrigerantState input)
        {
            double deltaT = input.Temperature - ambientAirTemp;
            double q = uValue * heatTransferArea * deltaT; // kW
            double hOut = input.Enthalpy - q / input.MassFlowRate; // kJ/kg

            RefrigerantState output = input.Clone();
            output.Enthalpy = hOut;
            output.Temperature = EstimateTemperatureFromEnthalpy(hOut);
            output.Entropy = EstimateEntropy(output);
            output.Pressure = operatingPressure;

            return output;
        }

        public double GetPressure()
        {
            return operatingPressure;
        }

        private double EstimateTemperatureFromEnthalpy(double h)
        {
            return 273.15 + (h - 200) * 0.5;
        }

        private double EstimateEntropy(RefrigerantState state)
        {
            return state.Enthalpy / state.Temperature;
        }
    }

}
