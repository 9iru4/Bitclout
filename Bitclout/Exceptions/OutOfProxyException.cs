using System;

namespace Bitclout.Exceptions
{
    public class OutOfProxyException : Exception
    {
        public OutOfProxyException(string message) : base(message)
        {
        }
    }
}
