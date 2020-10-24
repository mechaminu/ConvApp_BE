using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class PostContext : DbContext
    {
        public PostContext(DbContextOptions<PostContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
    }
}
