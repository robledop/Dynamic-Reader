using Dynamic_Reader.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Dynamic_Reader.Helpers
{
    public sealed class ZipHelper : IDisposable
    {
        private readonly ZipArchive _archive;
        private readonly StorageFolder _bookFolder;
        private bool _disposed;

        public ZipHelper(Stream stream, StorageFolder bookFolder)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (bookFolder == null) throw new ArgumentNullException("bookFolder");

            _bookFolder = bookFolder;

            try
            {
                _archive = new ZipArchive(stream, ZipArchiveMode.Read);
            }
            catch (Exception ex)
            {
                DebugReporter.ReportError(ex);

                stream.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _archive.Dispose();
                }
            }
            _disposed = true;
        }

        public async Task UnzipFile(string fileToExtract)
        {
            if (fileToExtract == null) throw new ArgumentNullException("fileToExtract");

            try
            {
                if (!_disposed)
                {
                    var zipEntryToExtract = _archive.Entries
                        .Where(z => z.FullName == fileToExtract)
                        .Select(z => z).FirstOrDefault();
                    if (zipEntryToExtract != null)
                    {
                        await ExtractFile(zipEntryToExtract);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                DebugReporter.ReportError(ex);
            }
            catch (ArgumentException ex)
            {
                var state = new Dictionary<string, string>
                {
                    {"fileToExtract", fileToExtract},
                    {"Exception type", ex.GetType().ToString()},
                    {"innerException", ex.InnerException.GetType().ToString()}
                };

            }
        }

        private async Task ExtractFile(ZipArchiveEntry entry)
        {
            if (!_disposed)
            {
                using (var fileStream = entry.Open())
                {
                    try
                    {
                        var filePath = entry.FullName;
                        var folderName = Path.GetDirectoryName(filePath);

                        if (filePath.Contains("/"))
                        {
                            await _bookFolder.CreateFolderAsync(folderName.ToWindowsFilePath(),
                                CreationCollisionOption.OpenIfExists);
                        }

                        var outputFile = await _bookFolder.CreateFileAsync(
                            filePath.ToWindowsFilePath(), CreationCollisionOption.ReplaceExisting);
                        using (var outputFileStream = await outputFile.OpenStreamForWriteAsync())
                        {
                            try
                            {
                                await fileStream.CopyToAsync(outputFileStream);
                                await outputFileStream.FlushAsync();
                            }
                            catch (Exception ex)
                            {
                                DebugReporter.ReportError(ex);

                                outputFileStream.Dispose();
                                throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugReporter.ReportError(ex);
                        fileStream.Dispose();
                        throw;
                    }

                }
            }
        }

        ~ZipHelper()
        {
            Dispose(false);
        }
    }
}