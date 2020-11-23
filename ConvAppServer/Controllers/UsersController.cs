using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Models.User.ToDTO(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostUser()
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                using (var reqStream = Request.Body)
                    await reqStream.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            var json = Encoding.UTF8.GetString(bytes);
            var user = JsonConvert.DeserializeObject<User>(json);

            _context.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { user.Id }, user);
        }
    }
}
