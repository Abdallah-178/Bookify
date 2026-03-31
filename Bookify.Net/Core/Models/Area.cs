namespace Bookify.Net.Core.Models
{
    [Index(nameof(Name), nameof(Governorateid), IsUnique = true)]
    public class Area : BaseModel
    {
        public int Id { get; set; }

        public String Name { get; set; } = null!;

        public int Governorateid { get; set; }
        public Governorate? Governorate { get; set; }

        internal object Select(Func<object, object> value)
        {
            throw new NotImplementedException();
        }
    }




}
