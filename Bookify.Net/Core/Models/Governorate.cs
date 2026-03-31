namespace Bookify.Net.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Governorate : BaseModel
    {
        public int Id { get; set; }

        public String Name { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }




}
