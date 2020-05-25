using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Speciale
{
    public class intensityObject
    {
        MathNet.Numerics.Distributions.Normal normalDist = new Normal(0D, 1D);

        public intensityObject(
            double mu0, // dødsintensitet til tid 0 
            double tau0, // surrender intensitet til tid 0
            List<double> mu,
            List<double> xVal,
            double horizon = 40, //angivet i år
            double gridpoints = 100, // finheden af min simulering. 100 svarer til 100 punkter pr. år
            double X_1_start = 0.001, //  tilhører mu
            double X_2_start = 0.001 // tilhører også mu
        )
        {
            this.mu0 = mu0;
            this.tau0 = tau0;
            this.X_1_start = X_1_start;
            this.X_2_start = X_2_start;
            this.horizon = horizon;
            this.gridpoints = gridpoints;
            this.mu = new List<double> ();
            this.xVal = new List<double>();
  
        }

        internal void exportMuToExcept()
        {
            string path = @"/Users/ViktorJeppesen/Documents/Studie/Aktuar/Advanced/muData.txt";
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine("x-values mu-values" + "\n");
                Assert.IsTrue(xVal.Count == mu.Count);
                for (int i=0; i < xVal.Count-1; i++)
                {
                    writer.WriteLine(xVal[i] + " " +mu[i] + "\n");

                }
            }
            Console.WriteLine("Ny fil er nu på i advanced mappen");
            Console.WriteLine("test");
        }

        public void simulateMu()
        {
            List<double> X_1 = new List<double>(); X_1.Add(X_1_start);
            List<double> X_2 = new List<double>(); X_2.Add(X_2_start);
            // Først simulerer vi X'erne helt igennem

            //Det her er X parametre, vi kan overveje om de skal gives med ind som parametre
            double alpha1 = -0.5; double alpha2 = -0.2;double sigma1 = 0.0001;double sigma2 = 0.0001;double c1 = 1.08; double x1Extension = -100;
            for (int i = 1; i < horizon*gridpoints; i++) {
                X_1.Add(-alpha1 * X_1[i-1] * (1 / gridpoints) + sigma1 * normalDist.Sample());
                X_2.Add(-alpha2 * X_2[i-1] * (1 / gridpoints) + sigma2 * normalDist.Sample());
            }

            // konstruerer mu
            for (int j = 0; j < horizon*gridpoints; j++)
            {
                xVal.Add(j/gridpoints);
                mu.Add(X_1[j] + X_2[j] + Math.Pow(c1, x1Extension + xVal[j]) );
            }

        }
        // Lineær interpolation af mu hvis det vil bruges
        public double muFunction(double x)
        {
            return Interpolate1D(x, xVal, mu, 0D, 40D);
        }

        public double mu0 { get; }
        public double tau0 { get; }
        public double horizon { get; }
        public double gridpoints { get; }
        List<double> mu { get; }
        List<double> xVal { get; }
        public double X_1_start { get; }
        public double X_2_start { get; }


        public static double Interpolate1D(double value, List<double> x, List<double> y, double lower, double upper)
        {
            for (int i = 0; i < x.Count; i++)
            {
                if (value < x[i])
                {
                    if (i == 0)
                        return lower;

                    int start = i - 1;
                    int next = i;

                    double m = (value - x[start]) / (x[next] - x[start]);
                    return y[start] + (y[next] - y[start]) * m;
                }
            }

            return upper;
        }

    }

}
