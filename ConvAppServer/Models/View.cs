using System;

namespace ConvAppServer.Models
{
    public class View
    {
        public DateTime Date { get; set; }
        public byte ParentType { get; set; }
        public int ParentId { get; set; }
        public int UserId { get; set; }
    }
}