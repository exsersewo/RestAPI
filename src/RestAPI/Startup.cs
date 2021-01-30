using FirebirdSql.Data.FirebirdClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MySqlConnector;
using Npgsql;
using RestAPI.Clients;
using RestAPI.ExceptionFilters;
using RestAPI.Interfaces;
using RestAPI.OutputFormatters;
using System;
using System.IO;
using System.Linq;

namespace RestAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		IDatabaseClient GetDatabase()
		{
			string dataSource = Environment.GetEnvironmentVariable("DATASOURCE");

			string serverHost = Environment.GetEnvironmentVariable("HOST");
			string serverHostPort = Environment.GetEnvironmentVariable("HOSTPORT");
			string username = Environment.GetEnvironmentVariable("USERNAME");
			string password = Environment.GetEnvironmentVariable("PASSWORD");
			string database = Environment.GetEnvironmentVariable("DATABASE");

			if (!ushort.TryParse(serverHostPort, out ushort hostPort))
			{
				switch (dataSource.ToLowerInvariant())
				{
					case "mysql":
					case "mariadb":
						hostPort = 3306;
						break;
					case "mongo":
					case "mongodb":
						hostPort = 27017;
						break;
					case "firebird":
					case "interbase":
						hostPort = 3050;
						break;
					case "pgsql":
					case "postgresql":
						hostPort = 5432;
						break;
				}
			}

			switch (dataSource.ToLowerInvariant())
			{
				case "mysql":
				case "mariadb":
					return new MySqlClient(new MySqlConnectionStringBuilder
					{
						Server = serverHost,
						Port = hostPort,
						UserID = username,
						Password = password,
						Database = database,
						AllowUserVariables = true
					}.ConnectionString);
				case "mongo":
				case "mongodb":
					MongoClientSettings mongoBuilder = new();

					string prefix = "mongodb";

					if (!System.Net.IPAddress.TryParse(serverHost, out _))
					{
						prefix += "+srv";
					}

					string connString;

					if (!string.IsNullOrEmpty(username))
					{
						connString = string.Format(
							"{0}://{1}:{2}@{3}:{4}/",
							prefix,
							username,
							password,
							serverHost,
							(ushort)hostPort
						);
					}
					else
					{
						connString = string.Format(
							"{0}://{1}:{2}/",
							prefix,
							serverHost,
							(ushort)hostPort
						);
					}

					return new MongoDbClient(connString, database);
				case "firebird":
				case "interbase":
					//TODO: TEST FIREBIRD
					throw new NotImplementedException();

					return new FirebirdClient(new FbConnectionStringBuilder
					{
						DataSource = serverHost,
						Port = hostPort,
						UserID = username,
						Password = password,
						ServerType = FbServerType.Default
					}.ConnectionString);
				case "pgsql":
				case "postgresql":
					return new NpgsqlClient(new NpgsqlConnectionStringBuilder
					{
						Host = serverHost,
						Port = hostPort,
						Database = database,
						Username = username,
						Password = password
					}.ConnectionString);
				case "sqlite":
					//TODO: TEST SQLite
					throw new NotImplementedException();

					SqliteConnectionStringBuilder sqliteBuilder = new();

					var sqliteFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.sqlite", SearchOption.AllDirectories);

					if (File.Exists(database))
					{
						sqliteBuilder.DataSource = database;
					}
					else if (!database.Contains(".sqlite"))
					{
						sqliteBuilder.DataSource = sqliteFiles.FirstOrDefault(x => x.Contains($"{database}.sqlite"));
					}

					return new SQLiteClient(sqliteBuilder.ConnectionString);
			}

			return null;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			string dataSource = Environment.GetEnvironmentVariable("DATASOURCE");
			string httpsPort = Environment.GetEnvironmentVariable("HTTPSPORT");

			if (!string.IsNullOrEmpty(dataSource))
			{
				var database = GetDatabase();
				services.AddSingleton(database);
			}

			if (!string.IsNullOrEmpty(httpsPort))
			{
				throw new InvalidOperationException("HTTPS not supported yet, a work around is to use a reverse proxy");

				if (int.TryParse(httpsPort, out int port))
				{
					services.AddHsts(options =>
					{
						options.Preload = true;
						options.IncludeSubDomains = true;
						options.MaxAge = TimeSpan.FromDays(60);
					});

					services.AddHttpsRedirection(options =>
					{
						options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
						options.HttpsPort = port;
					});
				}
			}

			services.AddControllers(options =>
				{
					options.Filters.Add(new HttpResponseExceptionFilter());
					options.OutputFormatters.Add(new HtmlOutputFormatter());
				})
				.AddXmlSerializerFormatters();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			string httpsPort = Environment.GetEnvironmentVariable("HTTPSPORT");
			if (!string.IsNullOrEmpty(httpsPort))
			{
				app.UseHttpsRedirection();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
