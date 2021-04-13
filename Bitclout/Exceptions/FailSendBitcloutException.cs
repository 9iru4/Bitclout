using System;

namespace Bitclout.Exceptions
{
    class FailSendBitcloutException : Exception
    {
        public FailSendBitcloutException(string message) : base(message)
        {
        }
    }
}
