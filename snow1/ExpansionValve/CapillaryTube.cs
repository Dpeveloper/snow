using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.ExpansionValve
{
    public class CapillaryTube : ExpansionValve
    {
        public override RefrigerantState Expand(RefrigerantState input, double targetPressure)
        {
            // Aquí se modela la caída de presión con un flujo permanente
            // Se puede añadir cálculo con ecuación de Darcy-Weisbach o Hazen-Williams

            RefrigerantState output = input.Clone();
            output.Pressure = targetPressure;
            output.Enthalpy = input.Enthalpy;
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
