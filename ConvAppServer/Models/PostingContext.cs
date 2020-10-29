using Microsoft.EntityFrameworkCore;

namespace ConvAppServer.Models
{
    public class PostingContext : DbContext
    {
        public PostingContext(DbContextOptions<PostingContext> options)
            : base(options)
        {
        }

        public DbSet<UserRecipe> UserRecipes { get; set; }
    }
}
