using System;

namespace Bitclout.Exceptions
{
    class BadProxyException : Exception
    {
        public BadProxyException(string message) : base(message)
        {
        }
    }
}
