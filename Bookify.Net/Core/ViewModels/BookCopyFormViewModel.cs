namespace Bookify.Net.Core.ViewModels
{
    public class BookCopyFormViewModel
    {
        public int id { get; set; }
        public int BookId { get; set; }
        [Display(Name = "Is Available For Rental ?")]
        public bool IsAvailableForRental { get; set; }

        [Range(1, 1000, ErrorMessage = Errors.InvalidRange)]
        [Display(Name = "Edition Number")]
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; } // Uniqe

        public bool ShowRentalOption { get; set; }


    }
}
