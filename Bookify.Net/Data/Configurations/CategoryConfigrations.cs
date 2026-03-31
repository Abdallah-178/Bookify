using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Net.Data.Configurations
{
    public class CategoryConfigrations : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            //builder.Property(p => p.CreatedOn).HasDefaultValueSql("SYSDATETIME()");
        }
    }


}
