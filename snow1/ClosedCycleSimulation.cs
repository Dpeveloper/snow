using snow1.Compressors;
using snow1.Condenser;
using snow1.Enviroment;
using snow1.Refrigerant;

namespace snow1
{
    public class ClosedCycleSimulation
    {
        private bool simulacionActiva = true;

        public async Task RunAsync()
        {
            // 1️⃣ Configurar refrigerante
            var refrigerantConfig = new RefrigerantConfig(
                refrigerantName: "R134a",
                initialTemperature: 278,   // 5°C en Kelvin
                evaporationTemperature: 273,  // 0°C en Kelvin
                condensationTemperature: 313,  // 40°C en Kelvin
                ambientTemperature: 303    // 30°C en Kelvin
            );

            // 2️⃣ Crear estado inicial desde configuración
            RefrigerantState currentState = refrigerantConfig.CreateInitialState();

            // 3️⃣ Crear modelo de compresión y compresor
            var compressionModel = new IsentropicCompressionModel(0.8, new RefrigerantProperties());
            var compressor = new Compressor(
                compressionRatio: refrigerantConfig.Pcond,
                compressionModel: compressionModel
            );

            // 4️⃣ Condensador usando la Pcond del refrigerante
            var condenser = new BasicCondenser(
                ambientAirTemp: refrigerantConfig.AmbientTemperature,
                heatTransferArea: 3.0,
                uValue: 0.6,
                operatingPressure: refrigerantConfig.Pcond,
                props: new RefrigerantProperties()
            );

            // 5️⃣ Válvula de expansión usando la Pevap del refrigerante
            var valve = new ThermostaticExpansionValve(new RefrigerantProperties());

            // 6️⃣ Evaporador usando la Pevap del refrigerante
            var evaporator = new BasicEvaporator(
                ambientAirTemp: refrigerantConfig.AmbientTemperature,
                heatTransferArea: 3.0,
                uValue: 0.6,
                operatingPressure: refrigerantConfig.Pevap,
                props: new RefrigerantProperties()
            );

            // 7️⃣ Ambiente (sala/frigorífico)
            var room = new ThermalEnvironment(
                initialTemperature: refrigerantConfig.AmbientTemperature,
                heatCapacity: 10000 // kJ/K
            );

            double tiempoActual = 0;
            double tiempoPaso = 1; // segundos reales

            Console.Clear();
            Console.WriteLine("== ❄️ SIMULADOR DE REFRIGERACIÓN: INICIO ==\n");

            while (simulacionActiva && room.Temperature > 289.15) // 16°C
            {
                Console.WriteLine($"\n=== ⏱️ Tiempo: {tiempoActual:F0}s ===");

                evaporator.SetAmbientTemperature(room.Temperature);

                // 🔵 1. EVAPORADOR
                var estadoEvap = evaporator.Process(currentState);
                double qAbs = (estadoEvap.Enthalpy - currentState.Enthalpy) * estadoEvap.MassFlowRate;
                room.RemoveHeat(qAbs * tiempoPaso);
                PrintComponente("EVAPORADOR", currentState, estadoEvap, qAbs, "Q Absorbido");
                currentState = estadoEvap;

                // 🔴 2. COMPRESOR
                var estadoComp = compressor.Process(currentState);
                double wComp = compressor.PowerConsumed;
                PrintComponente("COMPRESOR", currentState, estadoComp, wComp, "Trabajo eléctrico");
                currentState = estadoComp;

                // 🟡 3. CONDENSADOR
                var estadoCond = condenser.Process(currentState);
                double qRech = (currentState.Enthalpy - estadoCond.Enthalpy) * currentState.MassFlowRate;
                PrintComponente("CONDENSADOR", currentState, estadoCond, qRech, "Q Rechazado");
                currentState = estadoCond;

                // ⚪ 4. VÁLVULA DE EXPANSIÓN
                valve.SetTargetPressure(evaporator.GetPressure());
                var estadoValv = valve.Process(currentState);
                PrintComponente("VÁLVULA DE EXPANSIÓN", currentState, estadoValv, 0, "ΔP Forzada");
                currentState = estadoValv;

                // 🌡️ Ambiente
                Console.WriteLine($"\n🌍 Temperatura del ambiente: {room.Temperature - 273.15:F2} °C");
                Console.WriteLine("============================================\n");

                await Task.Delay(TimeSpan.FromSeconds(tiempoPaso));
                tiempoActual += tiempoPaso;
            }

            Console.WriteLine("✅ Simulación finalizada: Temperatura objetivo alcanzada o ciclo detenido.");
        }

        private void PrintComponente(string nombre, RefrigerantState entrada, RefrigerantState salida, double energia, string tipoEnergia)
        {
            Console.WriteLine($"\n🔧 [{nombre}]");
            Console.WriteLine($"   ▸ Presión:     {salida.Pressure:N0} Pa  (Δ {salida.Pressure - entrada.Pressure:N0})");
            Console.WriteLine($"   ▸ Temperatura: {salida.Temperature - 273.15:F2} °C (Δ {(salida.Temperature - entrada.Temperature):F2})");
            Console.WriteLine($"   ▸ Entalpía:    {salida.Enthalpy:F2} kJ/kg (Δ {(salida.Enthalpy - entrada.Enthalpy):F2})");
            Console.WriteLine($"   ▸ Entropía:    {salida.Entropy:F4} kJ/kg·K (Δ {(salida.Entropy - entrada.Entropy):F4})");
            Console.WriteLine($"   ▸ {tipoEnergia}: {energia:F2} kW");
        }
    }
}
