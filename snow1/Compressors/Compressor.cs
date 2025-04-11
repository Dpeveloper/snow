using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Compressors
{
    public class Compressor
    {
        private double compressionRatio; // Ej: 3 → triplica la presión
        private ICompressionModel model;

        public double PowerConsumed { get; private set; }

        public Compressor(double compressionRatio, ICompressionModel compressionModel)
        {
            this.compressionRatio = compressionRatio;
            this.model = compressionModel;
        }

        public RefrigerantState Process(RefrigerantState input)
        {
            double targetPressure = input.Pressure * compressionRatio;

            RefrigerantState output = model.Compute(input, targetPressure);
            PowerConsumed = input.MassFlowRate * (output.Enthalpy - input.Enthalpy);
            return output;
        }
    }
}
