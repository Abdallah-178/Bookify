using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Net.Data.Configurations
{
    public class BookCopyConfigrations : IEntityTypeConfiguration<BookCopy>
    {
        public void Configure(EntityTypeBuilder<BookCopy> builder)
        {
            builder.Property(e => e.SerialNumber)
                   .HasDefaultValueSql("Next Value For Shared.SerialNumber");
        }

    }


}
