using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Speciale
{
    class Program
    {
        static void Main(string[] args)
        {
            var TimeHorizon = 100;

            var testESG = new BSESG(T:TimeHorizon);
            var testscenario = testESG.Simulate();


            //var simpletest = new FinancialObject(null, 1);
            var test = new FinancialObject2(0.3, 1, TimeHorizon, testscenario);

            //simpletest.WriteData("C:/Users/Steffen/Dropbox (Keylane)/Speciale/Data/simpletest.csv");
            test.WriteData("C:/Users/Steffen/Dropbox (Keylane)/Speciale/Data/test.csv");
        }
    }
}