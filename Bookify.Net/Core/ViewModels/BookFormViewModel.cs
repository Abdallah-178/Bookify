



namespace Bookify.Net.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int id { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowItem", null, AdditionalFields = "id,AuthorId", ErrorMessage = Errors.Duplicated)]

        public string Title { get; set; } = null!;

        [Remote("AllowItem", null, AdditionalFields = "id,Title", ErrorMessage = Errors.Duplicated)]

        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? Authors { get; set; }


        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;


        [AssertThat("PublishingDate <= Today()", ErrorMessage = Errors.NotAllowedFutureDate)]
        [Display(Name = "Publishing Date")]
        public DateTime PublishingDate { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? custom_img { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is Avilable For Rentel ?")]
        public bool IsAvilableForRentel { get; set; }


        public string Description { get; set; } = null!;


        public IList<int> SelectedCategories { get; set; } = new List<int>();
        public IEnumerable<SelectListItem>? Categories { get; set; } //To Get Items
    }
}
