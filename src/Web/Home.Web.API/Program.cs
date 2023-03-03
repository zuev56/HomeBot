using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zs.Common.Extensions;

namespace Home.Web.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args).RunConsoleAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.TrySaveToFile();
            }
        }

        private static IConfiguration CreateConfiguration(string[] args)
        {
            throw new NotImplementedException();
            //if (!File.Exists(ProgramUtilites.MainConfigurationPath))
            //{
            //    throw new AppsettingsNotFoundException();
            //}
//
            //var configuration = new ConfigurationManager();
            //configuration.AddJsonFile(ProgramUtilites.MainConfigurationPath, optional: false, reloadOnChange: true);
//
            //foreach (var arg in args)
            //{
            //    if (!File.Exists(arg))
            //        throw new FileNotFoundException($"Wrong configuration path:\n{arg}");
//
            //    configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
            //}
//
            //if (configuration["SecretsPath"] != null)
            //    configuration.AddJsonFile(configuration["SecretsPath"]);
//
            ////AssertConfigurationIsCorrect(configuration);
//
            //return configuration;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) => builder.AddConfiguration(CreateConfiguration(args)))
                .ConfigureWebHostDefaults(builder => builder.UseKestrel().UseStartup<Startup>())
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                });
        }

    }
}