using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TodoApi
{
    public class Program
    {
        public static string SecurityKey = "1234567812345678";
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.UseStartup<Startup>().UseUrls("http://192.168.1.61:5000");
                    webBuilder.UseStartup<Startup>().UseUrls("http://localhost:5000");
                });
    }
}
