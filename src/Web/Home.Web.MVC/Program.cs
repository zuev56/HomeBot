using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var mainConfigPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(mainConfigPath))
                    throw new Exception($"Configuration file appsettings.json is not found in the application directory: {AppDomain.CurrentDomain.BaseDirectory}");

                var appsettings = new ConfigurationBuilder().AddJsonFile(mainConfigPath, optional: false, reloadOnChange: true).Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(appsettings, "Serilog")
                    .CreateLogger();

                await CreateHostBuilder(args, appsettings).Build().RunAsync();

                Console.WriteLine("The end of Main function is reached");
            }
            catch (Exception ex)
            {
                var text = $"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";

                Console.WriteLine(text);
                TrySaveFailInfo(text);
                Log.Logger.Fatal(ex, "Home.Web.MVC fault");
                Console.Read();
            }            
        }

        private static void TrySaveFailInfo(string text)
        {
            try
            {
                string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Critical_failure_{DateTime.Now::yyyy.MM.dd HH:mm:ss.ff}.log");
                File.AppendAllText(path, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot appsettings)
        {           
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddSerilog())
                .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
                {
                    foreach (var arg in args)
                    {
                        if (!File.Exists(arg))
                            throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

                        configurationBuilder.AddJsonFile(arg, optional: true, reloadOnChange: true);
                    }

                    var tmpConfig = configurationBuilder.Build();
                    if (tmpConfig["SecretsPath"] != null)
                        configurationBuilder.AddJsonFile(tmpConfig["SecretsPath"]);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    string[] urls = appsettings.GetSection("UseUrls").Get<string[]>();
                    if (urls?.Any() == true)
                        webBuilder.UseUrls(appsettings.GetSection("UseUrls").Get<string[]>());

                    webBuilder.UseStartup<Startup>();
                });
        }
            
    }
}
