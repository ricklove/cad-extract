using System;

namespace CadExtract.Library
{
    public class LibraryException : Exception
    {
        public LibraryException(string message) : base(message) { }
        public LibraryException(string message, Exception inner) : base(message, inner) { }
    }

    public class ResourceAccessException : Exception
    {
        public ResourceAccessException(string message) : base(message) { }
        public ResourceAccessException(string message, Exception inner) : base(message, inner) { }
    }
}
