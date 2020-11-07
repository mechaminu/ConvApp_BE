using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Posting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public bool is_recipe { get; set; }
        public long create_user_oid { get; set; }

        public DateTime create_date { get; set; }
        public DateTime modify_date { get; set; }
        public DateTime delete_date { get; set; }

        public string title { get; set; }
    }
}
