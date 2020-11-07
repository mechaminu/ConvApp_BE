using Microsoft.EntityFrameworkCore;

namespace ConvAppServer.Models
{
    public class PostingContext : DbContext
    {
        public PostingContext(DbContextOptions<PostingContext> options) : base(options) { }

        public DbSet<Posting> Posting { get; set; }
        public DbSet<PostingNode> PostingNode { get; set; }
        public DbSet<ProductNode> ProductNode { get; set; }
    }
}
