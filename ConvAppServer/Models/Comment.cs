using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Comment : Feedbackable
    {
        public int Id { get; set; }
        public byte ParentType { get; set; }
        public int ParentId { get; set; }
        public int CreatorId { get; set; }

        [Column(TypeName = "nvarchar(4000)")]
        public string Text { get; set; }
    }
}
