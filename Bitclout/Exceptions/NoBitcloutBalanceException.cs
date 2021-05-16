using System;

namespace Bitclout.Exceptions
{
    class NoBitcloutBalanceException : Exception
    {
        public NoBitcloutBalanceException(string message) : base(message)
        {
        }
    }
}
