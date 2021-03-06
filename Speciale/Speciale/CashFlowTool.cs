﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public double[] forward00(intensityObject intensityobject, int start, int slut, String method)
        {
            double[] p = new double[intensityobject.mu.Length+1];
            for (int i = 0; i < start; i++)
            {
                p[i] = 0;
            }
            // Exact, returns the E e^int(tau+mu)
            if (method == "marginal")
            {
                p[start] = Math.Exp(- intensityobject.tau[start] - intensityobject.mu[start]);
                for (int i = start + 1; i <= slut; i++)
                {
                    p[i] = zeroCuponPrice(intensityobject.a1, intensityobject.b1, intensityobject.tau, intensityobject.sigma1, start, i) * Math.Exp(- MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => intensityobject.muFunction(x), start, i));
                }
                return p;
            }
            else if (method == "simulation")
            {
                var paths = new List<double[]>();
                for (int n = 0; n < 100; n++)
                {
                    intensityObject intCopy = intensityobject;
                    intCopy.simulate("cor");
                    var path = new double[intensityobject.mu.Length + 1];
                    path[start] = Math.Exp(-intensityobject.tau[start] - intensityobject.mu[start]);
                    for (int i = start + 1; i <= slut; i++)
                    {
                        path[i] = Math.Exp(-MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => intCopy.tauFunction(x) + intCopy.muFunction(x), start, i));
                    }
                    paths.Add(path);
                }
                for (int i = 0; i <= intensityobject.mu.Length; i++)
                {
                    var total = 0.0;
                    for (int n = 0; n < 100; n++)
                    {
                        total += paths[n][i];
                    }
                    p[i] = total/100;
                }
                return p;
            }
            else
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
            double P00 = cashflowtool.muProbability00(intensititer, start, slut, "");
            return P00 * MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => intensititer.rFunction(y) * (cashflowtool.tekniskReserve_circle(y, intensititer)/ cashflowtool.tekniskReserve_dagger(y, intensititer)), start, slut);
        }

        public double[] calculateQ(intensityObject intensititer)
        {
            return euler(0, 0, intensititer);
        }

        public double tekniskReserve_circle(double time_i, intensityObject intensititer)
        {
            double b_0 = 1; double b_01 = 0.02; double b_02 = 0.02;
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => muProbability00(intensititer, time_i, y, "") * b_0 * (y>40 ? 1 : 0) + muProbability00(intensititer, time_i, y, "") * b_01 * intensititer.tauFunction(y) + muProbability00(intensititer, time_i, y, "") * b_02 * intensititer.muFunction(y), time_i, intensititer.horizon);
        }
        public double tekniskReserve_dagger(double time_i, intensityObject intensititer)
        {
            double b_0 = 1; 
            return MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => muProbability00(intensititer, time_i, y, "") * b_0 *(y > 40 ? 1 : 0), time_i, intensititer.horizon);
        }



        public double muProbability00(intensityObject intensityobject, double start, double slut, String method){
            return Math.Exp(- (MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => intensityobject.muFunction(x) + intensityobject.tauFunction(x), start, slut)));
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
            if (start == slut)
            {
                return 1;
            }
            double B = (1-Math.Exp(-a * (slut - start))) / a;
            double A = ((B - slut + start) * (a * b - 0.5 * Math.Pow(sigma, 2))) / Math.Pow(a, 2) - (Math.Pow(sigma, 2) * Math.Pow(B, 2)) / (4 * a); 
            return Math.Exp( (A  - B*r[start])); // return the zero cupon bond price (e^int(f))
        }
        static double funcQ(double x, double y, intensityObject intensiteter)
        {
            CashFlowTool tool = new CashFlowTool();
            return intensiteter.rFunction(x) * (( (tool.Vbarcircstjern(intensiteter, x) /tool.Vbardaggerstjern(intensiteter, x)  ) +y) );
        }

        private double Vbardaggerstjern(intensityObject intensiteter, double x)
        {
            CashFlowTool tool = new CashFlowTool();
            return tool.muProbability00(intensiteter, 0, x, "") * tool.tekniskReserve_dagger(x, intensiteter);
        }

        private double Vbarcircstjern(intensityObject intensiteter, double x)
        {
            CashFlowTool tool = new CashFlowTool();
            return tool.muProbability00(intensiteter, 0, x, "") * tool.tekniskReserve_circle(x, intensiteter);
        }

        static double[] euler(int x0, double y, intensityObject intensiteter)
        {
            int h = 1;
            double[] yres = new double[intensiteter.horizon+1];
            yres[0] = y;
            while (x0 < intensiteter.horizon)
            {
                y = y + h * funcQ(x0, y, intensiteter);
                x0 = x0 + h;
                yres[x0] = y;
            }

            // Printing approximation 
            return yres;
        }
    }

}