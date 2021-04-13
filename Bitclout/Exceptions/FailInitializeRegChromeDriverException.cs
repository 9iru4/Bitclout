using System;

namespace Bitclout.Exceptions
{
    class FailInitializeRegChromeDriverException : Exception
    {
        public FailInitializeRegChromeDriverException(string message) : base(message)
        {
        }
    }
}
