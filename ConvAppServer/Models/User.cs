using System;
using System.Collections.Generic;

namespace ConvAppServer.Models
{
    // 유저 일반 데이터
    public class User : Feedbackable
    {
        public string Name { get; set; }
        public string ProfileImage { get; set; }

        public List<Like> CreatedLikes { get; set; }
        public List<Posting> CreatedPostings { get; set; }
        public List<Comment> CreatedComments { get; set; }
    }

    // 유저 인증정보
    public class UserAuth
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}