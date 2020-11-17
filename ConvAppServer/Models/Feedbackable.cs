using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class Feedbackable : BaseEntity
    {
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }

        [NotMapped]
        public List<Comment> Comments { get; set; }
        [NotMapped]
        public List<Like> Likes { get; set; }

        public FeedbackableType GetEntityType()
        {
            FeedbackableType res = 0;
            switch (this.GetType().Name)
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
