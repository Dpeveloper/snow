using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using snow1.Refrigerant;

namespace snow1.ExpansionValve
{
    public class ThermostaticExpansionValve : ExpansionValve
    {
        public override RefrigerantState Expand(RefrigerantState input, double targetPressure)
        {
            // Modelo ideal: entalpía constante, presión desciende
            RefrigerantState output = input;
            output.Pressure = targetPressure;
            output.Enthalpy = input.Enthalpy; // proceso isoentálpico ideal
            output.Temperature = EstimateSaturationTempAtPressure(targetPressure);
            output.Entropy = EstimateEntropy(output);

            return output;
        }

        private double EstimateSaturationTempAtPressure(double pressure)
        {
            // Estimación educativa: inversa logarítmica aproximada
            return 273.15 + 40 - Math.Log(pressure / 100000) * 15;
        }

        private double EstimateEntropy(RefrigerantState state)
        {
            return state.Enthalpy / state.Temperature;
        }
    }

}
