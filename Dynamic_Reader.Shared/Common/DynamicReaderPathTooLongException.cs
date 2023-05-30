using System;

namespace Dynamic_Reader.Common
{
    class DynamicReaderPathTooLongException : Exception
    {
        public DynamicReaderPathTooLongException(string message, Exception innerException) : base(message,innerException)
        {
            
        }
    }
}
