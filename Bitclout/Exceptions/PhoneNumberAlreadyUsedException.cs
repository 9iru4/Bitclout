using System;

namespace Bitclout.Exceptions
{
    class PhoneNumberAlreadyUsedException : Exception
    {
        public PhoneNumberAlreadyUsedException(string message) : base(message)
        {
        }
    }
}
