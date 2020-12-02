using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConvAppServer.Models.Interfaces;

namespace ConvAppServer.Models
{
    public class Comment : Feedbackable, IModifiable
    {
        public DateTime ModifiedDate { get; set; }

        [Required]
        public byte ParentType { get; set; }
        [Required]
        public int ParentId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Column(TypeName = "nvarchar(4000)")]
        public string Text { get; set; }
    }
}
