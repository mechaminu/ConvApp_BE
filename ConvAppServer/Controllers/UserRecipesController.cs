using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConvAppServer.Models;

namespace ConvAppServer.Controllers
{
    [Route("api/posting/[controller]")]
    [ApiController]
    public class UserRecipesController : ControllerBase
    {
        private readonly PostingContext _context;

        public UserRecipesController(PostingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRecipe>>> GetUserRecipes()
        {
            return await _context.UserRecipes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserRecipe>> GetUserRecipe(long id)
        {
            var userRecipe = await _context.UserRecipes.FindAsync(id);

            if (userRecipe == null)
            {
                return NotFound();
            }

            return userRecipe;
        }

        #region 페이지네이션 데이터 제공
        // 페이지 총 갯수 리턴
        [HttpGet("page")]
        public async Task<ActionResult<int>> GetUserRecipeTotalPage()
        {
            var cntList = await _context.UserRecipes.CountAsync();
            int itemsPerPage = 10;
            int totalPage = cntList / itemsPerPage;

            return cntList % itemsPerPage != 0 ? totalPage + 1 : totalPage;
        }

        // 원하는 페이지에 해당하는 아이템 리스트 리턴
        [HttpGet("page/{page}")]
        public async Task<ActionResult<IEnumerable<UserRecipe>>> GetUserRecipePage(int page)
        {
            return (await _context.UserRecipes.ToListAsync()).Skip(page * 10).Take(10).ToList();
        }
        #endregion


        #region 인피니트 스크롤 데이터 제공
        // TODO
        #endregion

        // PUT: api/UserRecipes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserRecipe(long id, UserRecipe userRecipe)
        {
            if (id != userRecipe.oid)
            {
                return BadRequest();
            }

            _context.Entry(userRecipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRecipeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserRecipes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<UserRecipe>> PostUserRecipe(UserRecipe userRecipe)
        {
            _context.UserRecipes.Add(userRecipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserRecipe", new { id = userRecipe.oid }, userRecipe);
        }

        // DELETE: api/UserRecipes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserRecipe>> DeleteUserRecipe(long id)
        {
            var userRecipe = await _context.UserRecipes.FindAsync(id);
            if (userRecipe == null)
            {
                return NotFound();
            }

            _context.UserRecipes.Remove(userRecipe);
            await _context.SaveChangesAsync();

            return userRecipe;
        }

        private bool UserRecipeExists(long id)
        {
            return _context.UserRecipes.Any(e => e.oid == id);
        }
    }
}
