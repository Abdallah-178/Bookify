namespace Bookify.Net.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public string? Key { get; set; }

        [Display(Name = "First Name"), MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public String FirstName { get; set; } = null!;

        [Display(Name = "Last Name"), MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public String LastName { get; set; } = null!;

        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowedFutureDate)]
        [Display(Name = "Date Of Birth")]

        public DateTime DateOfBirth { get; set; }

        [Display(Name = "National Id"), MaxLength(10, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.NationalId, ErrorMessage = Errors.InvalidNationalId)]
        [Remote("AllowItemNationalId", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string NationalId { get; set; } = null!;

        [MaxLength(11)]
        [Display(Name = "Mobile Number")]
        [RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
        [Remote("AllowItemMobileNumber", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string MobileNumber { get; set; } = null!;
        [EmailAddress]
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]

        [Remote("AllowItemEmail", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [Display(Name = "Has Whats App ?")]
        public bool HasWhatsApp { get; set; }

        [RequiredIf("Key == ''", ErrorMessage = Errors.EmptyImage)] // Img Is required when you create
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? custom_img { get; set; }

        [Display(Name = "Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? Areas { get; set; } = new List<SelectListItem>();

        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? Governorates { get; set; } = new List<SelectListItem>();

        [MaxLength(500)]
        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; }





    }
}
