using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;
using Dynamic_Reader.Model;
using Dynamic_Reader.Util;
using Dynamic_Reader.ViewModel;
using GalaSoft.MvvmLight.Ioc;

namespace Dynamic_Reader.Helpers
{
    public static class SerializationHelper
    {
        private const string BOOKS_FILE = "Books.xml";

        public static async Task<ObservableCollection<Book>> DeserializeBooksAsync()
        {
            Debug.WriteLine("Deserializing Books");
            try
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<Book>));
                var dataFolder = await Globals.GetDataFolder();
                var file = await dataFolder.CreateFileAsync(BOOKS_FILE, CreationCollisionOption.OpenIfExists);
                var text = await FileIO.ReadTextAsync(file);
                TextReader textReader = new StringReader(text);
                var xmlReader = XmlReader.Create(textReader);
                try
                {
                    if (serializer.CanDeserialize(xmlReader))
                    {
                        return serializer.Deserialize(xmlReader) as ObservableCollection<Book>;
                    }
                    return new ObservableCollection<Book>();
                }
                finally
                {
                    textReader.Dispose();
                    xmlReader.Dispose();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                DebugReporter.ReportError(ex);
                Debugger.Break();
                return new ObservableCollection<Book>();
            }
            catch(Exception ex)
            {
                DebugReporter.ReportError(ex);
                Debug.WriteLine("Creating initial Books.xml");
                return new ObservableCollection<Book>();
            }
        }

        public static async Task SerializeAsync(ObservableCollection<Book> toSerialize = null)
        {
            if (toSerialize == null)
            {
                var mainViewModel = SimpleIoc.Default.GetInstance<MainViewModel>();
                toSerialize = mainViewModel.Books;
            }

            if (!toSerialize.Any()) return;

            Debug.WriteLine("Serializing Books");

            var xmlString = new StringBuilder();
            TextWriter xmlWriter = new StringWriter(xmlString);
            try
            {
                var serializer = new XmlSerializer(typeof (ObservableCollection<Book>));
                serializer.Serialize(xmlWriter, toSerialize);

                var dataFolder = await Globals.GetDataFolder();
                var file = await dataFolder.CreateFileAsync(BOOKS_FILE, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, xmlString.ToString());
            }
            catch (InvalidOperationException ex)
            {
                DebugReporter.ReportError(ex);
            }
            catch (Exception ex)
            {
                DebugReporter.ReportError(ex);
                Debugger.Break();
            }
            finally
            {
                xmlWriter.Dispose();
            }
        }
    }
}