using Microsoft.AspNet.Identity;

namespace SimplestMvc5Auth.Identity
{
    public class User : IUser<int>
    {
        public User(int id, string userName)
        {
            this.Id = id;
            this.UserName = userName;
        }

        public int Id
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }
    }
}
