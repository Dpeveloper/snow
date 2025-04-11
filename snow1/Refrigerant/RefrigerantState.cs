using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Refrigerant
{
    public class RefrigerantState
    {
        public double Pressure;       // en Pa o bar
        public double Temperature;    // en °C o K
        public double Enthalpy;       // en kJ/kg
        public double MassFlowRate;   // en kg/s
        public double Entropy;

        public RefrigerantState Clone()
        {
            return (RefrigerantState)this.MemberwiseClone();
        }
    }

}
