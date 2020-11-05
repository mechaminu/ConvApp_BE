using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class User
    {
        [Key]
        public string Username { get; set; }
        public string Userid { get; set; }
    }
}
