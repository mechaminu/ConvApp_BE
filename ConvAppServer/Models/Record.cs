namespace ConvAppServer.Models
{
    public class Record : EntityBase
    {
        public byte ParentType { get; set; }
        public long ParentId { get; set; }
        public long UserId { get; set; }
    }
}
