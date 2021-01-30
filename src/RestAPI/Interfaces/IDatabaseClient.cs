using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestAPI.Interfaces
{
	public interface IDatabaseClient
	{
		public Task OpenAsync();

		public Task CloseAsync();

		public Task<object> GetTableAsync(string tableName, string[] blacklistedFields = null);

		public Task<bool> DoesTableExistAsync(string tableName);

		public Task<IEnumerable<string>> GetTablesAsync();
	}
}
