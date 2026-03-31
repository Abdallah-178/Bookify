namespace Bookify.Net.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100), Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();




    }
}
