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
            int horizon = 40, //angivet i år
            int gridpoints = 1, // finheden af min simulering. 100 svarer til 100 punkter pr. år
            double W_1_start = 0, 
            double W_2_start = 0,
            double[] r =null,
            double[] mu =null,
            double[] tau = null,
            double[] xVal = null
        )
        {
            this.mu0 = mu0;
            this.tau0 = tau0;
            this.r0 = r0;
            this.W_1_start = W_1_start;
            this.W_2_start = W_2_start;
            this.horizon = horizon;
            this.gridpoints = gridpoints;
            this.mu = new double[this.horizon];
            this.tau = new double[this.horizon];
            this.r = new double[this.horizon];
            this.xVal = new double[this.horizon];
  
        }

        internal void exportTxt()
        {
            string path = @"/Users/ViktorJeppesen/Documents/Studie/Aktuar/Advanced/muData.txt";
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine("x-values mu-values tau-values r-values" + "\n");
                Assert.IsTrue(xVal.Length == mu.Length);
                for (int i=0; i < xVal.Length - 1; i++)
                {
                    writer.WriteLine(xVal[i] + " " +mu[i]+ " "+tau[i] +" " + r[i]+ "\n");

                }
            }
            Console.WriteLine("Ny fil er nu på i advanced mappen");
        }

        public double a2 = 0.5/100; public double b2 = 2;public double sigma2 = 0.01; public double a1 = 1; public double b1 = 0.049; public double sigma1 = 0.01; 
        public void simulate(string method)
        {
            xVal[0] = 0;
            List<double> W_1 = new List<double>(); W_1.Add(W_1_start);
            List<double> W_2 = new List<double>(); W_2.Add(W_2_start);
            // Først simulerer vi W'erne helt igennem

            for (int i = 1; i < horizon*gridpoints; i++) {
                xVal[i] = (i / gridpoints);
                W_1.Add(normalDist.Sample());
                W_2.Add(normalDist.Sample());
            }

            // konstruerer mu
            r[0] =r0; tau[0] =tau0; mu[0] =mu0;
            for (int j = 1; j < horizon*gridpoints; j++)
            {
                if (method == "cor")
                {
                    tau[j] = (a1 * (b1 - tau[j - 1]) * (1 / gridpoints) + sigma1* 0.8 * W_1[j - 1] + sigma1 *0.2 * W_2[j - 1] ) ;
                }
                else
                {
                    tau[j] = (a1 * (b1 - tau[j - 1]) * (1 / gridpoints) + sigma1 * W_1[j - 1]);
                }
                r[j] = (a2 * (b2 - r[j - 1]) * (1 / gridpoints) + sigma2 * W_2[j - 1]);
                mu[j] = (0.0005 + Math.Pow(10, (5.728 + 0.038 *j -10))); // Danicas kvindedødelighed

            }

        }





        public double muZeroCupon(int start, int slut)
        {
            return Math.Exp(MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => muFunction(x), start, slut));
        }


        // Lineær interpolation af mu hvis det vil bruges
        public double muFunction(double x)
        {
            return Interpolate1D(x, xVal, mu, 0D, horizon);
        }
        public double rFunction(double x)
        {
            return Interpolate1D(x, xVal, r, 0D, horizon);
        }
        public double tauFunction(double x)
        {
            return Interpolate1D(x, xVal, tau, 0D, horizon);
        }
        public double tekniskr(double x)
        {
            return 0D;
        }
        public double Interpolate1D(double value, double[] x, double[] y, double lower, double upper)
        {
            for (int i = 0; i < x.Length; i++)
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
        public double mu0 { get; }
        public double tau0 { get; }
        public double r0 { get; }
        public int horizon { get; }
        public int gridpoints { get; }
        public double[] r { get; }
        public double[] mu { get; }
        public double[] tau { get; }
        public double[] xVal { get; }
        public double W_1_start { get; }
        public double W_2_start { get; }


    }

}
