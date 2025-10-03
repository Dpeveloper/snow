using System;

namespace snow1.Refrigerant
{
    public class RefrigerantConfig
    {
        public string RefrigerantName { get; private set; }
        public double InitialTemperature { get; private set; } // K
        public double EvaporationTemperature { get; private set; } // K
        public double CondensationTemperature { get; private set; } // K
        public double AmbientTemperature { get; private set; } // K
        public double Pevap { get; private set; }
        public double Pcond { get; private set; }

        private RefrigerantProperties props = new();

        public RefrigerantConfig(
            string refrigerantName,
            double initialTemperature,
            double evaporationTemperature,
            double condensationTemperature,
            double ambientTemperature)
        {
            RefrigerantName = refrigerantName;
            InitialTemperature = initialTemperature;
            EvaporationTemperature = evaporationTemperature;
            CondensationTemperature = condensationTemperature;
            AmbientTemperature = ambientTemperature;

            props = new RefrigerantProperties(); // más adelante puede inyectarse uno específico
            ValidateInputs();

            // Calcular presiones de evaporación y condensación a partir de temperaturas
            Pevap = props.GetPressureFromTemperature(EvaporationTemperature);
            Pcond = props.GetPressureFromTemperature(CondensationTemperature);
        }

        private void ValidateInputs()
        {
            if (EvaporationTemperature >= CondensationTemperature)
                throw new ArgumentException("La temperatura de evaporación debe ser menor que la de condensación.");

            if (InitialTemperature < EvaporationTemperature)
                throw new ArgumentException("La temperatura inicial no puede ser menor que la de evaporación.");

            if (EvaporationTemperature < 200 || CondensationTemperature > 400)
                throw new ArgumentOutOfRangeException("Las temperaturas deben estar en un rango físico realista (200K - 400K).");

            if (AmbientTemperature < 200 || AmbientTemperature > 330)
                throw new ArgumentOutOfRangeException("La temperatura ambiente debe estar entre 200K y 330K.");
        }

        public RefrigerantState CreateInitialState(double massFlowRate = 0.05)
        {
            return new RefrigerantState
            {
                Temperature = InitialTemperature,
                Pressure = Pevap,
                Enthalpy = props.GetEnthalpyFromPressure(Pevap),
                Entropy = props.GetEntropyFromPressure(Pevap),
                MassFlowRate = massFlowRate
            };
        }
    }
}
