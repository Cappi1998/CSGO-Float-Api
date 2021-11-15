using CSGO_Float_Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace CSGO_Float_Api
{
    public class Program
    {
        public static bool UseCertificateFile = false;
        public static bool UseSSL_Certificate = true; //default true
        public static string CertificateFileName = "";
        public static string CertificatePassword = "";

        internal static readonly string ProcessFileName = Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException(nameof(ProcessFileName));
        public static string Base_Path = Path.Combine(Directory.GetCurrentDirectory());
        
        public static string LogDiretory = Path.Combine(Base_Path, "Logs/");
        public static string LogFile_Path = Path.Combine(LogDiretory, "Log.txt");
        public static string ErrorLogFile_Path = Path.Combine(LogDiretory, "Error.txt");

        public static string Database_Path = Path.Combine(Base_Path, "Database.db");
        public static string connectionString = $"Data Source={Database_Path}";

        public static Admin admin = new Admin();
        public static StatsAdminPage statsAdmin = new StatsAdminPage();
        public static void Main(string[] args)
        {
            Console.Title = "CSGO-Float-Api";
            LoadConfig();
            Directory.CreateDirectory(LogDiretory);

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Application start-up failed", ex);
                Console.Read();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (Program.UseSSL_Certificate)
                    {
                        if (Program.UseCertificateFile)
                        {
                            webBuilder.UseKestrel(options =>
                            {
                                options.Listen(IPAddress.Any, 80);
                                options.Listen(IPAddress.Any, 443, listenOptions =>
                                {
                                    listenOptions.UseHttps(Program.CertificateFileName, Program.CertificatePassword);
                                });
                            });
                        }
                        else//Automatic Generate
                        {
                            webBuilder.UseKestrel(k =>
                            {
                                var appServices = k.ApplicationServices;
                                k.Listen(
                                    IPAddress.Any, 443,
                                    o => o.UseHttps(h =>
                                    {
                                        h.UseLettuceEncrypt(appServices);
                                    }));
                            });
                        }
                    }
                    else
                    {
                        webBuilder.UseUrls("http://0.0.0.0:80");
                    }

                    webBuilder.UseStartup<Startup>();
                });

        public static void LoadConfig()
        {
            var Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            Program.UseSSL_Certificate = Configuration.GetValue<bool>("UseSSL_Certificate");
            Program.UseCertificateFile = Configuration.GetValue<bool>("UseCertificateFile");
            Program.CertificateFileName = Configuration.GetValue<string>("CertificateFileName");
            Program.CertificatePassword = Configuration.GetValue<string>("CertificatePassword");

            var admin = new Admin();
            Configuration.GetSection("Admin").Bind(admin);
            Program.admin = admin;
        }
    }
}
