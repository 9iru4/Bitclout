using System;

namespace Bitclout.Exceptions
{
    class FailToStartBitcloutChromeDriverException : Exception
    {
        public FailToStartBitcloutChromeDriverException(string message) : base(message)
        {
        }
    }
}
