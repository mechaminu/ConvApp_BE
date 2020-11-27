using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    // 유저 일반 데이터
    public class User : Feedbackable, IRankable
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ImageFilename { get; set; }

        // Secrets
        [Column(TypeName = "varchar(255)")]
        public string Email { get; set; }
        [Column(TypeName = "char(64)")]
        public string PasswordHash { get; set; }

        public double MonthlyScore { get; set; }
        public double SeasonalScore { get; set; }
        public double AlltimeScore { get; set; }

        public static UserDTO ToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                ImageFilename = user.ImageFilename
            };
        }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageFilename { get; set; }
    }
}