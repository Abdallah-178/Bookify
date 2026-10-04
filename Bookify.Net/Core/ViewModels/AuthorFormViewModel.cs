namespace Bookify.Net.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int id { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength), Display(Name = "Author")]
        [Remote("AllowItem", null, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string Name { get; set; }



    }
}
