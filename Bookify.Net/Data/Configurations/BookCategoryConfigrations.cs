using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Net.Data.Configurations
{
    public class BookCategoryConfigrations : IEntityTypeConfiguration<BookCategory>
    {
        public void Configure(EntityTypeBuilder<BookCategory> builder)
        {
            builder.HasKey(e => new
            {
                e.BookId,
                e.CategoryId
            });


        }
    }


}
