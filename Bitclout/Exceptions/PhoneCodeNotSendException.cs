using System;

namespace Bitclout.Exceptions
{
    class PhoneCodeNotSendException : Exception
    {
        public PhoneCodeNotSendException(string message) : base(message)
        {
        }
    }
}
