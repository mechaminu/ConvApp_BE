using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Feedbackable : BaseEntity
    {
        public byte EntityType { get; set; }

        public int CommentCount { get; set; }
        [NotMapped]
        public List<Comment> Comments { get; set; }

        public int LikeCount { get; set; }
        [NotMapped]
        public List<Like> Likes { get; set; }

        public static FeedbackableType GetEntityType(Feedbackable f)
        {
            return f.GetType().Name switch
            {
                "Posting" => FeedbackableType.Posting,
                "Product" => FeedbackableType.Product,
                "Comment" => FeedbackableType.Comment,
                "User" => FeedbackableType.User,
                _ => throw new InvalidOperationException("Can't find entity type!"),
            };
        }
    }

    public enum FeedbackableType : byte
    {
        Posting,
        Product,
        Comment,
        User
    }
}
