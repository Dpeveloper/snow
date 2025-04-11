using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Enviroment
{
    public class ThermalEnvironment
    {
        public double Temperature { get; private set; }  // Temperatura del ambiente (K)
        public double HeatCapacity { get; }              // Capacidad térmica del ambiente (kJ/K)

        public ThermalEnvironment(double initialTemperature, double heatCapacity)
        {
            Temperature = initialTemperature;
            HeatCapacity = heatCapacity;
        }

        // Método que reduce la temperatura en función del calor extraído
        public void RemoveHeat(double heatExtracted) // kJ
        {
            // Q = C * ΔT  => ΔT = Q / C
            double deltaT = heatExtracted / HeatCapacity;
            Temperature -= deltaT;
        }

        // Opcional: añadir calor externo si quieres simular ganancias térmicas
        public void AddHeat(double heatAdded)
        {
            Temperature += heatAdded / HeatCapacity;
        }
    }

}
