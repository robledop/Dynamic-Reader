using GalaSoft.MvvmLight;

namespace Dynamic_Reader.Model
{
    public class BookToImport : ObservableObject
    {
        private string _name;
        private string _path;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged();
            }
        }
    }
}