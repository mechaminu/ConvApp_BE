using ConvAppServer.Models;
using ConvAppServer.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConvAppServer
{
    public class MainContext : DbContext
    {
        public MainContext(DbContextOptions<MainContext> options) : base(options) { }

        public DbSet<Posting> Postings { get; set; }
        public DbSet<PostingNode> PostingNodes { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> UserAuths { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<View> Views { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 포스팅 및 포스팅 노드 테이블 정의
            modelBuilder.Entity<Posting>(b =>
            {
                b.HasMany(p => p.PostingNodes)
                 .WithOne()
                 .HasForeignKey(pn => pn.PostingId)
                 .HasPrincipalKey(p => p.Id);

                b.HasOne<User>()
                 .WithMany(u => u.Postings)
                 .HasForeignKey(p => p.UserId)
                 .HasPrincipalKey(u => u.Id);
            });

            modelBuilder.Entity<Comment>(b =>
            {
                b.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(c => c.UserId)
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

                b.HasOne<ProductDetail>()
                 .WithOne()
                 .HasForeignKey<ProductDetail>(p => p.ProductId)
                 .HasPrincipalKey<Product>(p => p.Id);
            });

            // 레코드들
            modelBuilder.Entity<Like>(b =>
            {
                b.HasIndex(l => new { l.ParentType, l.ParentId });

                b.HasOne<User>()
                .WithMany(u => u.Liked)
                .HasForeignKey(l => l.UserId)
                .HasPrincipalKey(u => u.Id);
            });

            modelBuilder.Entity<View>(b =>
            {
                b.HasIndex(v => new { v.ParentType, v.ParentId });

                b.HasOne<User>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .HasPrincipalKey(u => u.Id);
            });

            modelBuilder.Entity<User>(b =>
            {
                b.HasOne<UserAuth>()
                 .WithOne()
                 .HasForeignKey<UserAuth>(ua => ua.UserId)
                 .HasPrincipalKey<User>(u => u.Id);
            });

            modelBuilder.Entity<UserAuth>(b =>
            {
                b.HasIndex(ua => new { ua.OAuthProvider, ua.OAuthId });
            });
        }

        public async Task<Feedbackable> GetFeedbackable(FeedbackableType type, long id)
        {
            return type switch
            {
                FeedbackableType.Posting => await Postings
                    .Where(p => p.Id == id)
                    .Include(p => p.PostingNodes.OrderBy(pn => pn.OrderIndex))
                    .Include(p => p.Products)
                    .AsSplitQuery()
                    .FirstAsync(),
                FeedbackableType.Product => await Products
                    .Where(p => p.Id == id)
                    .Include(p => p.Postings)
                        .ThenInclude(p => p.PostingNodes.OrderBy(pn => pn.OrderIndex))
                    .AsSplitQuery()
                    .FirstAsync(),
                FeedbackableType.Comment => await Comments.FindAsync(id),
                FeedbackableType.User => await Users
                    .Where(u => u.Id == id)
                    .Include(u => u.Postings)
                        .ThenInclude(p => p.PostingNodes.OrderBy(pn => pn.OrderIndex))
                    .Include(u => u.Liked)
                    .AsSplitQuery()
                    .FirstAsync(),
                _ => throw new Exception()
            };
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is EntityBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var elem = entityEntry.Entity as EntityBase;

                if (entityEntry.State == EntityState.Added)
                {
                    elem.CreatedDate = DateTime.UtcNow;

                    if (elem is IModifiable)
                        (elem as IModifiable).ModifiedDate = DateTime.UtcNow;

                    if (elem is Feedbackable)
                    {
                        var elemf = elem as Feedbackable;
                        elemf.ViewCount = 0;
                        elemf.CommentCount = 0;
                        elemf.LikeCount = 0;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
