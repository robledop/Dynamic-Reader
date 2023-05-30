using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Dynamic_Reader.Extensions;

namespace Dynamic_Reader.Helpers
{
	public class FileHelper
	{
		public static async Task<bool> FileExists(string path)
		{
			try
			{
				await StorageFile.GetFileFromPathAsync(path.ToWindowsFilePath());
			}
			catch (FileNotFoundException)
			{
				return false;
			}
			return true;
		}
	}
}
