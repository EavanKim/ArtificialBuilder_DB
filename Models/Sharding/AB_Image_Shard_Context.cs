using ArtificialBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder.Sharding
{
    /// <summary>저장된 이미지 샤드용 DbContext. saved_images 테이블만 포함.</summary>
    public class AB_Image_Shard_Context : DbContext
    {
        public AB_Image_Shard_Context(DbContextOptions<AB_Image_Shard_Context> _options) : base(_options) { }

        public DbSet<AB_Saved_Image_Model> SavedImages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder _builder)
        {
            _builder.Entity<AB_Saved_Image_Model>(e =>
            {
                e.ToTable("saved_images");
                e.HasKey(m => m.Id_);
            });
        }
    }
}
