namespace Bookify.Net.Core.Const
{
    public static class Errors
    {
        public const string RequiredField = "Required Filed";
        public const string MaxLength = "Length Connot Be More Than {1} Character";
        public const string Duplicated = "Another Record With the Same {0} Is Already Exists !";

        public const string NotAllowedExtensions = "Only .png , .jpg , .jpeg Files Are Allowed !";
        public const string MaxSize = "File Cannot Be More That 2 Mb!";
        public const string NotAllowedFutureDate = "Date Cannot Be In The Future !";

        public const string InvalidRange = "{0} Should Be Between {1} to {2}";

        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string ConfirmPassword = "The password and confirmation password do not match.";
        public const string WeakPassword = "that passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long.";
        public const string InvalidUserName = "UserName Can Only Contain Letters Or Digits Without !@#$%^&*()";

        public const string OnlyEnglishLetters = "Only English Letters are Allowed .";
        public const string OnlyArabicLetters = "Only Arabic Letters are Allowed .";
        public const string OnlyNumbersAndLetters = "Only Arabic/English Letters Or Digits are Allowed .";
        public const string InvalidMobileNumber = "Invalid Mobile Number .";

        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidNationalId = "Invalid national ID.";
        public const string EmptyImage = "Please select an image.";
        public const string InvalidFolderPath = "The specified folder path is invalid or not allowed.";

        public const string InvalidSerailNumber = "Invalid Serail Number !";
        public const string NotAvalibleForRental = "This Book /Copy Is Not Available For Rental .";

        public const string BlackListedSubscriber = "This Subsecriper is Black Listed";
        public const string InActiveSubscriber = "This Subsecriper is InActive";
        public const string MaxCopiesReached = "This Subsecriper has Reached the Max Number For Rental .";
        public const string CopyIsInRental = "This Copy is Already in Rentaled";


    }
}
