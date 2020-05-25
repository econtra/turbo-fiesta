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
            double r0, // Rente tiltid 0
            List<double> r,
            List<double> mu,
            List<double> tau,
            List<double> xVal,
            double horizon = 40, //angivet i år
            double gridpoints = 1, // finheden af min simulering. 100 svarer til 100 punkter pr. år
            double W_1_start = 0, 
            double W_2_start = 0 
        )
        {
            this.mu0 = mu0;
            this.tau0 = tau0;
            this.r0 = r0;
            this.W_1_start = W_1_start;
            this.W_2_start = W_2_start;
            this.horizon = horizon;
            this.gridpoints = gridpoints;
            this.mu = new List<double> ();
            this.tau = new List<double>();
            this.r = new List<double>();
            this.xVal = new List<double>();
  
        }

        internal void exportTxt()
        {
            string path = @"/Users/ViktorJeppesen/Documents/Studie/Aktuar/Advanced/muData.txt";
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine("x-values mu-values tau-values r-values" + "\n");
                Assert.IsTrue(xVal.Count == mu.Count);
                for (int i=0; i < xVal.Count-1; i++)
                {
                    writer.WriteLine(xVal[i] + " " +mu[i]+ " "+tau[i] +" " + r[i]+ "\n");

                }
            }
            Console.WriteLine("Ny fil er nu på i advanced mappen");
        }

        public void simulateMu()
        {
            List<double> W_1 = new List<double>(); W_1.Add(W_1_start);
            List<double> W_2 = new List<double>(); W_2.Add(W_2_start);
            // Først simulerer vi W'erne helt igennem

            double a1 = 0.1; double b1 = 0.1;double sigma1 = 0.01; double a2 = 0.2; double b2 = 0.2; double sigma2 = 0.01; xVal.Add(0);
            for (int i = 1; i < horizon*gridpoints; i++) {
                xVal.Add(i / gridpoints);
                W_1.Add(normalDist.Sample());
                W_2.Add(normalDist.Sample());
            }

            // konstruerer mu
            r.Add(r0); tau.Add(tau0); mu.Add(mu0);
            for (int j = 1; j < horizon*gridpoints; j++)
            {
                tau.Add(a1 * (b1 - tau[j - 1]) * (1 / gridpoints) + sigma1 * W_1[j - 1]);
                r.Add(a2 * (b2 - r[j - 1]) * (1 / gridpoints) + sigma2 * W_2[j - 1]);
                mu.Add(0.0005 + Math.Pow(10, (5.728 + 0.038 * -10))); // Danicas kvindedødelighed

            }

        }
        // Lineær interpolation af mu hvis det vil bruges
        public double muFunction(double x)
        {
            return Interpolate1D(x, xVal, mu, 0D, 40D);
        }

        public double mu0 { get; }
        public double tau0 { get; }
        public double r0 { get; }
        public double horizon { get; }
        public double gridpoints { get; }
        List<double> r { get; }
        List<double> mu { get; }
        List<double> tau { get; }
        List<double> xVal { get; }
        public double W_1_start { get; }
        public double W_2_start { get; }


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
