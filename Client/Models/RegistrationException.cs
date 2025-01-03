﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class RegistrationException : Exception
    {
        public RegistrationException(string message) : base(message) { }
    }

    public class LoginException : Exception
    {
        public LoginException(string message) : base(message) { }
    }

    public class ServerConnectionException : Exception
    {
        public ServerConnectionException(string message) : base(message) { }
    }
}
