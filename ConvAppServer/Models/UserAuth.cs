using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConvAppServer.Models
{
    public class UserAuth
    {
        public string Email { get; set; }
        public string PwdDigest { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}