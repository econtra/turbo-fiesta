using System;
using MathNet.Numerics;
namespace Speciale
{
    public class CashFlowTool
    {
        public CashFlowTool()
        {
        }
        public double beregnIndreSumObjekt(intensityObject intensity, int start, int slut)
        {
            double sum = 0D;

            // Beregner a_b_k
            double[] a_b_k = new double[intensity.gridpoints];
            double antalmellempunkter = intensity.gridpoints;
            for (int i = 0; i < antalmellempunkter; i++)
            {
                Math.Exp(-MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(x => intensity.rFunction(x), 0, i+1/2) * ((a_b_k(0, i) + a_b_k(0, i+1))/2)*1 ;
            }
            return 0D;
        }
    }
}
