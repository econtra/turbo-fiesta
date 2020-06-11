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
            double[] f = new double[intensityobject.mu.Length];
            for (int i = 0; i < start; i++)
            {
                f[i] = 0;
            }
            if (method == "exact")
            {
                f[start] = intensityobject.tau0;
                for (int i = start + 1; i <= slut; i++)
                {
                    f[i] = calculateForwardRate(intensityobject.a1, intensityobject.b1, intensityobject.mu, intensityobject.sigma1, i, slut) ;
                }
                return f;
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

        public double calculatePq(intensityObject intensititer, double start, double slut)
        {
            CashFlowTool cashflowtool = new CashFlowTool();
            return cashflowtool.muProbability00(intensititer, start, slut, "") * MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => intensititer.rFunction(y) * (cashflowtool.tekniskReserve_circle(y, intensititer)/ cashflowtool.tekniskReserve_dagger(y, intensititer)), start, slut);
        }


        internal double tekniskReserve_circle(double time_i, intensityObject intensititer)
        {
            double b_0 = 1; double b_01 = 2; double b_02 = 3;
            // MANGLER AT FORRENTES
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => muProbability00(intensititer, time_i, y, "") * b_0 * (time_i>67 ? 1 : 0) + muProbability01(intensititer, time_i, y, "") * b_01 * intensititer.tauFunction(y) + muProbability02(intensititer, time_i, y, "") * b_02 * intensititer.muFunction(y), time_i, intensititer.horizon);
        }
        internal double tekniskReserve_dagger(double time_i, intensityObject intensititer)
        {
            double b_0 = 1; 
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => muProbability00(intensititer, time_i, y, "") * b_0 *(time_i > 67 ? 1 : 0), time_i, intensititer.horizon);
        }



        public double muProbability00(intensityObject intensityobject, double start, double slut, String method){
            return Math.Exp(- MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => intensityobject.muFunction(x) + intensityobject.tauFunction(x), start, slut));
        }
        public double muProbability01(intensityObject intensityobject, double start, double slut, String method)
        {
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => muProbability00(intensityobject, start, x ,"") * intensityobject.tauFunction(x), start, slut);
        }
        public double muProbability02(intensityObject intensityobject, double start, double slut, String method)
        {
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => muProbability00(intensityobject, start, x, "") * intensityobject.muFunction(x), start, slut);
        }



        private double calculateForwardRate(double a, double b, double[] mu, double sigma, int start, int slut)
        {
            double dB = Math.Exp(-a * (slut - start));
            double dA = ((dB - 1) / Math.Pow(a, 2)) * ((a * b) - (0.5 * Math.Pow(sigma, 2))) - (Math.Pow(sigma, 2) * 2 * dB * ((1 - Math.Exp(-a * (slut - start))) / a) / (4 * a));
            return (dA-mu[start]*dB) * zeroCuponPrice(a,b,mu,sigma,start,slut);
        }
        /*
        public double forwardProbability00(intensityObject intensityobject, int start, int slut, String method)
        {
            double tauterm = zeroCuponPrice(intensityobject.a1, intensityobject.b1, intensityobject.r, intensityobject.sigma2, start, slut);
            double muterm = intensityobject.muZeroCupon(start, slut);
            return muterm*tauterm;
        }
        */

        private static double zeroCuponPrice(double a, double b, double[] r, double sigma, int start, int slut)
        {
            double B = (1-Math.Exp(-a * (slut - start))) / a;
            double A = ((B - slut + start) * (a * b - 0.5 * Math.Pow(sigma, 2))) / Math.Pow(a, 2) - (Math.Pow(sigma, 2) * Math.Pow(B, 2)) / (4 * a); 
            return Math.Exp(A  + B*r[start]); // return the zero cupon bond price (e^int(f))
        }
    }

}