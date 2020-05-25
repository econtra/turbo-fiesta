using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Speciale
{
    class FinancialObject
    {
        /// <summary>
        /// Run a regression test with for a given client directory containing input, consistency
        /// and configuration data, with a particular configuration filename (or set of filenames).
        /// </summary>
        /// <param name="kappa">The contribution rate. If left null, it will be set to the rate ensuring constant consumption.</param>
        /// <param name="configFilename">The XML config data for use when making calculations.</param>
        /// <param name="performanceMode">Whether to test for changes to performance metrics</param>
        public FinancialObject(
            double? kappa,
            double gamma,
            double r = 0.05,
            double R = 60,
            double T = 100,
            double w = 1
        )
        {
            this.gamma = gamma;
            this.r = r;
            this.R = R;
            this.T = T;
            this.w = w;
            if (kappa is null)
            {
                this.kappa = (gamma * (T - R)) / (R * (1 - gamma) + gamma * T);
            }
            else
            {
                this.kappa = (double)kappa;
            }

            TimePoints = Enumerable
                .Repeat(0, 10001)
                .Select((entry, index) => entry + (0.01 * index))
                .ToList();

            Benefit = TimePoints.Select(t => TimePointsToBenefit(t)).ToList();

            Consumption = TimePoints.Select(t => TimePointsToConsumption(t)).ToList();

            Account = TimePoints.Select(t => TimePointsToAccount(t)).ToList();
        }

        private double TimePointsToConsumption(double t)
        {
            if(t <= R)
                return (1 - kappa) * w;
            return Benefit.Last(); // konstant benefit
        }

        private double TimePointsToBenefit(double t)
        {
            if (t <= R)
                return 0;
            return (R * kappa * w) / (T - R);
        }

        private double TimePointsToAccount(double t)
        {
            if (t <= R)
                return t * kappa * w;
            return TimePointsToAccount(R) - (t - R) * Benefit.Last();
        }

        public void WriteData(string path)
        {
            File.Delete(path);
            using (var writer = File.AppendText(path))
            {
                writer.WriteLine("TimePoints;" + string.Join(";", TimePoints));
                writer.WriteLine("Consumption;" + string.Join(";", Consumption));
                writer.WriteLine("Benefit;" + string.Join(";", Benefit));
                writer.WriteLine("Account;" + string.Join(";", Account));
            }
        }

        public double kappa {get; }
        public double? gamma { get; }
        public double r { get; }
        public double R { get; }
        public double T { get; }
        public double w { get; }
        public List<double> TimePoints { get; }
        public List<double> Consumption { get; }
        public List<double> Benefit { get; }
        public List<double> Account { get; }
    }
}