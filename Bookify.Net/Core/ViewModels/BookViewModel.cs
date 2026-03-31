namespace Bookify.Net.Core.ViewModels
{
    public class BookViewModel
    {
        public int id { get; set; }

        public string Title { get; set; } = null!;

        public string Publisher { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }

        public string? custom_img { get; set; }

        public string Hall { get; set; } = null!;

        public bool IsAvilableForRentel { get; set; }

        public string Description { get; set; } = null!;

        public string Author { get; set; } = null!;    ///

        public IEnumerable<string> Categories { get; set; } = null!;

        public IEnumerable<BookCopyViewModel> Copies { get; set; } = null!;

    }
}
