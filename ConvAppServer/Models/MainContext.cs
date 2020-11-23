using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class MainContext : DbContext
    {
        private ILogger _logger;

        public MainContext(DbContextOptions<MainContext> options, ILogger<MainContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<Posting> Postings { get; set; }
        public DbSet<User> Users { get; set; }

        // 제품
        public DbSet<Product> Products { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Category> Categories { get; set; }

        // 피드백
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 포스팅 및 포스팅 노드 테이블 정의
            modelBuilder.Entity<Posting>(b =>
            {
                b.HasMany(p => p.PostingNodes)
                 .WithOne();
            });

            modelBuilder.Entity<PostingNode>()
                .HasKey(pn => new { pn.PostingId, pn.OrderIndex });

            // 피드백 및 코멘트 테이블 정의
            modelBuilder.Entity<Like>(b =>
            {
                b.HasKey(l => new { l.ParentType, l.ParentId, l.CreatorId });
            });

            modelBuilder.Entity<Comment>(b =>
            {
                b.HasKey(c => new { c.ParentType, c.ParentId, c.CreatedDate });

                b.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(c => c.CreatorId)
                 .HasPrincipalKey(u => u.Id);
            });

            // 제품 테이블 정의
            modelBuilder.Entity<Product>(b =>
            {
                b.HasOne<Store>()
                 .WithMany()
                 .HasForeignKey(p => p.StoreId)
                 .HasPrincipalKey(s => s.Id);

                b.HasOne<Category>()
                 .WithMany()
                 .HasForeignKey(p => p.CategoryId)
                 .HasPrincipalKey(c => c.Id);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var elem = entityEntry.Entity as BaseEntity;
                elem.ModifiedDate = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    elem.CreatedDate = DateTime.UtcNow;

                    if (elem is Feedbackable)
                    {
                        var elemf = elem as Feedbackable;
                        elemf.EntityType = (byte)Feedbackable.GetEntityType(elemf);
                        if (elemf is IHasViewCount)
                            (elemf as IHasViewCount).ViewCount = 0;
                        elemf.CommentCount = 0;
                        elemf.LikeCount = 0;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
