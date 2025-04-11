using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Compressors
{
    // Esta clase implementa el modelo de compresión isentrópica del compresor.
    // Se utiliza para calcular cómo cambia el refrigerante durante la compresión,
    // teniendo en cuenta la eficiencia isentrópica, que describe cuánta energía real se requiere
    // en comparación con el proceso ideal (sin pérdidas).
    public class IsentropicCompressionModel : ICompressionModel
    {
        // Eficiencia isentrópica del compresor (entre 0 y 1)
        private double efficiency;

        // Constructor que inicializa la eficiencia del compresor
        // efficiency: eficiencia del proceso de compresión (ej: 0.8)
        public IsentropicCompressionModel(double isentropicEfficiency)
        {
            efficiency = isentropicEfficiency; // Guarda la eficiencia para usarla más tarde
        }

        // Método principal que calcula el nuevo estado del refrigerante después de la compresión
        // input: el estado del refrigerante antes de la compresión
        // targetPressure: la presión de salida deseada después de la compresión
        public RefrigerantState Compute(RefrigerantState input, double targetPressure)
        {
            // 1. Estima la entalpía de salida del proceso de compresión si fuera isentrópico (ideal)
            double h2s = EstimateIsentropicEnthalpy(input, targetPressure);

            // 2. Calcula la entalpía real del refrigerante después de la compresión
            // Si la eficiencia es menor que 1, la compresión será menos eficiente que el proceso ideal
            double h2 = input.Enthalpy + (h2s - input.Enthalpy) / efficiency;

            // 3. Retorna el nuevo estado del refrigerante con la nueva presión, entalpía, temperatura y entropía
            return new RefrigerantState
            {
                Pressure = targetPressure,                       // La presión final es la que se pasa al compresor
                Enthalpy = h2,                                   // La entalpía de salida calculada
                Temperature = EstimateTemperatureFromEnthalpy(h2),// Estimación de temperatura a partir de la entalpía
                Entropy = h2 / EstimateTemperatureFromEnthalpy(h2),// Estimación de entropía usando la entalpía y temperatura
                MassFlowRate = input.MassFlowRate                // El flujo másico no cambia
            };
        }

        // Método que estima la entalpía ideal de salida después de la compresión usando la relación de presiones
        // Este modelo usa la teoría de gases ideales con una relación de compresión.
        // input: estado del refrigerante antes de la compresión
        // targetPressure: presión final deseada después de la compresión
        private double EstimateIsentropicEnthalpy(RefrigerantState input, double targetPressure)
        {
            // Relación de presiones: P2 / P1
            double ratio = targetPressure / input.Pressure;

            // Se utiliza el valor de gamma (1.4 para gases ideales) para estimar el cambio de entalpía
            double gamma = 1.4;

            // Fórmula que calcula la entalpía ideal en base a la relación de compresión
            // Esta fórmula es una aproximación termodinámica común para gases ideales
            return input.Enthalpy * Math.Pow(ratio, (gamma - 1) / gamma);
        }

        // Método que estima la temperatura a partir de la entalpía usando un modelo simplificado.
        // Este es un modelo lineal, que aproxima la temperatura en función de la entalpía.
        // h: entalpía del refrigerante en kJ/kg
        private double EstimateTemperatureFromEnthalpy(double h)
        {
            // Aproximación educativa de temperatura a partir de la entalpía (simplificada)
            // Restamos 200 kJ/kg para normalizar el valor y luego convertimos a grados Kelvin
            return 273.15 + (h - 200) * 0.5;
        }
    }

}
