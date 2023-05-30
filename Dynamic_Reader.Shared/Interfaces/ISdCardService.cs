using System.Collections.Generic;
using System.Threading.Tasks;
using Dynamic_Reader.Model;

namespace Dynamic_Reader.Interfaces
{
	public interface ISdCardService
	{
		Task<IEnumerable<Book>> GetData();
	}
}