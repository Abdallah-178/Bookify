namespace Bookify.Net.Helpers
{
    public class PhoneHelper
    {
        public static (string Primary, string Fallback) ToInternationalPalestine(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return (phone, phone);

            phone = phone
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("+", "");

            var local = phone switch
            {
                _ when phone.StartsWith("972") => phone.Substring(3),
                _ when phone.StartsWith("970") => phone.Substring(3),
                _ when phone.StartsWith("0") => phone.Substring(1),
                _ => phone
            };

            return ($"+972{local}", $"+970{local}");
        }
    }
}
