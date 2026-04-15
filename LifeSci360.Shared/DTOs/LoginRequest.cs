using System;
using System.Collections.Generic;
using System.Text;

namespace LifeSci360.Shared.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
