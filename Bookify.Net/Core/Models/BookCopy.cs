namespace Bookify.Net.Core.Models
{
    public class BookCopy : BaseModel
    {
        public int id { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public bool IsAvailableForRental { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; } // Uniqe

        public ICollection<RentalCopy> Rentals { get; set; } = new List<RentalCopy>();
    }
}
