using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using SimplestMvc5Auth.Identity.Persistence;

namespace SimplestMvc5Auth.Identity
{
    public class UserStore : IUserStore<User, int>, IUserPasswordStore<User, int>
    {
        public async Task CreateAsync(User user)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                int userId = db.AddUser(user);
                user.Id = userId;

                UpdateUserDatabase(stream, db);
            }
        }

        public async Task DeleteAsync(User user)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (user != null)
                {
                    db.Users.Remove(persistenceUser);

                    UpdateUserDatabase(stream, db);
                }
            }
        }

        public void Dispose()
        {
        }

        public async Task<User> FindByIdAsync(int userId)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(userId);
                return persistenceUser == null
                    ? null
                    : persistenceUser.ToUser();
            }
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(userName);
                return persistenceUser == null
                    ? null
                    : persistenceUser.ToUser();
            }
        }

        public async Task<string> GetPasswordHashAsync(User user)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) throw new Exception("User doesn't exist.");
                return persistenceUser.PasswordHash;
            }
        }

        public async Task<bool> HasPasswordAsync(User user)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) throw new Exception("User doesn't exist.");
                return !string.IsNullOrEmpty(persistenceUser.PasswordHash);
            }
        }

        public async Task SetPasswordHashAsync(User user, string passwordHash)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) throw new Exception("User doesn't exist.");
                persistenceUser.PasswordHash = passwordHash;
                UpdateUserDatabase(stream, db);
            }
        }

        public async Task UpdateAsync(User user)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) throw new Exception("User doesn't exist.");
                persistenceUser.UserName = user.UserName;
                UpdateUserDatabase(stream, db);
            }
        }

        private static FileStream OpenUserDatabase()
        {
            return new FileStream(HostingEnvironment.MapPath("~/userdb.xml"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        private static UserDatabase ReadUserDatabase(FileStream stream)
        {
            var serializer = new XmlSerializer(typeof(UserDatabase));
            return (UserDatabase)serializer.Deserialize(stream);
        }

        private void UpdateUserDatabase(FileStream stream, UserDatabase database)
        {
            stream.SetLength(0L);
            var serializer = new XmlSerializer(typeof(UserDatabase));
            serializer.Serialize(stream, database);
        }
    }
}
