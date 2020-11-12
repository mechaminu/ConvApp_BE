using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class Feedback
    {
        public Posting Posting { get; set; }

        public int Views { get; set; }

        public List<User> UserLiked { get; set; }

        public List<Comment> Comments { get; set; }
    }

    public class Comment
    {
        public Feedback Feedback { get; set; }

        public User Creator { get; set; }
        public DateTime Created { get; set; }

        public string Text { get; set; }
    }
}
