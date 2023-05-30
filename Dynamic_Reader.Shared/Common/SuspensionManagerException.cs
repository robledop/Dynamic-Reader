using System;

namespace Dynamic_Reader.Common
{
    public class SuspensionManagerException : Exception
    {
        public SuspensionManagerException( Exception e)
            : base("SuspensionManager failed", e)
        {
            if (e == null) throw new ArgumentNullException("e");
        }
    }
}