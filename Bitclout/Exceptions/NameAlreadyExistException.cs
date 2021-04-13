using System;

namespace Bitclout.Exceptions
{
    class NameAlreadyExistException : Exception
    {
        public NameAlreadyExistException(string message) : base(message)
        {
        }
    }
}
