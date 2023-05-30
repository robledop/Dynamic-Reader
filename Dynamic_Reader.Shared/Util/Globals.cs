using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Dynamic_Reader.Util
{
    public static class Globals
    {
        private const string BOOKS_FOLDER = "Books";
        private const string EXTRACT_FOLDER = "Extract";
        private const string DATA_FOLDER = "Data";
        private static StorageFolder _booksFolder;
        private static StorageFolder _extractFolder;
        private static StorageFolder _dataFolder;

        public static async Task<StorageFolder> GetBooksFolder()
        {
            if (_booksFolder == null)
            {
                _booksFolder = await ApplicationData.Current.LocalFolder.
                    CreateFolderAsync(BOOKS_FOLDER, CreationCollisionOption.OpenIfExists);
            }
            return _booksFolder;
        }

        public static async Task<StorageFolder> GetDataFolder()
        {
            if (_dataFolder == null)
            {
                _dataFolder = await ApplicationData.Current.LocalFolder.
                    CreateFolderAsync(DATA_FOLDER, CreationCollisionOption.OpenIfExists);
            }
            return _dataFolder;
        }

        public static async Task<StorageFolder> GetExtractFolder()
        {
            if (_extractFolder == null)
            {
                _extractFolder = await ApplicationData.Current.LocalFolder.
                    CreateFolderAsync(EXTRACT_FOLDER, CreationCollisionOption.OpenIfExists);
            }
            return _extractFolder;
        }
    }
}