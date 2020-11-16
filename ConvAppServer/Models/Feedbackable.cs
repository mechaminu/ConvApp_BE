using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class Feedbackable : BaseEntity
    {
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
