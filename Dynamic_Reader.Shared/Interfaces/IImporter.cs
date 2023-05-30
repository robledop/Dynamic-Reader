using System.Threading.Tasks;

namespace Dynamic_Reader.Interfaces
{
	public interface IImporter
	{
		Task ImportBooksAsync();
		Task ContinueImportBooksAsync(object files);
	}
}