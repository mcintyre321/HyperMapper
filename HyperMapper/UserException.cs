using System;

namespace HyperMapper
{
    public class UserException : Exception
    {
        public string MessageForUser { get; private set; }
        public UserException(string messageForUser)
        {
            this.MessageForUser = messageForUser;
        }
    }
}