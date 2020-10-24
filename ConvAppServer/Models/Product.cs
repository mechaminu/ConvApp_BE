using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public byte Store { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Comment { get; set; }
        public string Image { get; set; }
    }
}
