using System.Xml.Serialization;

namespace SimplestMvc5Auth.Identity.Persistence
{
    public class User
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("username")]
        public string UserName { get; set; }

        [XmlElement("passwordHash")]
        public string PasswordHash { get; set; }

        public Identity.User ToUser() => new Identity.User(Id, UserName);
    }
}
