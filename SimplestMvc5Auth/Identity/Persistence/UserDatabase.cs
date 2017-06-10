using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SimplestMvc5Auth.Identity.Persistence
{
    [XmlRoot("userStore")]
    public class UserDatabase
    {
        [XmlArray("users")]
        [XmlArrayItem("user")]
        public List<User> Users { get; set; }

        public int AddUser(Identity.User user)
        {
            var persistenceUser = new User
            {
                Id = ((from existingUser in Users select (int?)existingUser.Id).Max() ?? 0) + 1,
                UserName = user.UserName,
            };

            Users.Add(persistenceUser);

            return persistenceUser.Id;
        }

        public User FindUser(int userId)
        {
            return Users.SingleOrDefault(u => u.Id == userId);
        }

        public User FindUser(string userName)
        {
            return Users.FirstOrDefault(u => u.UserName == userName);
        }
    }
}
