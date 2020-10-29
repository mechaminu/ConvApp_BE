using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class UserRecipe
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity),Key]
        public long oid { get; set; }

        public byte[] createdate { get; set; }      // Timestamp
        public Guid createuser { get; set; }        // uniqueidentifier
        public string textcontent { get; set; }         // ntext
        public byte[] images { get; set; }    // varbinary
    }
}
