using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.ExpansionValve
{
    public abstract class ExpansionValve : IExpansionDevice
    {
        public abstract RefrigerantState Expand(RefrigerantState input, double targetPressure);
    }

}
