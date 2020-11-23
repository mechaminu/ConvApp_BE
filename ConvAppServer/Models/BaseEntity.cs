using System;
using System.ComponentModel.DataAnnotations;

namespace ConvAppServer.Models
{
    public class BaseEntity
    {
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
    }
}
