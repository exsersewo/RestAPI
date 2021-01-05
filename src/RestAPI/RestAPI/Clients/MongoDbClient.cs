using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using RestAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Clients
{
	public class MongoDbClient : IDatabaseClient
	{
		string database;
		string connectionString;
		MongoClient connection;
		IEnumerable<string> Tables;

		public MongoDbClient(string connectionString, string database)
		{
			this.connectionString = connectionString;
			this.database = database;
			connection = new MongoClient(connectionString);
		}

		public Task CloseAsync()
		{
			connection.Cluster.Dispose();

			return Task.CompletedTask;
		}

		public async Task<bool> DoesTableExistAsync(string tableName)
		{
			if (Tables == null || !Tables.Any())
			{
				await GetTablesAsync().ConfigureAwait(false);
			}

			return Tables.Select(x => x.ToLowerInvariant()).Contains(tableName.ToLowerInvariant());
		}

		public async Task<object> GetTableAsync(string tableName, string[] blacklistedFields = null)
		{
			if (blacklistedFields == null)
			{
				blacklistedFields = new string[0];
			}

			List<List<KeyValuePair<string, string>>> content = null;

			var database = connection.GetDatabase(this.database);

			var collection = database.GetCollection<dynamic>(tableName);

			var entries = await (await collection.FindAsync(_ => true)).ToListAsync();

			if (entries.Count > 0)
			{
				content = new List<List<KeyValuePair<string, string>>>();

				foreach (dynamic entry in entries)
				{
					var res = new RouteValueDictionary(entry);

					foreach (var blacklisted in blacklistedFields)
					{
						if (res.ContainsKey(blacklisted))
						{
							res.Remove(blacklisted);
						}
					}

					List<KeyValuePair<string, string>> obj = new List<KeyValuePair<string, string>>();

					foreach (var ent in res)
					{
						obj.Add(new KeyValuePair<string, string>(ent.Key, Convert.ToString(ent.Value)));
					}

					content.Add(obj);
				}
			}

			return content;
		}

		public async Task<IEnumerable<string>> GetTablesAsync()
		{
			if (Tables == null || !Tables.Any())
			{
				var database = connection.GetDatabase(this.database);
				Tables = await (await database.ListCollectionNamesAsync()).ToListAsync();
			}

			return Tables;
		}

		public Task OpenAsync()
		{
			if (connection.Cluster.Description.State != ClusterState.Connected)
			{
				connection = new MongoClient(connectionString);
			}

			return Task.CompletedTask;
		}
	}
}
