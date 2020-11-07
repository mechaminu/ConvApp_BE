using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class ContentNode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long parent_id { get; set; }
        public byte order_number { get; set; }
    }
}
