namespace Bookify.Net.Core.Models
{
    public class RentalCopy
    {
        public int RentalId { get; set; }
        public Rental Rental { get; set; }

        public int bookCopyId { get; set; }
        public BookCopy? bookCopy { get; set; }

        public DateTime RentalDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration); // 7 Days
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }

    }
}
