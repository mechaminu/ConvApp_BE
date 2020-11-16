using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ConvAppServer.Models
{
    public class Posting : Feedbackable
    {
        public byte PostingType { get; set; }
        public int CreatorId { get; set; }

        public List<Product> Products { get; set; }
        public List<PostingNode> PostingNodes { get; set; }
    }

    public class PostingNode
    {
        public int PostingId { get; set; }          // PK, FK
        public byte OrderIndex { get; set; }        // PK, 순서
        public string Text { get; set; }            // 텍스트 내용
        public string Image { get; set; }           // 이미지 파일명
    }
}
