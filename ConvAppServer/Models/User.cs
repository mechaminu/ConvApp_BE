using System.Collections.Generic;

namespace ConvAppServer.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string PwdDigest { get; set; }

        public int Name { get; set; }
        public string ProfileImage { get; set; }

        public List<Posting> CreatedPostings { get; set; }
    }
}