namespace Bookify.Net.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }


        public SubscriberViewModel? Subsecriber { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime CreatedOn { get; set; }
        public bool PanaltyPaid { get; set; }

        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = new List<RentalCopyViewModel>();

        public int TotalDelayInDays
        {
            get
            {
                return RentalCopies.Sum(x => x.DalayInDays);
            }

        }
        public int NumberOfCopies
        {
            get
            {
                return RentalCopies.Count();
            }

        }

    }

}
