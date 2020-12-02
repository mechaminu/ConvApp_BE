using System;
using System.ComponentModel.DataAnnotations;

namespace ConvAppServer.Models
{
    public class EntityBase
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
