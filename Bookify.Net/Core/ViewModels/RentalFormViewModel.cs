namespace Bookify.Net.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public int? Id { get; set; }
        public string SubsecriberKey { get; set; } = null!;
        public IList<int> SelectedCopies { get; set; } = new List<int>(); // IList becase we want to use the index of the selected copy in the view

        public IEnumerable<BookCopyViewModel> CurrentCopies { get; set; } = new List<BookCopyViewModel>();
        public int? MaxAlowedCopies { get; set; } // For Current Copies

    }

}
