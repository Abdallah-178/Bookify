namespace Bookify.Net.Core.ViewModels
{
    public class SubscriberViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }

        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; } = null!;
        [MaxLength(15)]
        public string MobileNumber { get; set; } = null!;
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;
        [MaxLength(500)]
        public string custom_img { get; set; } = null!;

        public bool IsBlackListed { get; set; }
        public string? Area { get; set; }

        public string? Governorate { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = null!;

        public IEnumerable<SubscriptionViewModel>? Subscriptions { get; set; } = new List<SubscriptionViewModel>();
        public IEnumerable<RentalViewModel>? Rentals { get; set; } = new List<RentalViewModel>();



    }

}
