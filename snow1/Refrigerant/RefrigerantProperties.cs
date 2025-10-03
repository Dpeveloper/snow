using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snow1.Refrigerant
{
    public class RefrigerantProperties
    {
        // Tabla 1D: presión (Pa) → propiedades
        private List<(double pressure, double temperature, double enthalpy, double entropy)> table1D;

        // Tabla 2D: presión, entropía → entalpía (para interpolación bidimensional)
        private List<(double pressure, double entropy, double enthalpy)> table2D;

        public RefrigerantProperties()
        {
            table1D = new List<(double pressure, double temperature, double enthalpy, double entropy)>
    {
        (100000, 247.15, 200, 0.95),
        (200000, 260.15, 230, 1.05),
        (300000, 273.15, 250, 1.10),
        (400000, 283.15, 275, 1.20),
        (500000, 293.15, 300, 1.30),
        (600000, 303.15, 320, 1.35),
        (700000, 313.15, 340, 1.38),
        (800000, 323.15, 355, 1.40),
        (900000, 333.15, 370, 1.42),
        (1000000, 343.15, 385, 1.45)
    };

            // Para simulación del compresor con P y S → h
            table2D = new List<(double pressure, double entropy, double enthalpy)>
    {
        (200000, 1.00, 220), (200000, 1.20, 250), (200000, 1.40, 280),
        (300000, 1.00, 230), (300000, 1.20, 260), (300000, 1.40, 290),
        (400000, 1.00, 240), (400000, 1.20, 270), (400000, 1.40, 300),
        (500000, 1.00, 250), (500000, 1.20, 280), (500000, 1.40, 310),
        (600000, 1.00, 260), (600000, 1.20, 290), (600000, 1.40, 320),
    };
        }

        public double GetPressureFromTemperature(double temperature)
        {
            return Interpolate1D<(double pressure, double temperature, double enthalpy, double entropy)>(
                temperature,
                t => t.temperature,
                t => t.pressure
            );
        }

        public double GetTemperatureFromPressure(double pressure)
        {
            return Interpolate1D<(double pressure, double temperature, double enthalpy, double entropy)>(
                pressure,
                t => t.pressure,
                t => t.temperature
            );
        }

        public double GetEnthalpyFromPressure(double pressure)
        {
            return Interpolate1D<(double pressure, double temperature, double enthalpy, double entropy)>(
                pressure,
                t => t.pressure,
                t => t.enthalpy
            );
        }

        public double GetEntropyFromPressure(double pressure)
        {
            return Interpolate1D<(double pressure, double temperature, double enthalpy, double entropy)>(
                pressure,
                t => t.pressure,
                t => t.entropy
            );
        }

        public double GetEnthalpyAt(double targetPressure, double targetEntropy)
        {
            var groupP = table2D
                .GroupBy(d => d.pressure)
                .OrderBy(g => Math.Abs(g.Key - targetPressure))
                .Take(2)
                .ToList();

            if (!groupP.Any())
                throw new Exception("No hay datos de entalpía disponibles en la tabla 2D.");

            // Caso con un solo grupo (no hay suficiente rango de presión para interpolar en P)
            if (groupP.Count < 2)
            {
                var singleGroup = groupP.First().ToList();
                return Interpolate1D(targetEntropy,
                    x => x.entropy,
                    x => x.enthalpy,
                    singleGroup);
            }

            // Caso normal con dos grupos de presión
            var lowerP = groupP[0].Key;
            var upperP = groupP[1].Key;

            // Subconjuntos de entropía cercanos al targetEntropy
            var subsetLow = groupP[0]
                .Where(x => Math.Abs(x.entropy - targetEntropy) <= 0.4)
                .ToList();
            if (!subsetLow.Any()) subsetLow = groupP[0].ToList(); // fallback si queda vacío

            var subsetHigh = groupP[1]
                .Where(x => Math.Abs(x.entropy - targetEntropy) <= 0.4)
                .ToList();
            if (!subsetHigh.Any()) subsetHigh = groupP[1].ToList(); // fallback si queda vacío

            // Interpolación en entropía dentro de cada presión
            var hLow = Interpolate1D(targetEntropy, x => x.entropy, x => x.enthalpy, subsetLow);
            var hHigh = Interpolate1D(targetEntropy, x => x.entropy, x => x.enthalpy, subsetHigh);

            // Interpolación final entre presiones
            return Interpolate(targetPressure, lowerP, upperP, hLow, hHigh);
        }


        private double Interpolate1D<T>(double x,
        Func<T, double> xSelector,
        Func<T, double> ySelector,
        List<T>? subset = null)
        {
            var data = subset ?? table1D.Cast<T>().ToList();

            if (!data.Any()) throw new Exception("No hay datos disponibles para interpolar.");

            var lower = data.LastOrDefault(d => xSelector(d) <= x);
            var upper = data.FirstOrDefault(d => xSelector(d) >= x);

            // Si solo hay un punto cercano, devuelve ese
            if (lower == null && upper != null) return ySelector(upper);
            if (upper == null && lower != null) return ySelector(lower);
            if (lower != null && upper != null && lower.Equals(upper)) return ySelector(lower);

            // Si hay dos puntos distintos, interpola
            double x0 = xSelector(lower), x1 = xSelector(upper);
            double y0 = ySelector(lower), y1 = ySelector(upper);

            return Interpolate(x, x0, x1, y0, y1);
        }


        private double Interpolate(double x, double x0, double x1, double y0, double y1)
        {
            if (Math.Abs(x1 - x0) < 1e-6) return y0;
            return y0 + (x - x0) / (x1 - x0) * (y1 - y0);
        }
    }
}
