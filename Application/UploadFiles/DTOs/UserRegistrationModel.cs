using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadFiles.DTOs
{
    public class UserRegistrationModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}