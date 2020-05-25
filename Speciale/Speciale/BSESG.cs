using System;
using System.Security.Cryptography.X509Certificates;

namespace Speciale
{
    internal class BSESG
    {
        public BSESG(double T, double mu = 0.02, double sigma = 0.04, double x_0 = 1, int numberOfTimePoints = 10001)
        {
            Mu = mu;
            Sigma = sigma;
            X_0 = x_0;
            NumberOfTimePoints = numberOfTimePoints;
            TimePoints = new double[NumberOfTimePoints];
            for (int i = 1; i < NumberOfTimePoints; i++)
            {
                TimePoints[i] = T / ((double)NumberOfTimePoints - 1) * i;
            }
        }

        public double[] Simulate()
        {
            var path = new double[NumberOfTimePoints];
            Random rand = new Random(); //reuse this if you are generating many

            path[0] = X_0;
            for (int i = 1; i < NumberOfTimePoints; i++)
            {
                double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                       Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal =
                    0 + 1 * randStdNormal; //random normal(mean,stdDev^2)

                path[i] = path[i - 1] + path[i - 1] * (Mu * TimePoints[1] + TimePoints[1] * randNormal);
            }
            return path;
        }

        public double Mu { get; }
        public double Sigma { get; }
        public double X_0 { get; }
        public int NumberOfTimePoints { get; }
        public double[] TimePoints { get; }
    }
}