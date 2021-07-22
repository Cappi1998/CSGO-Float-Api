using CSGO_Float_Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace CSGO_Float_Api
{
    public class Program
    {
        public static bool UseCertificateFile = false;
        public static bool UseSSL_Certificate = true; //default true
        public static string CertificateFileName = "";
        public static string CertificatePassword = "";

        internal static readonly string ProcessFileName = Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException(nameof(ProcessFileName));
        public static string Database_Path = Path.Combine(Directory.GetCurrentDirectory(), "Database.db");
        public static string connectionString = $"Data Source={Database_Path}";
        public static Admin admin = new Admin();

        public static void Main(string[] args)
        {
            LoadConfig();

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
