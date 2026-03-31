namespace Bookify.Net.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Author : BaseModel
    {
        public int id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
    }
}
