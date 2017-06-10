using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SimplestMvc5Auth.Identity;

namespace SimplestMvc5Auth
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        public async Task<User> AuthenticateAsync(UserManager userManager)
        {
            var user = await userManager.FindByNameAsync(UserName).ConfigureAwait(false);
            if (user == null) return null;
            var passwordCheck = await userManager.CheckPasswordAsync(user, Password);
            return passwordCheck
                ? user
                : null;
        }
    }
}
