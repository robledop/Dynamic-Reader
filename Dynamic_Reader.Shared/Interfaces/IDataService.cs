using System.Collections.Generic;
using System.Threading.Tasks;
using Dynamic_Reader.Model;

namespace Dynamic_Reader.Interfaces
{
	public interface IDataService
	{
		/// <summary>
		/// Main method to load all the books
		/// </summary>
		Task<IEnumerable<Book>> GetData();

		/// <summary>
		/// Populate all books metadata.
		/// </summary>
		/// <param name="repopulate">Force repopulation</param>
		/// <returns></returns>
		Task PopulateBooksMetadataAsync(bool repopulate);
	}
}