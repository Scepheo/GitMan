using System;
using System.Net;

namespace GitMan.Providers
{
    internal class RemoteProviderException : Exception
    {
        public RemoteProviderException(HttpStatusCode statusCode, string message)
            : base($"Remove provider returned {(int)statusCode} - {statusCode}:\n{message}")
        { }
    }
}
