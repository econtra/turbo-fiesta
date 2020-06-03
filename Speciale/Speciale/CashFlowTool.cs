using System;
using MathNet.Numerics;
namespace Speciale
{
    public class CashFlowTool
    {
        public CashFlowTool()
        {
        }
        public double[] forwardMortality(intensityObject intensityobject, int start, String method)
        {
            return intensityobject.mu;
        }
        public double[] forwardInterestRate(intensityObject intensityobject, int start, int slut, String method)
        {
            double[] p = new double[intensityobject.mu.Length];
            for(int i=0; i<start; i++)
            {
                p[i] = 0;
            }
            // Exact, returns the e^int(f)
            if (method == "exact")
            {
                p[start] = Math.Exp(intensityobject.r[start]);
                for (int i = start+1; i <= slut; i++)
                {
                    p[i] = zeroCuponPrice(intensityobject.a2, intensityobject.b2, intensityobject.r, intensityobject.sigma2, i, slut);
                }
                return p;
            } else if (method == "simulation")
            {
                throw new NotImplementedException("method not implemented yet");
            } else
            {
                throw new NotImplementedException("method not implemented yet");
            }
        }
        public double[] forwardSurrenderRate(intensityObject intensityobject, int start, int slut, String method)
        {
            // Exact , this is m
            double[] D = new double[intensityobject.mu.Length];
            for (int i = 0; i < start; i++)
            {
                D[i] = 0;
            }
            if (method == "exact")
            {
                D[start] = Math.Exp(intensityobject.tau[start])*intensityobject.tau[start];
                for (int i = start + 1; i <= slut; i++)
                {
                    D[i] = densityPrice(intensityobject.a1, intensityobject.b1, intensityobject.mu, intensityobject.sigma1, i, slut);
                }
                return D;
            }
            else if (method == "simulation")
            {
                throw new NotImplementedException("method not implemented yet");
            }
            else
            {
                throw new NotImplementedException("method not implemented yet");
            }
        }

        private double densityPrice(double a, double b, double[] mu, double sigma, int start, int slut)
        {
            double dB = Math.Exp(-a * (slut - start));
            double dA = ((dB - 1) / Math.Pow(a, 2)) * ((a * b) - (0.5 * Math.Pow(sigma, 2))) - (Math.Pow(sigma, 2) * 2 * dB * ((1 - Math.Exp(-a * (slut - start))) / a) / (4 * a));
            return dA-mu[start]*dB;
        }

        public double forwardProbability11(intensityObject intensityobject, int start, int slut, String method)
        {
            double tauterm = zeroCuponPrice(intensityobject.a1, intensityobject.b1, intensityobject.r, intensityobject.sigma2, start, slut);
            double muterm = intensityobject.muZeroCupon(start, slut);
            return muterm*tauterm;
        }


        private static double zeroCuponPrice(double a, double b, double[] r, double sigma, int start, int slut)
        {
            double B = (1-Math.Exp(-a * (slut - start))) / a;
            double A = ((B - slut + start) * (a * b - 0.5 * Math.Pow(sigma, 2))) / Math.Pow(a, 2) - (Math.Pow(sigma, 2) * Math.Pow(B, 2)) / (4 * a); 
            return Math.Exp(A  + B*r[start]); // return the zero cupon bond price (e^int(f))
        }
    }

}