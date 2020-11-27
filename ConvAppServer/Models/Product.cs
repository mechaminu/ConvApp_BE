﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Product : Feedbackable, IHasViewCount, IRankable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }  // or decimal for the future USD implementation?
        [Column(TypeName = "char(16)")]
        public string ImageFilename { get; set; }

        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        public int ViewCount { get; set; }

        public ICollection<Posting> Postings { get; set; }

        public double MonthlyScore { get; set; }
        public double SeasonalScore { get; set; }
        public double AlltimeScore { get; set; }
    }

    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFileName { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFileName { get; set; }
    }
}
