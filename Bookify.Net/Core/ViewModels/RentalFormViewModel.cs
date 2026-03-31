namespace Bookify.Net.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public string SubsecriberKey { get; set; } = null!;
        public IList<int> SelectedCopies { get; set; } = new List<int>();
        public int? MaxAlowedCopies { get; set; } // For Current Copies

    }

}
