using System;

namespace ConvAppServer.Models
{
    public class Feedbackable : BaseEntity
    {
        public byte EntityType { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }

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

    interface IHasViewCount
    {
        int ViewCount { get; set; }
    }

    interface IRankable
    {
        double MonthlyScore { get; set; }
        double SeasonalScore { get; set; }
        double AlltimeScore { get; set; }
    }
}
