using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zs.Common.Exceptions;
using Zs.Common.Models;

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
                var text = $"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";
                ProgramUtilites.TrySaveFailInfo(text);
            }
        }

        private static IConfiguration CreateConfiguration(string[] args)
        {
            if (!File.Exists(ProgramUtilites.MainConfigurationPath))
                throw new AppsettingsNotFoundException();

            var configuration = new ConfigurationManager();
            configuration.AddJsonFile(ProgramUtilites.MainConfigurationPath, optional: false, reloadOnChange: true);

            foreach (var arg in args)
            {
                if (!File.Exists(arg))
                    throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

                configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
            }

            if (configuration["SecretsPath"] != null)
                configuration.AddJsonFile(configuration["SecretsPath"]);

            //AssertConfigurationIsCorrect(configuration);

            return configuration;
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
