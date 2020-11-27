using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    public class Feedbackable : BaseEntity
    {
        public byte EntityType { get; set; }

        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        // 조회수 등록이 필요한 경우 IHasViewCount 구현하도록 함. << TODO 조회수 증가 메커니즘까지 Entity 클래스에서 제공

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
