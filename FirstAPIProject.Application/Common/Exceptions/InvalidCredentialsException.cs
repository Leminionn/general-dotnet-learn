using System;
using System.Collections.Generic;
using System.Text;

namespace FirstAPIProject.Application.Common.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("Invalid email or password.")
        {
        }
    }
}
