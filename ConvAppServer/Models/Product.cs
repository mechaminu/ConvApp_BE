using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Product : Feedbackable
    {
        public byte StoreType { get; set; }
        public byte CategoryType { get; set; }

        public int Price { get; set; }  // or decimal for the future USD implementation?

        public string Name { get; set; }
        public string Image { get; set; }

        public List<Posting> Postings { get; set; }
    }
}
