using System;
using System.Runtime.Serialization;

namespace AlvTimeWebApi.Exceptions
{
    [Serializable]
    public class AuthorizationException : Exception
    {
        public AuthorizationException(string message) : base(
            $"You are not authorized to perform this action: {message}")
        {
        }

        protected AuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}