using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using ZedGraph;

namespace Speciale
{
    class FinancialObject2
    {
        /// <summary>
        /// Run a regression test with for a given client directory containing input, consistency
        /// and configuration data, with a particular configuration filename (or set of filenames).
        /// </summary>
        /// <param name="kappa">The contribution rate. If left null, it will be set to the rate ensuring constant consumption.</param>
        /// <param name="gamma"></param>
        /// <param name="alpha"></param>
        /// <param name="r"></param>
        /// <param name="R"></param>
        /// <param name="T"></param>
        /// <param name="w_0"></param>
        /// <param name="x_0"></param>
        /// <param name="numberOfTimePoints"></param>
        /// <param name="configFilename">The XML config data for use when making calculations.</param>
        /// <param name="performanceMode">Whether to test for changes to performance metrics</param>
        public FinancialObject2(
            double? kappa,
            double gamma,
            double T,
            double[] scenario,
            double alpha = 0.012,
            double r = 0.00000001,
            double R = 60,
            double w_0 = 1,
            double x_0 = 0,
            int numberOfTimePoints = 10001
        )
        {
            this.gamma = gamma;
            this.alpha = alpha;
            this.Scenario = scenario;
            this.r = r;
            this.R = R;
            this.T = T;
            this.w_0 = w_0;
            this.NumberOfTimePoints = numberOfTimePoints;
            if (kappa is null)
            {
                this.kappa =
                    (gamma * w_0 * Math.Exp(alpha * R) * (1 - Math.Exp(-r * (T - R))) / (r * Math.Exp(r * R)) - x_0) / // Regnes forkert
                    (w_0 * (Math.Exp(R * (alpha - r)) - 1) / (alpha - r) +
                     gamma * w_0 * Math.Exp(alpha * R) * (1 - Math.Exp(-r * (T - R))) / (r * Math.Exp(r * R)));
            }
            else
            {
                this.kappa = (double)kappa;
            }

            TimePoints = new double[NumberOfTimePoints];
            for (int i = 1; i < NumberOfTimePoints; i++)
            {
                TimePoints[i] = T / ((double) NumberOfTimePoints - 1) * i;
            }

            LabourMarketParticipation = new double[NumberOfTimePoints];
            for (int i = 0; i < NumberOfTimePoints; i++)
            {
                //switch (TimePoints[i])
                //{
                //    case double t when (t <= 40):
                //        LabourMarketParticipation[i] = 1;
                //        break;
                //    case double t when (40 < t && t <= R):
                //        LabourMarketParticipation[i] = 1 - (t - 40) * 1/(60 - 40);
                //        break;
                //    case double t when (R < t):
                //        LabourMarketParticipation[i] = 0;
                //        break;
                //}
                switch (TimePoints[i])
                {
                    case double t when (t <= R):
                        LabourMarketParticipation[i] = 1 - 0.5 * Math.Pow(TimePoints[i] / R, 15); ;
                        break;
                    case double t when (R < t):
                        LabourMarketParticipation[i] = 0;
                        break;
                }
            }

            Wage = new double[NumberOfTimePoints];
            Wage[0] = w_0;
            for (int i = 1; i < NumberOfTimePoints; i++)
            {
                Wage[i] = Wage[i-1] + this.alpha * Wage[i - 1] * TimePoints[1];
            }

            Income = new double[NumberOfTimePoints];
            for (int i = 0; i < NumberOfTimePoints; i++)
            {
                Income[i] = Wage[i] * LabourMarketParticipation[i];
            }

            Account = new double[NumberOfTimePoints];
            Account[0] = x_0;
            Benefit = new double[NumberOfTimePoints];
            for (int i = 1; i < NumberOfTimePoints; i++)
            {
                switch (TimePoints[i])
                {
                    case double t when (t <= R):
                        Account[i] = Account[i-1] + r * Account[i - 1] * TimePoints[1] + LabourMarketParticipation[i-1] * this.kappa * Income[i-1] * TimePoints[1];
                        break;
                    case double t when (R < t):
                        if (Benefit[i - 1] == 0)
                            Benefit[i] = r * Account[i - 1] / (1 - Math.Exp(-r * (T - R)));
                        else Benefit[i] = Benefit[i - 1];
                        Account[i] = Account[i - 1] + r * Account[i - 1] * TimePoints[1] - Benefit[i] * TimePoints[1];
                        break;
                }
            }

            Consumption = new double[NumberOfTimePoints];
            for (int i = 0; i < NumberOfTimePoints; i++)
            {
                switch (TimePoints[i])
                {
                    case double t when (t <= R):
                        Consumption[i] = Income[i] * (1 - this.kappa);
                        break;
                    case double t when (R < t):
                        Consumption[i] = Benefit[i];
                        break;

                }
            }







        }

        public void WriteData(string path)
        {
            File.Delete(path);
            using (var writer = File.AppendText(path))
            {
                writer.WriteLine("TimePoints;" + string.Join(";", TimePoints));
                writer.WriteLine("LabourMarketParticipation;" + string.Join(";", LabourMarketParticipation));
                writer.WriteLine("Wage;" + string.Join(";", Wage));
                writer.WriteLine("Income;" + string.Join(";", Income));
                writer.WriteLine("Consumption;" + string.Join(";", Consumption));
                writer.WriteLine("Account;" + string.Join(";", Account));
                writer.WriteLine("Benefit;" + string.Join(";", Benefit));
                writer.WriteLine("Scenario;" + string.Join(";", Scenario));
            }
        }

        public double kappa {get; }
        public double? gamma { get; }
        public double r { get; }
        public double R { get; }
        public double T { get; }
        public double[] Scenario { get; }
        public double w_0 { get; }
        public int NumberOfTimePoints { get; set; }
        public double[] TimePoints { get; }
        public double[] LabourMarketParticipation { get; }
        public double[] Income { get; }
        public double[] Wage { get; }
        public double[] Consumption { get; }
        public double[] Benefit { get; }
        public double[] Account { get; }
        public double alpha { get; }
    }
}