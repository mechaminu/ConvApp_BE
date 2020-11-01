using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Posting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public DateTime create_date { get; set; }
        public DateTime modify_date { get; set; }
        public long create_user_oid { get; set; }

        public byte pst_type { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string images { get; set; }
        public string products { get; set; }
    }
}
