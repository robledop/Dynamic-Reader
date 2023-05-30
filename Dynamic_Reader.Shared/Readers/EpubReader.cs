using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Dynamic_Reader.Common;
using Dynamic_Reader.Extensions;
using Dynamic_Reader.Helpers;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Model;
using Dynamic_Reader.Util;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;

namespace Dynamic_Reader.Readers
{
    [DebuggerDisplay("Book: {CurrentBook}")]
    public class EpubReader : BookReader
    {
        private const string IDPF_ORG2007_OPF_NAME_SPACE = "{http://www.idpf.org/2007/opf}";
        private const string CONTAINER_XML_NAME_SPACE = "{urn:oasis:names:tc:opendocument:xmlns:container}";
        private const string PURL_ORG_ELEMENTS_NAME_SPACE = "{http://purl.org/dc/elements/1.1/}";
        private const string CONTAINER_XML_FILE = "container.xml";
        private const string META_INF_DIRECTORY = "META-INF";
        private const string MIMETYPE_FILE = "mimetype";
        private const string MIMETYPE = "application/epub+zip";
        private const string APPLICATION_OEBPS_PACKAGE = "application/oebps-package+xml";



        private Stream _bookFileStream;
        private StorageFolder _bookFolder;
        private StorageFolder _extractFolder;
        private string _rootFolder = "";
        private ZipHelper _zipHelper;

        public XElement Package { get; private set; }
        public XElement Manifest { get; private set; }
        public XElement Spine { get; private set; }
        private XElement Metadata { get; set; }
        private Dictionary<string, string> ManifestItems { get; set; }

        public EpubReader(Book book)
        {
            if (book == null) throw new ArgumentNullException("book");

            //_dataService = ServiceLocator.Current.GetInstance<IDataService>();

            CurrentBook = book;
            ManifestItems = new Dictionary<string, string>();
            Timer = new Timer(Timer_Tick, SynchronizationContext.Current, -1, Timeout.Infinite);

        }

        private async void Timer_Tick(object state)
        {
            try
            {
                DispatcherHelper.CheckBeginInvokeOnUI(async () =>
                {
                    FillScreen();
                    await CheckForEndOfChapter();

                    if (CurrentText == null)
                    {
                        return;
                    }

                    if (SettingsHelper.Settings.PauseAtTheEndOfPhrase && IsEndOfFrase(CurrentText))
                    {
                        ResumeReading(SettingsHelper.Settings.PauseAtTheEndOfFraseDuration);
                    }
                    else if (SettingsHelper.Settings.PauseAfterPunctuation && EndWithPunctuation(CurrentText))
                    {
                        ResumeReading(SettingsHelper.Settings.PauseAfterPunctuationDuration);
                    }
                    else if (SettingsHelper.Settings.PauseAfterLongWords && CurrentText.Length > 8)
                    {
                        ResumeReading(SettingsHelper.Settings.PauseAfterLongWordsDuration);
                    }
                    else
                    {
                        Timer.Change(TimerInterval, Timeout.Infinite);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task<bool> CheckForEndOfChapter(bool continueReading = true)
        {
            if (TextToRead == null)
            {
                StopReading();
                return true;
            }

            if (CurrentBook.PositionInChapter >= TextToRead.Length)
            {
                StopReading();
                CurrentBook.Chapter++;
                if (CurrentBook.Chapter < ChapterFiles.Count)
                {
                    CurrentBook.PositionInChapter = 0;
                    await ReadBook(continueReading);
                }
                else
                {
                    CurrentBook.Chapter = ChapterFiles.Count - 1;
                }
                return true;
            }
            return false;
        }

        private async Task ReadBook(bool continueReading, string htmlPosition = null)
        {
            if (CurrentBook.Chapter < 0)
            {
                CurrentBook.Chapter = 0;
            }
            else if (CurrentBook.Chapter > ChapterFiles.Count - 1)
            {
                CurrentBook.Chapter = ChapterFiles.Count - 1;
            }

            var chapterFileName = ChapterFiles[CurrentBook.Chapter].Split('#').FirstOrDefault();

            await ReadChapter(chapterFileName, htmlPosition);

            if (continueReading)
            {
                StartReading();
            }
        }

        private async Task<int> GetTotalNumberOfWords()
        {
            CurrentBook.ChapterFileList = new List<ChapterFile>();
            int total = 0;
            foreach (var chapter in ChapterFiles)
            {
                var num = await GetNumberOfWordsInChapter(chapter);
                string chapter1 = chapter;

                var newChapterFile = new ChapterFile
                {
                    File = chapter,
                    NumberOfWords = num,
                    Number = ChapterFiles.FindIndex(s => chapter1 == s)
                };

                CurrentBook.ChapterFileList.Add(newChapterFile);

                total += num;
            }
            return total;
        }

        private async Task<int> GetNumberOfWordsInChapter(string chapterFileName)
        {
            try
            {
                var chapterFile = await _bookFolder.GetFileAsync(chapterFileName.ToWindowsFilePath());
                var chapterHtmlContent = await FileIO.ReadTextAsync(chapterFile);

                var chapterParsedContent = HtmlHelper.StripHtml(chapterHtmlContent);
                var splitedText = chapterParsedContent.Split(' ');

                return splitedText.Length + 1;
            }
            catch (InvalidDataException)
            {
                throw new NotSupportedException("cant unzip");
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine(ex);

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return 0;
            }
        }

        public override async Task ReadChapter(string chapterFileName, string htmlPosition = null)
        {
            try
            {
                if (_bookFolder == null)
                {
                    return;
                }

                if (chapterFileName == null) throw new ArgumentNullException("chapterFileName");
                var chapterFile = await _bookFolder.GetFileAsync(chapterFileName.ToWindowsFilePath());
                var chapterHtmlContent = await FileIO.ReadTextAsync(chapterFile);

                var chapterParsedContent = HtmlHelper.StripHtml(chapterHtmlContent, htmlPosition);
                var splitedText = chapterParsedContent.Split(' ');

                if (htmlPosition != null)
                {
                    var fullParsedContent = HtmlHelper.StripHtml(chapterHtmlContent);
                    SetHtmlPositionInChapter(fullParsedContent, ref splitedText);
                }

                if (splitedText.Count() == 1 && string.IsNullOrWhiteSpace(splitedText[0]))
                {
                    CurrentBook.Chapter++;
                    if (CurrentBook.Chapter < ChapterFiles.Count)
                    {
                        CurrentBook.PositionInChapter = 0;
                        await ReadChapter(ChapterFiles[CurrentBook.Chapter].Split('#').FirstOrDefault());
                    }
                }
                else
                {
                    TextToRead = splitedText;

                    if (CurrentBook.PositionInChapter >= 0)
                    {
                        //if (CurrentBook.PositionInChapter >= 1)
                        //{
                        //	DispatcherHelper.CheckBeginInvokeOnUI(() => CurrentBook.PositionInChapter--);
                        //}
                        FillScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void SetHtmlPositionInChapter(string chapterParsedContent, ref string[] splitedText)
        {
            var entireText = HtmlHelper.StripHtml(chapterParsedContent);
            var entireTextArray = entireText.Split(' ');
            CurrentBook.PositionInChapter = entireTextArray.Length - splitedText.Length;
            if (CurrentBook.PositionInChapter < 0)
            {
                CurrentBook.PositionInChapter = 0;
            }
            splitedText = entireTextArray;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EpubReader()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_zipHelper != null) _zipHelper.Dispose();
                if (_bookFileStream != null) _bookFileStream.Dispose();
                if (Timer != null) Timer.Dispose();
            }
            Disposed = true;
        }

        public override async Task OpenAsync()
        {
            bool success = true;
            bool pathTooLong = false;
            bool bookNotFound = false;
            try
            {
                BookLoading = true;
                CurrentBook.LastAccess = DateTime.Now;
                await GetEpubMetadataAsync();
                await ExtractManifestItems();
                PopulateChapterFiles();
                var ncxItem = GetNcxItemFromManifest();
                if (ncxItem != null) await ParseToc(ncxItem);
                await ReadBook(false);

                CurrentBook.Extracted = true;

                if (CurrentBook.TotalNumberOfWords == 0)
                {
                    CurrentBook.TotalNumberOfWords = await GetTotalNumberOfWords();
                }
                BookLoading = false;
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex);
                success = false;
            }
            catch (XmlException ex)
            {
                Debug.WriteLine(ex);
                success = false;
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex);
                success = false;
            }
            catch (InvalidDataException ex)
            {
                Debug.WriteLine(ex);
                success = false;
            }
            catch (DynamicReaderPathTooLongException)
            {
                pathTooLong = true;
                CurrentBook.Author = "File name is too long";
            }
            catch (DynamicReaderBookNotFoundException)
            {
                bookNotFound = true;
                CurrentBook.Author = "File not found";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debugger.Break();
            }

            if (!success)
            {
                await ShowUnsupportedFileMessage();
            }

            if (pathTooLong)
            {
                await ShowPathTooLongMessage();
            }

            if (bookNotFound)
            {
                await ShowBookNotFoundMessage();
            }
        }

        private async Task ShowBookNotFoundMessage()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            await dialogService.ShowMessage("Book not found", "Error");
        }

        private static async Task ShowPathTooLongMessage()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            await
                dialogService.ShowMessage(
                    "The file name is too long, try to rename the file to a shorter name, and then reimport it.",
                    "File name is too long", "Ok",
                    navigationService.GoBack);
        }

        public async Task GetEpubMetadataAsync()
        {
            try
            {
                await Initialize();
                await ParseMimeType();
                var containerStream = await GetContainerStreamAsync();

                var packageFilePath = ParseContainer(containerStream);
                if (packageFilePath.Contains("/"))
                {
                    _rootFolder = Path.GetDirectoryName(packageFilePath);
                    _rootFolder = _rootFolder.Replace("\\", "/");
                    _rootFolder = _rootFolder + "/";
                }

                await ParsePackageAsync(packageFilePath);
                ParseMetadata();
                ParseManifest();
                await SetBookCover();
            }
            catch (NotSupportedException)
            {
                CurrentBook.Author = "File not supported or corrupted";
            }
            catch (XmlException)
            {
                CurrentBook.Author = "File not supported or corrupted";
            }
            catch (InvalidDataException)
            {
                CurrentBook.Author = "File not supported or corrupted";
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine("EpubReader.GetEpubMetadata" + ", error: " + ex.Message);
            }
            catch (DynamicReaderPathTooLongException)
            {
                CurrentBook.Author = "File name is too long";
            }
            catch (DynamicReaderBookNotFoundException)
            {
                CurrentBook.Author = "File not found";
            }
            catch (IOException ex)
            {
                if (ex.Message == "The filename or extension is too long.\r\n")
                {
                    CurrentBook.Author = "File name is too long";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }
        }

        private async Task Initialize()
        {
            try
            {
                _extractFolder = await Globals.GetExtractFolder();
                _bookFolder = await _extractFolder.
                    CreateFolderAsync(CurrentBook.FileName, CreationCollisionOption.OpenIfExists);
                if (CurrentBook.Extracted) return;

                //var booksFolder = await Globals.GetBooksFolder();

                if (_bookFileStream == null)
                {
                    var bookFile = await StorageFile.GetFileFromPathAsync(CurrentBook.FilePath);
                    _bookFileStream = await bookFile.OpenStreamForReadAsync();
                }

                var bookMemoryStream = new MemoryStream((int)_bookFileStream.Length);
                await _bookFileStream.CopyToAsync(bookMemoryStream);
                _zipHelper = new ZipHelper(bookMemoryStream, _bookFolder);
            }
            catch (FileNotFoundException ex)
            {
                throw new DynamicReaderBookNotFoundException("Book file not found", ex);
            }
            catch (InvalidDataException ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task ParseMimeType()
        {
            try
            {
                if (CurrentBook.Extracted) return;
                if (_zipHelper != null)
                {
                    await _zipHelper.UnzipFile(MIMETYPE_FILE);
                }

                var mimetypeFile = await _bookFolder.GetFileAsync(MIMETYPE_FILE);

                using (var fileStream = await mimetypeFile.OpenStreamForReadAsync())
                using (TextReader reader = new StreamReader(fileStream))
                {
                    var fileContent = reader.ReadToEnd();

                    if (fileContent != MIMETYPE && fileContent != MIMETYPE + "\r\n")
                    {
                        reader.Dispose();
                        fileStream.Dispose();
                        throw new NotSupportedException("mimetype");
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new NotSupportedException("mimetype");
            }
            catch (IOException ex)
            {
                if (ex.Message == "The filename or extension is too long.\r\n")
                {
                    throw new DynamicReaderPathTooLongException("Path too long", ex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task<Stream> GetContainerStreamAsync()
        {
            if (_zipHelper != null)
            {
                await _zipHelper.UnzipFile("META-INF/container.xml");
            }
            var containerFile = await _bookFolder.GetFileAsync(
                Path.Combine(META_INF_DIRECTORY, CONTAINER_XML_FILE));
            var containerStream = await containerFile.OpenStreamForReadAsync();
            return containerStream;
        }

        private string ParseContainer(Stream containerStream)
        {
            var containerDoc = XDocument.Load(containerStream);

            var rootfile = containerDoc.
                Elements(CONTAINER_XML_NAME_SPACE + "container").
                Elements(CONTAINER_XML_NAME_SPACE + "rootfiles").
                Elements(CONTAINER_XML_NAME_SPACE + "rootfile").
                First();

            var mediaType = rootfile.Attribute("media-type").Value.Trim();
            if (mediaType != APPLICATION_OEBPS_PACKAGE)
            {
                throw new NotSupportedException(mediaType);
            }

            var packageFileName = rootfile.Attribute("full-path").Value.Trim();
            containerStream.Dispose();

            return packageFileName;
        }

        private async Task ParsePackageAsync(string packageFilePath)
        {
            // Se o livro já tiver sido extraído, _zipHelper será null	
            if (_zipHelper != null)
            {
                await _zipHelper.UnzipFile(packageFilePath);
            }

            var packageFile = await _bookFolder.GetFileAsync(packageFilePath.ToWindowsFilePath());
            var packageFileStream = await packageFile.OpenStreamForReadAsync();
            var packageDoc = XDocument.Load(packageFileStream);

            Package = packageDoc.Element(IDPF_ORG2007_OPF_NAME_SPACE + "package");
            Metadata = Package.Element(IDPF_ORG2007_OPF_NAME_SPACE + "metadata");
            Manifest = Package.Element(IDPF_ORG2007_OPF_NAME_SPACE + "manifest");
            Spine = Package.Element(IDPF_ORG2007_OPF_NAME_SPACE + "spine");
        }

        private void ParseMetadata()
        {
            try
            {
                var metadataItems = Metadata.Elements();
                var xElements = metadataItems as XElement[] ?? metadataItems.ToArray();

                var authorElement =
                    xElements.FirstOrDefault(x => x.Name == PURL_ORG_ELEMENTS_NAME_SPACE + "creator");
                var titleElement = xElements.FirstOrDefault(x => x.Name == PURL_ORG_ELEMENTS_NAME_SPACE + "title");
                //var descriptionElement = xElements.FirstOrDefault(x => x.Name == PurlOrgElementsNameSpace + "description"); //TODO: Implement this
                var publisherElement =
                    xElements.FirstOrDefault(x => x.Name == PURL_ORG_ELEMENTS_NAME_SPACE + "publisher");

                if (authorElement != null)
                {
                    if (string.IsNullOrWhiteSpace(authorElement.Value))
                    {
                        CurrentBook.Author = "Unknown";
                    }
                    else
                    {
                        CurrentBook.Author = authorElement.Value;
                    }
                }
                else
                {
                    CurrentBook.Author = "Unknown";
                }

                if (titleElement != null)
                    CurrentBook.Title = titleElement.Value;

                // TODO: Not in use at the moment
                //if (descriptionElement != null)
                //    CurrentBook.Description = descriptionElement.Value;

                if (publisherElement != null)
                    CurrentBook.Publisher = publisherElement.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ParseManifest()
        {
            try
            {
                AddNonImageItemsToManifest();
                AddCoverFileToManifest();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void AddNonImageItemsToManifest()
        {
            foreach (var item in Manifest.Elements())
            {
                var fileName = item.Attribute("href").Value;
                fileName = WebUtility.UrlDecode(fileName);
                var id = item.Attribute("id").Value;

                if (Helper.IsImage(fileName)) continue;

                if (fileName.Contains("../"))
                {
                    ManifestItems.Add(id, fileName.Replace("../", string.Empty));
                }
                else
                {
                    ManifestItems.Add(id, _rootFolder + fileName);
                }
            }
        }

        private void AddCoverFileToManifest()
        {
            foreach (var item in Manifest.Elements())
            {
                var fileName = item.Attribute("href").Value;
                fileName = WebUtility.UrlDecode(fileName);
                var id = item.Attribute("id").Value;

                if (!ManifestItems.ContainsKey("cover") && Helper.IsCover(id) && Helper.IsImage(fileName))
                {
                    if (fileName.Contains("../"))
                    {
                        ManifestItems.Add("cover", fileName.Replace("../", string.Empty));
                    }
                    else
                    {
                        ManifestItems.Add("cover", _rootFolder + fileName);
                    }
                    return;
                }
            }
        }

        private async Task SetBookCover()
        {
            try
            {
                if (ManifestItems.Keys.Contains("cover"))
                {
                    var cover = ManifestItems["cover"];
                    if (!CurrentBook.Extracted)
                    {
                        await _zipHelper.UnzipFile(cover);
                    }
                    CurrentBook.Cover = Path.Combine(_bookFolder.Path, cover.ToWindowsFilePath());
                }
                else
                {
                    CurrentBook.Cover = null;
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine(ex);
                CurrentBook.Cover = null;
            }
        }

        private async Task ExtractManifestItems()
        {
            if (CurrentBook.Extracted) return;

            foreach (var item in ManifestItems.Where(item => item.Key != "cover"))
            {
                await _zipHelper.UnzipFile(item.Value);
            }
        }

        private void PopulateChapterFiles()
        {
            var itemrefs = Spine.Elements();
            ChapterFiles = new List<string>();
            foreach (var idref in itemrefs.Select(item => Path.Combine(item.Attribute("idref").Value)))
            {
                ChapterFiles.Add(WebUtility.UrlDecode(ManifestItems[idref]));
            }
        }

        private XElement GetNcxItemFromManifest()
        {
            try
            {
                var ncx = (from n in Manifest.Elements()
                           where n.Attribute("id").Value == "ncx"
                           select n).FirstOrDefault();

                if (ncx != null) return ncx;

                ncx = (from n in Manifest.Elements()
                       where n.Attribute("id").Value == "toc"
                       select n).FirstOrDefault();

                if (ncx != null) return ncx;

                ncx = (from n in Manifest.Elements()
                       where n.Attribute("id").Value == "ncxtoc"
                       select n).FirstOrDefault();

                return ncx;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        private async Task ParseToc(XElement ncxItem)
        {
            try
            {
                Toc = new List<Chapter>();
                var ncxElement = await GetNcxElement(ncxItem);
                var ns = ncxElement.GetDefaultNamespace();
                var navMap = from n in ncxElement.Elements(ns + "navMap")
                             select n;

                var navPoints = navMap.Descendants(ns + "navPoint");
                foreach (var item in navPoints)
                {
                    var text = from t in item.Descendants(ns + "text")
                               select t;

                    var content = from c in item.Descendants(ns + "content")
                                  select c;

                    var source = content.First().Attribute("src").Value;
                    source = WebUtility.UrlDecode(source);
                    var newChapter = new Chapter { File = _rootFolder + source, Title = text.FirstOrDefault().Value };
                    if (!Toc.Contains<Chapter>(newChapter))
                    {
                        Toc.Add(newChapter);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task<XElement> GetNcxElement(XElement ncxItem)
        {
            var ncxFilePath = Path.Combine(_rootFolder, ncxItem.Attribute("href").Value);
            var ncxFile = await _bookFolder.GetFileAsync(ncxFilePath.ToWindowsFilePath());

            var ncxDoc = XDocument.Load(await ncxFile.OpenStreamForReadAsync());
            var ncxElement = ncxDoc.Elements().First();
            return ncxElement;
        }

        public override async Task GoNext()
        {
            try
            {
                if (CurrentBook == null || BookLoading || ChapterFiles == null || TextToRead == null) return;
                StopReading();
                CurrentBook.PositionInChapter = CurrentBook.PositionInChapter + 30;
                if (CurrentBook.PositionInChapter >= TextToRead.Length && ++CurrentBook.Chapter < ChapterFiles.Count)
                {
                    CurrentBook.PositionInChapter = 0;
                    await ReadBook(false);
                }

                if (CurrentBook.Chapter >= ChapterFiles.Count)
                {
                    CurrentBook.Chapter = ChapterFiles.Count - 1;
                }

                if (CurrentBook.PositionInChapter >= TextToRead.Length)
                {
                    CurrentBook.PositionInChapter = TextToRead.Length - 1;
                }
                FillScreen();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override async Task GoPrevious()
        {
            try
            {
                if (CurrentBook == null || BookLoading || ChapterFiles == null || TextToRead == null) return;
                StopReading();
                CurrentBook.PositionInChapter = CurrentBook.PositionInChapter - 31;
                if (CurrentBook.PositionInChapter < 0)
                {
                    CurrentBook.Chapter--;
                    if (CurrentBook.Chapter >= 0)
                    {
                        if (CurrentBook.Chapter >= ChapterFiles.Count)
                        {
                            CurrentBook.Chapter = ChapterFiles.Count - 1;
                        }

                        await ReadChapter(ChapterFiles[CurrentBook.Chapter].Split('#').FirstOrDefault());
                        //Lembrando que PositionInChapter deve ser negativo agora
                        CurrentBook.PositionInChapter = TextToRead.Length + CurrentBook.PositionInChapter;
                        if (CurrentBook.PositionInChapter < 0)
                        {
                            CurrentBook.PositionInChapter = 0;
                        }
                        FillScreen();
                    }
                    else
                    {
                        CurrentBook.Chapter = 0;
                        CurrentBook.PositionInChapter = 0;
                        await ReadBook(false);
                    }
                }
                else
                {
                    FillScreen();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public override async void Seek(int position)
        {
            int i = 0;
            if (CurrentBook != null && CurrentBook.ChapterFileList != null)
            {
                foreach (var chapter in CurrentBook.ChapterFileList)
                {
                    i += chapter.NumberOfWords;
                    if (i >= position)
                    {
                        int totalNumberOfWordsUntilPreviousChapter = i - chapter.NumberOfWords;
                        int newPositionInChapter = position - totalNumberOfWordsUntilPreviousChapter;

                        CurrentBook.PositionInChapter = newPositionInChapter;
                        CurrentBook.Chapter = chapter.Number;
                        await ReadChapter(chapter.File);
                        break;
                    }
                }
            }
        }
    }
}