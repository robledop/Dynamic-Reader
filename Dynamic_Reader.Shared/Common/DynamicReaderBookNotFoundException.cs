using System;

namespace Dynamic_Reader.Common
{
    class DynamicReaderBookNotFoundException : Exception
    {
        public DynamicReaderBookNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
