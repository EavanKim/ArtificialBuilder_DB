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
            void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AB_Saved_Image_Model> _e)
            {
                _e.ToTable("saved_images");
                _e.HasKey(m => m.Id_);
            }
            _builder.Entity<AB_Saved_Image_Model>(Configure);
        }
    }
}
