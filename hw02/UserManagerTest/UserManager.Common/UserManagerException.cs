using System;

namespace UserManager.Common
{
    public class UserManagerException : Exception
    {
        public UserManagerException(string message) : base(message)
        {
        }
    }
}
