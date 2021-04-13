using System;

namespace Bitclout.Exceptions
{
    class NoPhoneBalanceException : Exception
    {
        public NoPhoneBalanceException(string message) : base(message)
        {
        }
    }
}
