using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class Comment : Feedbackable
    {
        public int ParentId { get; set; }
        public byte ParentType { get; set; }

        public int CreatorId { get; set; }
        public string Text { get; set; }
    }
}
