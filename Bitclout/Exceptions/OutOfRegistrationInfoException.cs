using System;

namespace Bitclout.Exceptions
{
    class OutOfRegistrationInfoException : Exception
    {
        public OutOfRegistrationInfoException(string message) : base(message)
        {
        }
    }
}
