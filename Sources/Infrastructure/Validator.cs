namespace Infrastructure
{
    using System.Text.RegularExpressions;

    public class EmailValidator
    {
        const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                   + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                   + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

        private static readonly Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        public static bool CheckEmail(string email)
        {
            return email != null && regex.IsMatch(email);
        }
    }
}