using dotenv.net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;

namespace RestAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string envFile = Path.Combine(AppContext.BaseDirectory, ".env");

			DotEnv.Config(false, filePath: envFile);

			CreateHostBuilder(args).Build().Run();
		}

		public static string[] GetUrls(IWebHostBuilder webBuilder)
		{
			List<string> urls = new()
			{
				"http://0.0.0.0:80"
			};

			var httpsPort = Environment.GetEnvironmentVariable("HTTPSPORT");

			if (!string.IsNullOrEmpty(httpsPort))
			{
				var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
				if (!string.IsNullOrEmpty(environment) && !environment.ToLowerInvariant().StartsWith("dev"))
				{
					throw new InvalidOperationException("HTTPS not supported yet, a work around is to use a reverse proxy");
				}

				if (int.TryParse(httpsPort, out int port))
				{
					urls.Add($"https://0.0.0.0:{port}");
				}
			}

			return urls.ToArray();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>().UseUrls(GetUrls(webBuilder));
				});
	}
}
