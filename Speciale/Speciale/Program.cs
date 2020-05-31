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
            var stateSpace = new [] {"active", "surrender", "dead"};
            var timeHorizon = 100;
            var gridPoints = 100;

            //var testESG = new BSESG(T:TimeHorizon);
            //var testscenario = testESG.Simulate();


            ////var simpletest = new FinancialObject(null, 1);
            //var test = new FinancialObject2(0.3, 1, TimeHorizon, testscenario);

            ////simpletest.WriteData("C:/Users/Steffen/Dropbox (Keylane)/Speciale/Data/simpletest.csv");
            //test.WriteData("C:/Users/Steffen/Dropbox (Keylane)/Speciale/Data/test.csv");

            var intensititer = new intensityObject(0.1, 0.1, 0.1, null, null, null, null, 40, 1, 0, 0);
            intensititer.simulateMu();
            intensititer.exportTxt();

            // PW-means //

            // TODO: Sim r and mu

            // TODO: Then calculate transition probs P^mu and forward rates

            // The E[a^circle|r and mu up till time t] cashflow

            var aCircle = new double[timeHorizon, timeHorizon];
            for (int t = 0; t < timeHorizon; t ++)
            {
                for (int s = t; s < timeHorizon; s ++)
                {
                    var result = 0;
                    for (int i = 0; i < stateSpace.Length; i++)
                    {
                        var temp = 0;
                        for (int j = 0; j < stateSpace.Length; j++)
                        {
                            var tempInner = b_sojourn[j,s];
                            for (int k = 0; k < stateSpace.Length & k != j; k++)
                            {
                                tempInner += b_transition[j, k, s] * m[j, k, s, t];
                            }
                            temp += forwardProb(fromTime: t, toTime: s, fromState: i, toState: j) * tempInner;
                        }
                        result += transitionProb(fromTime: 0, toTime: t, fromState: 0, toState: i) * temp;
                    }
                    aCircle[t, s] = result;
                }
            }

            
        }
    }
}