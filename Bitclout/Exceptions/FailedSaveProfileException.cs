using System;

namespace Bitclout.Exceptions
{
    class FailedSaveProfileException : Exception
    {
        public FailedSaveProfileException(string message) : base(message)
        {
        }
    }
}
