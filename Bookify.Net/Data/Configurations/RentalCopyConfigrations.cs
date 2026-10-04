using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Net.Data.Configurations
{
    public class RentalCopyConfigrations : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {
            builder.HasKey(e => new
            {
                e.RentalId,
                e.bookCopyId
            });
        }
    }


}
