using System;

namespace Bitclout.Exceptions
{
    public class FailedInitializeBitcloutChromeDriver : Exception
    {
        public FailedInitializeBitcloutChromeDriver(string message) : base(message)
        {
        }
    }
}
