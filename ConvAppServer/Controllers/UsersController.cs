using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConvAppServer.Models;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SqlContext _context;

        public UsersController(SqlContext context)
        {
            _context = context;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddUser()
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
