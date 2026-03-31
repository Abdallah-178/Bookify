namespace Bookify.Net.Core.ViewModels
{
    public class ResetPasswordFormViewModel
    {

        public string? Id { get; set; }


        [DataType(DataType.Password),
         StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
         RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]

        public string? Password { get; set; } = null!;

        [DataType(DataType.Password), Display(Name = "Confirm password"),
         Compare("Password", ErrorMessage = Errors.ConfirmPassword)]
        public string? ConfirmPassword { get; set; } = null!;
    }
}
