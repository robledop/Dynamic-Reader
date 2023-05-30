using System;

namespace Dynamic_Reader.Common
{
    public class DynamicReaderException : Exception
    {
        public DynamicReaderException()
        {
        }

        public DynamicReaderException(string message)
            : base(message)
        {
            if (message == null) throw new ArgumentNullException("message");
        }

        public DynamicReaderException( string message,  Exception innerException)
            : base(message, innerException)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (innerException == null) throw new ArgumentNullException("innerException");
        }
    }
}