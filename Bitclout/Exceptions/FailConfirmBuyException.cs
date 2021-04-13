using System;

namespace Bitclout.Exceptions
{
    class FailConfirmBuyException : Exception
    {
        public FailConfirmBuyException(string message) : base(message)
        {
        }
    }
}
