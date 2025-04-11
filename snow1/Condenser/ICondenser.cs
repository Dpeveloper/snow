using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Condenser
{
    public interface ICondenser
    {
        RefrigerantState Condense(RefrigerantState input);
        double GetPressure(); // útil para el compresor
    }

}
