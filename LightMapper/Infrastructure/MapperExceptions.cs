using System;
using System.Runtime.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace LightMapper.Infrastructure
{
    [Serializable]
    public class MappingNotFoundException : Exception
    {
        public MappingNotFoundException() : base() { }
        public MappingNotFoundException(string message) : base(message) { }
        public MappingNotFoundException(string format, params object[] args) : base(string.Format(format, args)) { }
        public MappingNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        public MappingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class CompilationFailedException : Exception
    {
        public CompilationFailedException() : base() { }
        public CompilationFailedException(string message) : base(message) { }
        public CompilationFailedException(string format, params object[] args) : base(string.Format(format, args)) { }
        public CompilationFailedException(string message, Exception innerException) : base(message, innerException) { }
        public CompilationFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class MappingFailedException : Exception
    {
        public MappingFailedException() : base() { }
        public MappingFailedException(string message) : base(message) { }
        public MappingFailedException(string format, params object[] args) : base(string.Format(format, args)) { }
        public MappingFailedException(string message, Exception innerException) : base(message, innerException) { }
        public MappingFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member