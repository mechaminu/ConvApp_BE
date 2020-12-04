using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bcrypt = BCrypt.Net.BCrypt;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MainContext _context;

        public UsersController(MainContext context)
        {
            _context = context;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserBreif>> GetUserBrief(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => u.ToBrief())
                .SingleAsync();

            return user == null
                ? NotFound()
                : user;
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<User>> GetUserDetail(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Include(u => u.Postings)
                    .ThenInclude(p => p.PostingNodes.OrderBy(pn => pn.OrderIndex))
                .Include(u => u.Liked)
                .AsSplitQuery()
                .SingleAsync();

            user.LikedPostings = new List<Posting>();
            user.LikedProducts = new List<Product>();

            user.FollowingUsers = new List<UserBreif>();
            user.FollowerUsers = new List<UserBreif>();

            foreach (var elem in user.Liked)
            {
                var feedbackable = await _context.GetFeedbackable((FeedbackableType)elem.ParentType, elem.ParentId);
                switch (Feedbackable.GetEntityType(feedbackable))
                {
                    case FeedbackableType.Posting:
                        user.LikedPostings.Add((Posting)feedbackable);
                        break;
                    case FeedbackableType.Product:
                        user.LikedProducts.Add((Product)feedbackable);
                        break;
                    case FeedbackableType.User:
                        user.FollowingUsers.Add(((User)feedbackable).ToBrief());
                        break;
                    default:
                        break;
                }
            }

            user.Likes = await _context.Likes
                .Where(l => l.ParentType == (byte)FeedbackableType.User)
                .Where(l => l.ParentId == user.Id)
                .ToListAsync();

            return user == null
                ? NotFound()
                : user;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO dto)
        {
            if (await _context.UserAuths.Where(ua => ua.Email == dto.Email).AnyAsync())
                return ValidationProblem("동일 이메일로 가입정보가 이미 존재합니다");

            if (await _context.Users.Where(u => u.Name == dto.Name).AnyAsync())
                return ValidationProblem("이미 사용중인 닉네임입니다");

            var user = new User { Name = dto.Name };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _context.UserAuths.Add(new UserAuth { UserId = user.Id, Email = dto.Email, PasswordHash = EncryptInput(dto.Password) });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserBrief), new { user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserBreif>> LoginUser([FromBody] LoginDTO dto)
        {
            try
            {
                var ua = await _context.UserAuths.Where(ua => ua.Email == dto.Email).SingleAsync();

                if (!bcrypt.Verify(dto.Password, Encoding.ASCII.GetString(ua.PasswordHash)))
                    return ValidationProblem("비밀번호가 일치하지 않습니다");

                return (await _context.Users.FindAsync(ua.UserId)).ToBrief();
            }
            catch
            {
                return ValidationProblem("해당 이메일의 유저정보가 존재하지 않습니다");
            }
        }

        [HttpPost("registerauth/{id}")]
        public async Task<ActionResult> RegisterUserAuth(int id, [FromBody] LoginDTO dto)
        {
            var hash = EncryptInput(dto.Password);

            _context.UserAuths.Add(new UserAuth { UserId = id, Email = dto.Email, PasswordHash = hash });
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class RegisterDTO
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class LoginDTO
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        private byte[] EncryptInput(string str)
        {
            var hash = bcrypt.HashPassword(str);
            var bytes = Encoding.ASCII.GetBytes(hash);

            return bytes;
        }
    }
}
