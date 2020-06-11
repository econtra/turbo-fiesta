using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;


namespace Speciale
{
    class Program
    {
        static void Main(string[] args)
        {
            var stateSpace = new[] { "active", "surrender", "dead" };
            var timeHorizon = 100;
            var gridPoints = 1;

            //var testESG = new BSESG(T:TimeHorizon);
            //var testscenario = testESG.Simulate();


            ////var simpletest = new FinancialObject(null, 1);
            //var test = new FinancialObject2(0.3, 1, TimeHorizon, testscenario);

            ////simpletest.WriteData("C:/Users/Steffen/Dropbox (Keylane)/Speciale/Data/simpletest.csv");
            //test.WriteData("C:/Users/Steffen/Dropbox (Keylane)/Speciale/Data/test.csv");
            var cashflowtool = new CashFlowTool();
            var intensititer = new intensityObject(0.1, 0.1, 0.1, timeHorizon, gridPoints, 0, 0, null, null, null, null);
            intensititer.simulate();
            intensititer.exportTxt();

            // PW-means //

            // TODO: Sim r and mu

            // TODO: Then calculate transition probs P^mu and forward rates

            // The E[a^circle|r and mu up till time t] cashflow

            var aCircle = new double[timeHorizon+1, timeHorizon+1];
            var ab = new double[timeHorizon+1, timeHorizon+1];
            var interestContainer = new List<double[]>(timeHorizon);
            ab[0, 0] = 0;
            double result = 0;
            for (int scenario = 0; scenario < timeHorizon; scenario++)
            {
                intensititer.simulate();

                for (int time_i = 1; time_i <= timeHorizon; time_i++)
                {

                    //CALCULATING NECESSARY CONTROLS

                    // DERIVING CONTROLS
                    double eta = 0;
                    double delta_1 = 0;
                     // double delta_0 = cashflowtool.tekniskReserve_circle(time_i, intensititer) * (intensititer.r[time_i-1] - intensititer.tekniskr(time_i));
                    // Calculating PQ
                    double Pq = cashflowtool.calculatePq(intensititer, 0, time_i);


                    // Payments
                    double b_0_circ = 1; double b_01_circ = 2; double b_02_circ = 3;
                    double b_0_dagger = 1; double b_01_dagger = 0; double b_02_dagger = 0;
                    aCircle[scenario, time_i] = cashflowtool.muProbability00(intensititer, 0, time_i, "") * b_0_circ + cashflowtool.muProbability01(intensititer, 0, time_i, "") * b_01_circ + cashflowtool.muProbability02(intensititer, 0, time_i, "") * b_02_circ;
                    ab[scenario, time_i] = cashflowtool.calculatePq(intensititer, 0, time_i) * b_0_dagger;
                    interestContainer.Add(intensititer.r); // need to hold the interest curve for later sum


                }
            }
            for (int scenario = 0; scenario < timeHorizon; scenario++)
            {
                double scenarioResult = 0;
                for (int time = 0; time < timeHorizon; time++)
                { // UNDER GÆTTER JEG HVILKEN DER ER A CIRCLE OG HVILKEN DER ER B!!!!!!
                   scenarioResult+=  Math.Exp(-MathNet.Numerics.Integration.SimpsonRule.IntegrateThreePoint(y => rFunctionInx(y, interestContainer[scenario], intensititer), 0, time + 0.5)) * (ab[0, time] + (ab[0, time + 1]) / 2);
                }

                result += scenarioResult;
            }
            Console.WriteLine("Bonus værdi", result);


        }
        public static double rFunctionInx(double x, double[] r, intensityObject intensityobject)
        {
            return intensityobject.Interpolate1D(x, intensityobject.xVal, r, 0D, intensityobject.horizon);
        }

    }

}