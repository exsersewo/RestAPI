using dotenv.net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
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

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
