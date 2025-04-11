using snow1.Compressors;
using snow1.Condenser;
using snow1.Enviroment;
using snow1.Evaporator;
using snow1.ExpansionValve;
using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1
{
    public class ClosedCycleSimulation
    {
        private bool simulacionActiva = true;

        public async Task RunAsync()
        {
            var initialState = new RefrigerantState
            {
                Pressure = 300000,
                Temperature = 278,
                Enthalpy = 240,
                Entropy = 1.1,
                MassFlowRate = 0.05
            };

            var currentState = initialState;

            var compressionModel = new IsentropicCompressionModel(0.8);
            var compressor = new Compressor(3.0, compressionModel);

            var condenser = new BasicCondenser(
                ambientAirTemp: 303.15,
                heatTransferArea: 3.0,
                uValue: 0.6,
                operatingPressure: 900000
            );

            var valve = new ThermostaticExpansionValve();

            var evaporator = new BasicEvaporator(
                ambientAirTemp: 295,
                heatTransferArea: 3.0,
                uValue: 0.6,
                operatingPressure: 300000
            );

            var room = new ThermalEnvironment(
                initialTemperature: 303.15,  // 30°C
                heatCapacity: 10000          // kJ/K
            );

            double tiempoActual = 0;
            double tiempoPaso = 1; // segundos reales

            Console.WriteLine("== Simulación de Refrigeración en Tiempo Real ==\n");

            while (simulacionActiva && room.Temperature > 289.15) // 16°C
            {
                evaporator.SetAmbientTemperature(room.Temperature);

                // 1. Evaporador → Compresor
                currentState = evaporator.AbsorbHeat(initialState);
                double qAbs = (currentState.Enthalpy - initialState.Enthalpy) * currentState.MassFlowRate;
                room.RemoveHeat(qAbs * tiempoPaso);
                PrintState(tiempoActual, room, currentState);
                PrintRefrigerantState("Después del EVAPORADOR", currentState);

                // 2. Compresor → Condensador
                currentState = compressor.Process(currentState);
                PrintRefrigerantState("Después del COMPRESOR", currentState);

                // 3. Condensador → Válvula
                currentState = condenser.Condense(currentState);
                PrintRefrigerantState("Después del CONDENSADOR", currentState);

                // 4. Válvula → Evaporador
                currentState = valve.Expand(currentState, evaporator.GetPressure());
                PrintRefrigerantState("Después de la VÁLVULA DE EXPANSIÓN", currentState);

                // Esperar 1 segundo en tiempo real
                await Task.Delay(TimeSpan.FromSeconds(tiempoPaso));
                tiempoActual += tiempoPaso;
            }


            Console.WriteLine("\nSimulación finalizada. Temperatura objetivo alcanzada o detenida.");
        }

        private void PrintState(double tiempo, ThermalEnvironment room, RefrigerantState estadoActual, RefrigerantState estadoAnterior = null, string componente = "Ciclo")
        {
            Console.WriteLine($"[t={tiempo:F0}s] 🌡️ Ambiente: {room.Temperature - 273.15:F2} °C");

            if (estadoAnterior != null)
            {
                Console.WriteLine($"🔄 Cambios en el {componente}:");
                Console.WriteLine($"   ▸ ΔPresión:    {estadoActual.Pressure - estadoAnterior.Pressure:N0} Pa");
                Console.WriteLine($"   ▸ ΔTemperatura:{estadoActual.Temperature - estadoAnterior.Temperature:F2} K");
                Console.WriteLine($"   ▸ ΔEntalpía:   {estadoActual.Enthalpy - estadoAnterior.Enthalpy:F2} kJ/kg");
                Console.WriteLine($"   ▸ ΔEntropía:   {estadoActual.Entropy - estadoAnterior.Entropy:F4} kJ/kg·K");
            }

            Console.WriteLine($"📦 Estado actual:");
            Console.WriteLine($"   ▸ Presión:     {estadoActual.Pressure:N0} Pa");
            Console.WriteLine($"   ▸ Temperatura: {estadoActual.Temperature:F2} K");
            Console.WriteLine($"   ▸ Entalpía:    {estadoActual.Enthalpy:F2} kJ/kg");
            Console.WriteLine($"   ▸ Entropía:    {estadoActual.Entropy:F4} kJ/kg·K");
            Console.WriteLine();
            Console.WriteLine($"=========FIN {tiempo} SEGUNDO DE LA SIMULACIÓN=========");
            Console.WriteLine();
        }

        private void PrintRefrigerantState(string etapa, RefrigerantState state)
        {
            Console.WriteLine($"🔄 {etapa}");
            Console.WriteLine($"   - Presión:     {state.Pressure:N0} Pa");
            Console.WriteLine($"   - Temperatura: {state.Temperature - 273.15:F2} °C");
            Console.WriteLine($"   - Entalpía:    {state.Enthalpy:F2} kJ/kg");
            Console.WriteLine($"   - Entropía:    {state.Entropy:F4} kJ/kg·K");
            Console.WriteLine($"   - Flujo másico:{state.MassFlowRate:F3} kg/s\n");
        }


    }
}
