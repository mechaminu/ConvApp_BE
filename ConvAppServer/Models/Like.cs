namespace ConvAppServer.Models
{
    public class Like : BaseEntity
    {
        public byte ParentType { get; set; }
        public int ParentId { get; set; }
        public int CreatorId { get; set; }
    }
}
