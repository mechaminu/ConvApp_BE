using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Base62;
using ConvAppServer.Models;
using ConvAppServer.Models.Internal;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using bcrypt = BCrypt.Net.BCrypt;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly MainContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly BlobContainerClient _blob;

        public UsersController(MainContext context, IHttpClientFactory clientFactory, BlobServiceClient blob, ILogger<UsersController> logger)
        {
            _blob = blob.GetBlobContainerClient("images");
            _logger = logger;
            _context = context;
            _clientFactory = clientFactory;
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
        public async Task<ActionResult<UserBreif>> RegisterUser([FromBody] RegisterDTO dto)
        {
            if (await _context.UserAuths.Where(ua => ua.Email == dto.Email).AnyAsync())
                return ValidationProblem("동일 이메일로 가입정보가 이미 존재합니다");

            if (await _context.Users.Where(u => u.Name == dto.Name).AnyAsync())
                return ValidationProblem("이미 사용중인 닉네임입니다");

            var user = new User { Name = dto.Name, ImageFilename= dto.Image };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _context.UserAuths.Add(new UserAuth 
            {
                UserId = user.Id,
                Email = dto.Email,
                PasswordHash = (dto.OAuthProvider == (byte)OAuthProvider.None && dto.Password != null) ? EncryptInput(dto.Password) : null,
                OAuthProvider = dto.OAuthProvider,
                OAuthId=dto.OAuthId 
            });
            await _context.SaveChangesAsync();

            return user.ToBrief();
        }

        [HttpPost("updateoauth")]
        public async Task<ActionResult<UserBreif>> UpdateOAuth([FromBody] RegisterDTO dto)
        {
            UserAuth target;
            try
            {
                target = await _context.UserAuths.Where(ua => ua.Email == dto.Email).SingleAsync();
            }
            catch
            {
                return ValidationProblem("가입정보가 올바르지 않습니다.");
            }

            target.OAuthProvider = dto.OAuthProvider;
            target.OAuthId = dto.OAuthId;
            await _context.SaveChangesAsync();

            return await GetUserBrief((int)target.UserId);
        }

        [HttpGet("login")]
        public async Task<ActionResult<UserBreif>> LoginEmailAccount([FromQuery] string id, [FromQuery] string pwd)
        {
            try
            {
                var ua = await _context.UserAuths.Where(ua => ua.Email == id).SingleAsync();

                if (ua.OAuthId != null)
                    return ValidationProblem($"해당 이메일의 SNS 가입정보가 존재합니다 ({(OAuthProvider)ua.OAuthProvider})\nSNS 로그인으로 진행해주세요!");

                var hashstr = Encoding.ASCII.GetString(ua.PasswordHash);
                var match = bcrypt.Verify(pwd, hashstr);

                if (!match)
                    return ValidationProblem("비밀번호가 일치하지 않습니다");

                return (await _context.Users.FindAsync(ua.UserId)).ToBrief();
            }
            catch
            {
                return ValidationProblem("해당 이메일의 유저정보가 존재하지 않습니다");
            }
        }

        [HttpGet("loginoauth")]
        public async Task<ActionResult<object>> LoginOauthAccount([FromQuery] string token, [FromHeader] byte? provider = null)
        {
            _logger.LogInformation($"OAuth login request - {(OAuthProvider)provider}");
            string oauthId;
            string email;
            string image;

            try
            {
                if (!provider.HasValue)
                    throw new Exception("OAuth 제공자 정보 없음! 헤더의 provider 항목으로 제공해야 함");

                HttpRequestMessage request;
                switch ((OAuthProvider)provider.Value)
                {
                    case OAuthProvider.Kakao:
                        request = new HttpRequestMessage(HttpMethod.Get, "https://kapi.kakao.com/v2/user/me");
                        request.Headers.Add("Authorization", $"Bearer {token}");
                        break;
                    case OAuthProvider.Facebook:
                        request = new HttpRequestMessage(HttpMethod.Get, $"https://graph.facebook.com/me?fields=id,email&access_token={token}");
                        break;
                    case OAuthProvider.Google:
                        request = null;
                        break;
                    default:
                        throw new Exception("알 수 없는 OAuth 제공자");
                }

                JObject json = null;
                if (request != null)
                {
                    var response = await _clientFactory.CreateClient().SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("OAuth 정보 없음");

                    json = JObject.Parse(await response.Content.ReadAsStringAsync());
                }

                HttpResponseMessage imgResponse;
                string imgUrl;
                switch ((OAuthProvider)provider.Value)
                {
                    case OAuthProvider.Kakao:
                        oauthId = json["id"].ToString();
                        email = json["kakao_account"]["email"].ToString();
                        imgUrl = json["properties"]["profile_image"].ToString();
                        imgResponse = await _clientFactory.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, imgUrl));
                        break;
                    case OAuthProvider.Facebook:
                        oauthId = json["id"].ToString();
                        email = json["email"].ToString();
                        imgUrl = $"https://graph.facebook.com/me/picture?access_token={token}";
                        imgResponse = await _clientFactory.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, imgUrl));
                        break;
                    case OAuthProvider.Google:
                        var validPayload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new List<string>() { "681688314448-jk0u2lcrvnt54al1j95hap206uf4u0n1.apps.googleusercontent.com" }
                        });
                        oauthId = validPayload.Subject;
                        email = validPayload.Email;
                        imgResponse = await _clientFactory.CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, validPayload.Picture));
                        break;
                    default:
                        throw new Exception("알 수 없는 OAuth 제공자");
                }

                image = await PostImageStream(await imgResponse.Content.ReadAsStreamAsync(), imgResponse.Content.Headers.ContentType.MediaType);

            }
            catch (Exception ex)
            {
                return ValidationProblem(ex.Message);
            }
            
            try
            {
                var userid = await _context.UserAuths
                .Where(ua => ua.OAuthProvider == provider && ua.OAuthId == oauthId)
                .Select(ua => ua.UserId)
                .SingleAsync();

                return (await _context.Users.FindAsync(userid)).ToBrief();
            }
            catch
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return new { type = (OAuthProvider)provider.Value, oid = oauthId, email = email, image = image };
            }
        }

        public class RegisterDTO
        {
            public string Name { get; set; }
            public string Image { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public byte OAuthProvider { get; set; }
            public string OAuthId { get; set; }
        }

        private byte[] EncryptInput(string str)
        {
            if (str == null)
                return null;

            var hash = bcrypt.HashPassword(str);
            var bytes = Encoding.ASCII.GetBytes(hash);
            return bytes;
        }

        /// <summary>
        ///     이미지를 스트림으로 받아 Azure blob container에 저장
        /// </summary>
        /// <param name="data">이미지 stream 데이터</param>
        /// <param name="type">이미지 MIME 형식</param>
        /// <returns>blob container에 저장된 이미지 파일명</returns>
        private async Task<string> PostImageStream(Stream data, string type)
        {
            try
            {
                string fileName;
                BlobClient blobClient;
                bool dupe;

                do
                {
                    fileName = GenerateFileName();
                    blobClient = _blob.GetBlobClient(fileName);
                    dupe = await blobClient.ExistsAsync();
                } while (dupe);

                await blobClient.UploadAsync(data, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = type } });

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uploading an image failed");
                throw;
            }
        }

        /// <summary>
        ///     시간 및 난수 기반 랜덤 Base62 파일명을 생성
        /// </summary>
        /// <returns>16자리 Base62 파일명</returns>
        private static string GenerateFileName()
        {

            var t = BitConverter.GetBytes(DateTime.UtcNow.Ticks).ToBase62() + "_";

            var base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var len = 16 - t.Length;

            char[] res = new char[len];
            var random = new Random();
            for (int i = 0; i < len; i++)
            {
                int idx = random.Next(0, base62Chars.Length - 1);
                res[i] = base62Chars[idx];
            }

            return t + string.Join("", res);
        }
    }
}
