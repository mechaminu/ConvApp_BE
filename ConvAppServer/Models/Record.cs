using System;
namespace ConvAppServer.Models
{
    public class Record : EntityBase
    {
        public byte ParentType { get; set; }
        public int ParentId { get; set; }
        public int UserId { get; set; }
    }
}
