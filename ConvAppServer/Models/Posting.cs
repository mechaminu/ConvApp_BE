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
        public List<PostingNode> PostingNodes { get; set; }
        public List<Product> Products { get; set; }
    }

    public class PostingNode
    {
        public int PostingId { get; set; }          // FK, PK*
        public byte OrderIndex { get; set; }        // PK*, 순서
        public string Text { get; set; }            // 텍스트 내용
        public string ImageFilename { get; set; }   // 이미지 파일명
    }
}
