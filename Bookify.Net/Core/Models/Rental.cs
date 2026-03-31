namespace Bookify.Net.Core.Models
{
    public class Rental : BaseModel
    {
        public int Id { get; set; }

        public int SubsecriberId { get; set; }
        public Subsecriber Subsecriber { get; set; }

        public DateTime StratDate { get; set; } = DateTime.Today;

        public bool PanaltyPaid { get; set; }

        public ICollection<RentalCopy> RentalCopies { get; set; } = new List<RentalCopy>();

    }
}
