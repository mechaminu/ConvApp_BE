using ConvAppServer.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    // 유저 일반 데이터
    public class User : Feedbackable, IRankable
    {
        [Column(TypeName = "nvarchar(15)")]
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFilename { get; set; }

        public List<Like> Liked { get; set; }
        public List<Posting> Postings { get; set; }

        [NotMapped]
        public List<Posting> LikedPostings { get; set; }
        [NotMapped]
        public List<Product> LikedProducts { get; set; }
        [NotMapped]
        public List<UserBreif> FollowingUsers { get; set; }
        [NotMapped]
        public List<UserBreif> FollowerUsers { get; set; }

        public double MonthlyScore { get; set; }
        public double SeasonalScore { get; set; }
        public double AlltimeScore { get; set; }

        public UserBreif ToBrief()
        {
            return new UserBreif
            {
                Id = this.Id,
                Name = this.Name,
                ImageFilename = this.ImageFilename
            };
        }
    }

    public class UserBreif
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageFilename { get; set; }
    }

    public class UserAuth
    {
        [Key]
        public long UserId { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Email { get; set; }
        [Column(TypeName = "binary(60)")]
        public byte[] PasswordHash { get; set; }

        public byte OAuthProvider { get; set; }
        [Column(TypeName = "varchar(255)")]
        public string OAuthId { get; set; }
    }
}