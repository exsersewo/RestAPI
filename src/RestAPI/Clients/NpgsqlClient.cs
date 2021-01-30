using Npgsql;
using RestAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Clients
{
	public class NpgsqlClient : IDatabaseClient
	{
		NpgsqlConnection connection;
		IEnumerable<string> Tables;

		public NpgsqlClient(string connectionString)
		{
			connection = new NpgsqlConnection(connectionString);
		}

		public async Task CloseAsync()
		{
			if (connection.State != System.Data.ConnectionState.Closed)
			{
				await connection.CloseAsync();
			}
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

			NpgsqlCommand cmd = new NpgsqlCommand($"SELECT * FROM {tableName}", connection);

			var reader = await cmd.ExecuteReaderAsync();

			if (reader.HasRows)
			{
				content = new List<List<KeyValuePair<string, string>>>();

				while (await reader.ReadAsync())
				{
					var colSchema = await reader.GetColumnSchemaAsync();

					List<KeyValuePair<string, string>> obj = new List<KeyValuePair<string, string>>();

					for (int col = 0; col < colSchema.Count; col++)
					{
						if (!blacklistedFields.Contains(colSchema[col].ColumnName.ToLowerInvariant()))
						{
							obj.Add(
								new KeyValuePair<string, string>(
									colSchema[col].ColumnName,
									Convert.ToString(reader[col])
								)
							);
						}
					}

					content.Add(obj);
				}
			}

			await reader.CloseAsync();

			return content;
		}

		public async Task<IEnumerable<string>> GetTablesAsync()
		{
			if (Tables == null || !Tables.Any())
			{
				List<string> TableNames = new List<string>();

				NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM information_schema.tables", connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					if (reader.HasRows)
					{
						while (await reader.ReadAsync())
						{
							TableNames.Add(Convert.ToString($"{reader["table_schema"]}.{reader["table_name"]}"));
						}
					}

					await reader.CloseAsync();
				}

				Tables = TableNames;
			}

			return Tables;
		}

		public async Task OpenAsync()
		{
			if (connection.State != System.Data.ConnectionState.Open)
			{
				await connection.OpenAsync();
			}
		}
	}
}
