using System.ComponentModel.DataAnnotations.Schema;

namespace ConvAppServer.Models
{
    // 유저 일반 데이터
    public class User : Feedbackable
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(15)")]
        public string Name { get; set; }
        [Column(TypeName = "char(16)")]
        public string ProfileImage { get; set; }

        // Secrets
        [Column(TypeName = "varchar(255)")]
        public string Email { get; set; }
        [Column(TypeName = "char(64)")]
        public string PasswordHash { get; set; }

        public static UserDTO ToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                ProfileImage = user.ProfileImage
            };
        }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfileImage { get; set; }
    }
}