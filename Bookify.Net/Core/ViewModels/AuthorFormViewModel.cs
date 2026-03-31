namespace Bookify.Net.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int id { get; set; }

        [MaxLength(50, ErrorMessage = "Max Length Cannet Be More Than 50 Character.")]
        [Remote("AllowItem", null, AdditionalFields = "Id", ErrorMessage = "Author Name Have the Same Name Is Already Exists !")]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; }



    }
}
