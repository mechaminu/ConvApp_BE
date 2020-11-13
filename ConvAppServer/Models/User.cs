using System.Collections.Generic;

namespace ConvAppServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfileImage { get; set; }
        public List<Posting> CreatedPostings { get; set; }
    }
}