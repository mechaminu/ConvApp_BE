using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Posting : Feedbackable, IHasViewCount, IRankable
    {
        public int Id { get; set; }
        public byte PostingType { get; set; }
        public int CreatorId { get; set; }

        public int ViewCount { get; set; }

        public ICollection<PostingNode> PostingNodes { get; set; }
        public ICollection<Product> Products { get; set; }

        public double MonthlyScore { get; set; }
        public double SeasonalScore { get; set; }
        public double AlltimeScore { get; set; }
    }

    public class PostingNode
    {
        public int PostingId { get; set; }          // PK, FK
        public byte OrderIndex { get; set; }        // PK, 순서
        public string Text { get; set; }            // 텍스트 내용
        [Column(TypeName = "char(16)")]
        public string ImageFilename { get; set; }   // 이미지 파일명
    }
}
