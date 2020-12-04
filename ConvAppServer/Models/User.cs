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
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageFilename { get; set; }
    }

    public class UserAuth
    {
        [Key]
        public int UserId { get; set; }
        [Column(TypeName = "varchar(255)")]
        public string Email { get; set; }

        // 비밀번호 찾기는 정확한 비밀번호를 줄 수 없고, 해당 이메일을 바탕으로 비밀번호 재설정 메일을 보내준다.
        // TODO 메일 전송 솔루션 탐색
        [Column(TypeName = "binary(60)")]
        public byte[] PasswordHash { get; set; }
    }
}