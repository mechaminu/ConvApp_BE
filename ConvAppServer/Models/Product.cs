using System.Collections.Generic;

namespace ConvAppServer.Models
{
    public class Product : Feedbackable, IHasViewCount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }  // or decimal for the future USD implementation?
        public string Image { get; set; }

        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        public int ViewCount { get; set; }

        public ICollection<Posting> Postings { get; set; }
    }

    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageFileName { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageFileName { get; set; }
    }
}
