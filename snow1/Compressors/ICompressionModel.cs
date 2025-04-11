using snow1.Refrigerant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Compressors
{
    public interface ICompressionModel
    {
        RefrigerantState Compute(RefrigerantState input, double targetPressure);
    }
}
