using FirebirdSql.Data.FirebirdClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			string dataSource = Environment.GetEnvironmentVariable("DATASOURCE");

			if (dataSource != null)
			{
				string serverHost = Environment.GetEnvironmentVariable("HOST");
				string serverHostPort = Environment.GetEnvironmentVariable("HOSTPORT");
				string username = Environment.GetEnvironmentVariable("USERNAME");
				string password = Environment.GetEnvironmentVariable("PASSWORD");
				string database = Environment.GetEnvironmentVariable("DATABASE");

				ushort hostPort = 0;

				if (!ushort.TryParse(serverHostPort, out hostPort))
				{

				}

				switch (dataSource.ToLowerInvariant())
				{
					case "mysql":
					case "mariadb":
						{
							if (hostPort == 0)
							{
								hostPort = 3306;
							}

							services.AddSingleton<IDatabaseClient>(new MySqlClient(new MySqlConnectionStringBuilder
							{
								Server = serverHost,
								Port = hostPort,
								UserID = username,
								Password = password,
								Database = database,
								AllowUserVariables = true
							}.ConnectionString));
						}
						break;
					case "mongo":
					case "mongodb":
						{
							if (hostPort == 0)
							{
								hostPort = 27017;
							}

							MongoClientSettings builder = new MongoClientSettings();

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
									hostPort
								);
							}
							else
							{
								connString = string.Format(
									"{0}://{1}:{2}/",
									prefix,
									serverHost,
									hostPort
								);
							}

							services.AddSingleton<IDatabaseClient>(new MongoDbClient(connString, database));
						}
						break;
					case "firebird":
					case "interbase":
						{
							//TODO: TEST FIREBASE
							throw new NotImplementedException();

							if (hostPort == 0)
							{
								hostPort = 3050;
							}

							services.AddSingleton<IDatabaseClient>(new FirebirdClient(new FbConnectionStringBuilder
							{
								DataSource = serverHost,
								Port = hostPort,
								UserID = username,
								Password = password,
								ServerType = FbServerType.Default
							}.ConnectionString));
						}
						break;
					case "pgsql":
					case "postgresql":
						{
							if (hostPort == 0)
							{
								hostPort = 5432;
							}

							services.AddSingleton<IDatabaseClient>(new NpgsqlClient(new NpgsqlConnectionStringBuilder
							{
								Host = serverHost,
								Port = hostPort,
								Database = database,
								Username = username,
								Password = password
							}.ConnectionString));
						}
						break;
					case "sqlite":
						{
							//TODO: TEST SQLite
							throw new NotImplementedException();

							SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();

							var sqliteFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.sqlite", SearchOption.AllDirectories);

							if (File.Exists(database))
							{
								builder.DataSource = database;
							}
							else if (!database.Contains(".sqlite"))
							{
								builder.DataSource = sqliteFiles.FirstOrDefault(x => x.Contains($"{database}.sqlite"));
							}

							services.AddSingleton<IDatabaseClient>(new SQLiteClient(builder.ConnectionString));
						}
						break;
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

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
