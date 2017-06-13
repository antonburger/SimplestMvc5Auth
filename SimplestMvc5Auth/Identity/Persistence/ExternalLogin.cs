using System;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;

namespace SimplestMvc5Auth.Identity.Persistence
{
    public class ExternalLogin
    {
        [XmlElement("provider")]
        public string Provider { get; set; }

        [XmlElement("key")]
        public string Key { get; set; }

        public static ExternalLogin FromUserLoginInfo(UserLoginInfo login)
        {
            return new ExternalLogin
            {
                Provider = login.LoginProvider,
                Key = login.ProviderKey,
            };
        }
    }
}
