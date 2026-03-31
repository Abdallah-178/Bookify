namespace Bookify.Net.Core.ViewModels
{
    public class AuthorViewModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}
