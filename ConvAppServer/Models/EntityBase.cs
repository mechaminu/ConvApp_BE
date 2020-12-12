using System;
using System.ComponentModel.DataAnnotations;

namespace ConvAppServer.Models
{
    public class EntityBase
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
