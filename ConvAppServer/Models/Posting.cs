using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Posting
    {
        public long Id { get; set; }
        public bool IsRecipe { get; set; }
        public long CreatorId { get; set; }

        public DateTime create_date { get; set; }
        public DateTime modify_date { get; set; }
        public DateTime delete_date { get; set; }

        public List<PostingNode> PostingNodes { get; set; }
    }

    public class PostingNode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long parent_id { get; set; }
        public byte order_number { get; set; }
        public string text { get; set; }    // 텍스트 설명
        public string images { get; set; }  // 이미지 파일명
    }

    public class PostingNodeClient
    {
        public string text { get; set; }
        public string image { get; set; }
    }

    public class PostingSet : Posting
    {
        public List<PostingNodeClient> PostingNodes { get; set; }
    }
}
