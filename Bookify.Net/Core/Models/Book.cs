namespace Bookify.Net.Core.Models
{
    [Index(nameof(Title), nameof(AuthorId), IsUnique = true)]
    public class Book : BaseModel
    {
        public int id { get; set; }

        [MaxLength(50)]
        public string Title { get; set; } = null!;

        [MaxLength(100)]
        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }

        public string? custom_img { get; set; }

        public string? ImagePublicId { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = null!;


        public bool IsAvilableForRentel { get; set; }


        public string Description { get; set; } = null!;


        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>();
        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();




    }
}
