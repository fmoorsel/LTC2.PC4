using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Webapps.MainApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrelOverrules();
                });

    }

    public static class ConfigurePortOverruleExtension
    {
        public static IWebHostBuilder ConfigureKestrelOverrules(this IWebHostBuilder webBuilder)
        {
            var urlsOverrule = GetUrlsOverrule();

            if (urlsOverrule.Count > 0)
            {
                webBuilder.UseUrls(urlsOverrule.ToArray());
            }
            else
            {
                var appSettings = Startup.GetAppSettings();

                if (appSettings.DefaultListenUrls != null)
                {
                    webBuilder.UseUrls(appSettings.DefaultListenUrls.Split(','));
                }
            }

            return webBuilder;
        }

        private static List<string> GetUrlsOverrule()
        {
            var result = new List<string>();
            var arguments = Environment.GetCommandLineArgs();

            foreach (var parameter in arguments)
            {
                var urlsParToken = "urls:";
                if (parameter.ToLower().StartsWith(urlsParToken))
                {
                    var urls = parameter.Substring(urlsParToken.Length).Split(',').ToList();

                    return urls;
                }
            }

            return result;
        }
    }
}
