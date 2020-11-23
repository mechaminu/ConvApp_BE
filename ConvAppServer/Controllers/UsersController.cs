using ConvAppServer.Models;
using Microsoft.AspNetCore.Mvc;
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

            return user == null
                ? NotFound()
                : Models.User.ToDTO(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { user.Id }, user);
        }
    }
}
