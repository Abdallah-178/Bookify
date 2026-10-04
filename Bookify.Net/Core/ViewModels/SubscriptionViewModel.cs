namespace Bookify.Net.Core.ViewModels
{
    public class SubscriptionViewModel
    {

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string? CreatedById { get; set; }


        public string Status
        {
            get
            {
                return DateTime.Today > EndDate ? "Expired" : DateTime.Today < StartDate ? string.Empty : "Active";
            }
        }
        // if (DateTime.Today > EndDate) return "Expired";
        //if (DateTime.Today<StartDate) return string.Empty;
        //return "Active";
    }
}
