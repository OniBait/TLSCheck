using CommandLine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Console = Colorful.Console;

namespace TLSCheck
{
    public class Program
    {
        private static readonly List<string> AllVersions = new List<string>
        {
            "1.0",
            "1.1",
            "2.0",
            "3.0",
            "3.5",
            "4.0",
            "4.5",
            "4.5.1",
            "4.5.2",
            "4.6",
            "4.6.1",
            "4.6.2",
            "4.7",
            "4.7.1",
            "4.7.2"
        };

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        private static void Run(Options options)
        {
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(int).Assembly.Location).ProductVersion;
            Console.WriteLineFormatted("Current .NET Runtime Version: {0}", version, Color.Goldenrod, Color.White);
            Console.WriteLineFormatted("Application .NET Target Framework: {0}\n", AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Color.Goldenrod, Color.White);

            var versions = new HashSet<string>(options.TargetFrameworks, StringComparer.InvariantCultureIgnoreCase);
            if (versions.Contains("all"))
            {
                Console.WriteLineFormatted("Testing {0} .NET Framework versions.\n", "All", Color.Goldenrod, Color.White);
                versions.Remove("all");
                foreach (var v in AllVersions) versions.Add(v);
            }

            var workers = versions.Select(v => new { Version = v, Worker = CreateWorker(v) }).Where(w => w.Worker != null);

            foreach (var worker in workers)
            {
                try
                {
                    worker.Worker.Run(options);
                }
                catch (Exception ex)
                {
                    Console.WriteLineFormatted("Exception occurred: {0}", ex, Color.Red, Color.White);
                }
                Console.WriteLine();
            }
        }

        private static Worker CreateWorker(string version)
        {
            try
            {
                if (version == null) return new Worker();

                if (Version.TryParse(version, out _))
                {
                    version = $".NETFramework,Version=v{version}";
                }

                var appsetup = new AppDomainSetup {TargetFrameworkName = version};
                var appdomain = AppDomain.CreateDomain("NewDomain", null, appsetup);

                return (Worker) appdomain.CreateInstanceAndUnwrap(typeof(Worker).Assembly.FullName, typeof(Worker).FullName ?? nameof(Worker));
            }
            catch(Exception ex)
            {
                Console.WriteLineFormatted("Could not create worker for framework: {0}. Exception: {1}", version, ex.Message, Color.Red, Color.White);
                return null;
            }
        }
    }
}
