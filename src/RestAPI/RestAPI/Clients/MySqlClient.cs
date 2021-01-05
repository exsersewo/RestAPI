using MySqlConnector;
using RestAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestAPI.Clients
{
	public class MySqlClient : IDatabaseClient
	{
		MySqlConnection connection;
		IEnumerable<string> Tables;

		public MySqlClient(string connectionString)
		{
			connection = new MySqlConnection(connectionString);
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

			MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{tableName}`", connection);

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

				MySqlCommand command = new MySqlCommand($"SHOW TABLES FROM `{connection.Database}`", connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						TableNames.Add(reader.GetString(0));
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
