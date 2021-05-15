using System;

namespace Bitclout.Exceptions
{
    class BadSyncResponseException : Exception
    {
        public BadSyncResponseException(string message) : base(message)
        {
        }
    }
}
