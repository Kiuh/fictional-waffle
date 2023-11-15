using Common;

namespace Networking
{
    public static class DataValidator
    {
        public static Result ValidateLogin(string login)
        {
            return login switch
            {
                string x when x.Contains(' ') => new FailResult("Login can`t contain space."),
                string x when x.Length < 4 => new FailResult("Login may be at least 4 letters."),
                _ => new SuccessResult()
            };
        }

        public static Result ValidateEmail(string email)
        {
            return email switch
            {
                string x when x.Contains(' ') => new FailResult("Email can`t contain space."),
                string x when x.Length < 4 => new FailResult("Email may be at least 4 letters."),
                _ => new SuccessResult()
            };
        }

        public static Result ValidatePassword(string password)
        {
            return password switch
            {
                string x when x.Contains(' ') => new FailResult("Password can`t contain space."),
                string x when x.Length < 4 => new FailResult("Password may be at least 4 letters."),
                _ => new SuccessResult()
            };
        }
    }
}
