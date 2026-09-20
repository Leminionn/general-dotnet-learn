using System;

namespace FirstAPIProject.Application.Common.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(Guid userId)
            : base($"User with ID '{userId}' was not found.")
        {
        }

        public UserNotFoundException(string email)
            : base($"User with email '{email}' was not found.")
        {
        }
    }

    public class EmailNotWhitelistedException : Exception
    {
        public EmailNotWhitelistedException(string email)
            : base($"The email address '{email}' is not in the allowed registration whitelist.")
        {
        }
    }

    public class WhitelistNotFoundException : Exception
    {
        public WhitelistNotFoundException(Guid id)
            : base($"Whitelist entry with ID '{id}' was not found.")
        {
        }
    }

    public class WhitelistAlreadyExistsException : Exception
    {
        public WhitelistAlreadyExistsException(string pattern)
            : base($"A whitelist entry with pattern '{pattern}' already exists.")
        {
        }
    }
}
