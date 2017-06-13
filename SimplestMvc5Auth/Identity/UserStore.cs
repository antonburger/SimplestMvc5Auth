using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using SimplestMvc5Auth.Identity.Persistence;

namespace SimplestMvc5Auth.Identity
{
    public class UserStore : IUserStore<User, int>, IUserPasswordStore<User, int>, IUserLoginStore<User, int>
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

        public async Task AddLoginAsync(User user, UserLoginInfo login)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) return;
                var logins = new List<ExternalLogin>(persistenceUser.ExternalLogins ?? Enumerable.Empty<ExternalLogin>());
                logins.Add(ExternalLogin.FromUserLoginInfo(login));
                persistenceUser.ExternalLogins = logins.ToArray();
                UpdateUserDatabase(stream, db);
            }
        }

        public async Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) return;
                var logins = new List<ExternalLogin>(persistenceUser.ExternalLogins ?? Enumerable.Empty<ExternalLogin>());
                var removedCount = logins.RemoveAll(l => l.Key == login.ProviderKey && l.Provider == login.LoginProvider);
                if (removedCount > 0)
                {
                    UpdateUserDatabase(stream, db);
                }
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var persistenceUser = db.FindUser(user.Id);
                if (persistenceUser == null) return new UserLoginInfo[0];
                var logins = new List<ExternalLogin>(persistenceUser.ExternalLogins ?? Enumerable.Empty<ExternalLogin>());
                return logins.ConvertAll(ConvertPersistenceLoginToLogin);
            }
        }

        public async Task<User> FindAsync(UserLoginInfo login)
        {
            using (var stream = OpenUserDatabase())
            {
                var db = ReadUserDatabase(stream);
                var matchingUsers = from user in db.Users
                                    let logins = user.ExternalLogins ?? Enumerable.Empty<ExternalLogin>()
                                    from externalLogin in logins
                                    where externalLogin.Provider == login.LoginProvider && externalLogin.Key == login.ProviderKey
                                    select user;

                return matchingUsers.FirstOrDefault()?.ToUser();
            }
        }

        private static UserLoginInfo ConvertPersistenceLoginToLogin(ExternalLogin login)
        {
            return new UserLoginInfo(login.Provider, login.Key);
        }
    }
}
