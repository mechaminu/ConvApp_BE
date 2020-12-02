using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConvAppServer.Models.Interfaces;

namespace ConvAppServer.Models
{
    public class Product : Feedbackable, IRankable
    {
        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(4000)")]
        public string Description { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFilename { get; set; }
        public int Price { get; set; }  // or decimal for the future USD implementation?

        public ICollection<Posting> Postings { get; set; }

        public double MonthlyScore { get; set; }
        public double SeasonalScore { get; set; }
        public double AlltimeScore { get; set; }
    }

    public class ProductDetail
    {
        [Key]
        public int ProductId { get; set; }
    }

    public class Store
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFileName { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFileName { get; set; }
    }
}
