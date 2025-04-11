using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Evaporator
{
    public interface IEvaporator
    {
        RefrigerantState AbsorbHeat(RefrigerantState input);
        double GetPressure(); // Retorna la presión de operación para usarla en la válvula
    }

}
