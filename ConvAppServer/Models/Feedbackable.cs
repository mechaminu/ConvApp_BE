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
            FeedbackableType res = 0;
            switch (f.GetType().Name)
            {
                case "Posting":
                    res = FeedbackableType.Posting;
                    break;
                case "Product":
                    res = FeedbackableType.Product;
                    break;
                case "Comment":
                    res = FeedbackableType.Comment;
                    break;
                case "User":
                    res = FeedbackableType.User;
                    break;
                default:
                    throw new InvalidOperationException("Can't find entity type!");
            }

            return res;
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
