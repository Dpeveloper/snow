using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Evaporator
{
    public class BasicEvaporator : IEvaporator
    {
        private double ambientAirTemp;   // K
        private double heatTransferArea; // m²
        private double uValue;           // Coef. global de transferencia U (kW/m²·K)
        private double operatingPressure;

        public BasicEvaporator(double ambientAirTemp, double heatTransferArea, double uValue, double operatingPressure)
        {
            this.ambientAirTemp = ambientAirTemp;
            this.heatTransferArea = heatTransferArea;
            this.uValue = uValue;
            this.operatingPressure = operatingPressure;
        }

        public RefrigerantState AbsorbHeat(RefrigerantState input)
        {
            double deltaT = ambientAirTemp - input.Temperature;
            double q = uValue * heatTransferArea * deltaT; // Q en kW (kJ/s)
            double hOut = input.Enthalpy + (q / input.MassFlowRate); // kJ/kg

            RefrigerantState output = input.Clone();
            output.Enthalpy = hOut;
            output.Temperature = EstimateTemperatureFromEnthalpy(hOut);
            output.Entropy = EstimateEntropy(output);
            output.Pressure = operatingPressure; // Presión constante

            return output;
        }

        public double GetPressure()
        {
            return operatingPressure;
        }

        private double EstimateTemperatureFromEnthalpy(double h)
        {
            // Modelo lineal (educativo)
            return 273.15 + (h - 200) * 0.5;
        }

        private double EstimateEntropy(RefrigerantState state)
        {
            return state.Enthalpy / state.Temperature;
        }
        public void SetAmbientTemperature(double newTemp)
        {
            ambientAirTemp = newTemp;
        }

    }
}
