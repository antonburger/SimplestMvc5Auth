using Microsoft.AspNet.Identity;

namespace SimplestMvc5Auth.Identity
{
    public class UserManager : UserManager<User, int>
    {
        public UserManager(IUserStore<User, int> store) : base(store)
        {
        }
    }
}
