using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace ConvAppServer.Models
{
    public class Posting
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public bool IsRecipe { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; }

        public List<PostingNode> PostingNodes { get; set; }
        public List<Product> Products { get; set; }
    }

    public class PostingNode
    {
        public int Id { get; set; }
        public byte OrderIndex { get; set; }        // 순서 첨자
        public string Text { get; set; }            // 텍스트 내용
        public string ImageFilename { get; set; }   // 이미지 파일명
    }
}
