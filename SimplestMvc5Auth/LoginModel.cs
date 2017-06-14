namespace SimplestMvc5Auth
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        public bool HasValidUsernameAndPassword
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
            }
        }
    }
}
