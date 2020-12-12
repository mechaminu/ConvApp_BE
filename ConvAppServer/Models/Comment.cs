using ConvAppServer.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Comment : Feedbackable, IModifiable
    {
        public DateTime ModifiedDate { get; set; }

        [Required]
        public byte ParentType { get; set; }
        [Required]
        public long ParentId { get; set; }
        [Required]
        public long UserId { get; set; }
        [Column(TypeName = "nvarchar(4000)")]
        public string Text { get; set; }
    }
}
