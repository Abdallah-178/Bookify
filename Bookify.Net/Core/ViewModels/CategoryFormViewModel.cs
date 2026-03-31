namespace Bookify.Net.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength), Display(Name = "Category")]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        [Remote("AllowItem", null, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; }
    }
}
