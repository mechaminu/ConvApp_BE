using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Product
    {
        public int Id { get; set; }
        public byte Store { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        public List<Posting> Postings { get; set; }
    }
}
