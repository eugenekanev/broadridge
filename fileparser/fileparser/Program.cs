using Autofac;
using fileparser.engine;
using fileparser.engine.txt;
using fileparser.wordfrequencycaclculation;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using fileparser.utils;
using Serilog;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace fileparser
{
    class Program
    {
        private static readonly string _supportedSearchFilesPattern = "*";
        private static readonly string _maxparallelism = "maxparallelism";
        private static readonly string _txtprocessorwriterthreshold = "txtprocessorwriterthreshold";
        static async Task<int> Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

            var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            var countOfPartitions = configuration.GetValue<int>(_maxparallelism);

            Log.Debug("File Parser Utility.");
            Log.Debug($"Max Parrellism level is {countOfPartitions}.");

            try
            {

                if (args.Length != 2)
                {
                    ShowHelp();
                    return 1;
                }

                using (IContainer container = BuildContainer(args[1], configuration))
                {

                    string[] filePaths = Directory.GetFiles(args[0], _supportedSearchFilesPattern, SearchOption.AllDirectories);

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    await filePaths.ForEachAsync(countOfPartitions, async filepath => {
                        var streamProcessor = container.ResolveOptionalNamed<IFileProcessor>(Path.GetExtension(filepath));
                        if (streamProcessor != null)
                        {
                            await streamProcessor.ProcessLinesAsync(filepath);
                        }
                    }, CancellationToken.None);
                    var elapsedTime = stopWatch.Elapsed;
                    stopWatch.Stop();
                    Log.Debug($"Parsing of files and building Dictionaries time is {elapsedTime}");
                    stopWatch.Reset();

                    stopWatch.Start();
                    await container.Resolve<IWordFrequencyCalculator>().FlushAsync();
                    elapsedTime = stopWatch.Elapsed;
                    stopWatch.Stop();
                    Log.Debug($"Sorting the final Dictionary and flushing to the file time is : {elapsedTime}");

                }


                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return 1;
            }

        }

        private static void ShowHelp()
        {
            Log.Error("Wrong arguments: correct launch looks like fileparser.exe [source direcotry name] [result file name]");
        }

        public static IContainer BuildContainer(string outputFile, IConfigurationRoot configuration)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<DefaultWordFrequencyCaclulator>()
               .As<IWordFrequencyCalculator>().WithParameter("outputPath", outputFile).SingleInstance();

            containerBuilder.RegisterType<TxtFileProcessor>()
               .WithParameter("writerThreshold", configuration.GetValue<int>(_txtprocessorwriterthreshold))
               .Named<IFileProcessor>(".txt");

            containerBuilder.RegisterType<TxtFileProcessor>()
               .WithParameter("writerThreshold", configuration.GetValue<int>(_txtprocessorwriterthreshold))
               .Named<IFileProcessor>(".txt1");

            return containerBuilder.Build();
        }
    }
}
