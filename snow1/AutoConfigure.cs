// Archivo: AutoConfigurator.cs
// Ajusta estos using según la ubicación real de tus clases:
using snow1.Compressors;
using snow1.Condenser;
using snow1.Refrigerant;
using System;
// Si tus clases están en namespaces concretos, reemplaza por los correctos.

namespace snow1.Configuration
{
    public class CycleConfigurationResult
    {
        public IsentropicCompressionModel CompressionModel { get; set; }
        public Compressor Compressor { get; set; }
        public BasicEvaporator Evaporator { get; set; }
        public BasicCondenser Condenser { get; set; }
        public ThermostaticExpansionValve Valve { get; set; }

        public RefrigerantState InitialEvaporatorExitState { get; set; } // estado 1 (entrada al compresor)
        public RefrigerantState AfterCompressorState { get; set; }       // estado 2
        public double MassFlow { get; set; }    // kg/s
        public double Qevap { get; set; }       // kW
        public double Wcomp { get; set; }       // kW
        public double Qcond { get; set; }       // kW
        public double COP { get; set; }
        public double Pevap { get; set; }
        public double Pcond { get; set; }
        public double Tevap { get; set; }
        public double Tcond { get; set; }
        public double EvaporatorArea { get; set; }
        public double CondenserArea { get; set; }
    }

    public static class AutoConfigurator
    {
        /// Configura un ciclo básico (evap, comp, cond, valvula) capaz de entregar Qload_kW.
        /// Todas las temperaturas en grados C; internamente se usan Kelvin.

        public static CycleConfigurationResult ConfigureCycleForLoad(
            RefrigerantProperties props,
            double Qload_kW,
            double productTargetTempC,
            double roomTempC,
            double externalAmbientTempC,
            // parámetros opcionales con valores razonables
            double isentropicEfficiency = 0.8,
            double superheatK = 2.0,
            double condApproachK = 8.0,
            double U_evap = 0.6,    // kW / (m2·K)
            double U_cond = 0.6     // kW / (m2·K)
            )
        {
            if (Qload_kW <= 0) throw new ArgumentException("Qload_kW debe ser > 0");

            // --- 1) convertir temperaturas a Kelvin
            double TprodK = productTargetTempC + 273.15;
            double TroomK = roomTempC + 273.15;
            double TambK = externalAmbientTempC + 273.15;

            // --- 2) elegir Tevap y Tcond (supuestos de diseño)
            double TevapK = TprodK - superheatK;        // evaporador: algo por debajo del producto
            double TcondK = TambK + condApproachK;     // condensador: por encima del ambiente

            // --- 3) obtener presiones saturación a esas temperaturas
            double Pevap = props.GetPressureFromTemperature(TevapK);
            double Pcond = props.GetPressureFromTemperature(TcondK);

            // --- 4) obtener entalpías representativas (simplificado)
            double h_liq_cond = props.GetEnthalpyFromPressure(Pcond); // aproximación líquido a Pcond
            double h_vap_evap = props.GetEnthalpyFromPressure(Pevap); // aproximación vapor a Pevap

            double effect_kJperKg = h_vap_evap - h_liq_cond;
            if (effect_kJperKg <= 0) throw new InvalidOperationException("Efecto frigorífico no positivo. Revisar temperaturas/propiedades.");

            // --- 5) calcular flujo másico requerido (kg/s)
            double mdot = Qload_kW / effect_kJperKg;

            // --- 6) crear estado 1 (salida del evaporador / entrada al compresor)
            var state1 = new RefrigerantState
            {
                Pressure = Pevap,
                Temperature = props.GetTemperatureFromPressure(Pevap),
                Enthalpy = h_vap_evap,
                Entropy = props.GetEntropyFromPressure(Pevap),
                MassFlowRate = mdot
            };

            // --- 7) configurar compresor (modelo isentrópico con props)
            double compressionRatio = Pcond / Pevap;
            var model = new IsentropicCompressionModel(isentropicEfficiency, props);
            var compressor = new Compressor(compressionRatio, model);

            // Procesar compresión
            var state2 = compressor.Process(state1);   // estado después del compresor
            double Wcomp_kW = compressor.PowerConsumed; // kW (basado en kJ/kg * kg/s)

            // --- 8) capacidad frigorífica y calor rechazado
            double Qevap_kW = mdot * effect_kJperKg;   // kW
            double Qcond_kW = Qevap_kW + Wcomp_kW;     // kW (energía a rechazar en condensador)

            // --- 9) dimensionar áreas (A = Q / (U * ΔT)) con defensas
            // Evaporador: ΔT_evap = Troom - Tevap (no puede ser < 1K)
            double deltaT_evap = Math.Max(1.0, TroomK - TevapK);
            double A_evap = Qevap_kW / (U_evap * deltaT_evap);

            // Condensador: usar T_comp_out - TambK (temperatura del refrigerante después del compresor)
            double tempCompOut = state2.Temperature;
            double deltaT_cond = Math.Max(2.0, tempCompOut - TambK); // mínimo 2 K para evitar división por cero
            double A_cond = Qcond_kW / (U_cond * deltaT_cond);

            // --- 10) crear componentes con los valores calculados
            var evaporator = new BasicEvaporator(
                ambientAirTemp: TroomK,
                heatTransferArea: A_evap,
                uValue: U_evap,
                operatingPressure: Pevap,
                props: props);

            var condenser = new BasicCondenser(
                ambientAirTemp: TambK,
                heatTransferArea: A_cond,
                uValue: U_cond,
                operatingPressure: Pcond,
                props: props);

            var valve = new ThermostaticExpansionValve(props);
            valve.SetTargetPressure(Pevap);

            // --- 11) calcular COP y preparar resultado
            double cop = (Qevap_kW) / Math.Max(1e-9, Wcomp_kW);

            var result = new CycleConfigurationResult
            {
                CompressionModel = model,
                Compressor = compressor,
                Evaporator = evaporator,
                Condenser = condenser,
                Valve = valve,
                InitialEvaporatorExitState = state1,
                AfterCompressorState = state2,
                MassFlow = mdot,
                Qevap = Qevap_kW,
                Wcomp = Wcomp_kW,
                Qcond = Qcond_kW,
                COP = cop,
                Pevap = Pevap,
                Pcond = Pcond,
                Tevap = TevapK,
                Tcond = TcondK,
                EvaporatorArea = A_evap,
                CondenserArea = A_cond
            };

            return result;
        }
    }
}
