using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    class Login
    {
        [DataMember(Name = "loggedIn")]
        public bool loggedIn { get; set; }
        [DataMember(Name = "username")]
        public string username { get; set; }
        [DataMember(Name = "userId")]
        public string userId { get; set; }
        [DataMember(Name = "apiKey")]
        public string apiKey { get; set; }
        [DataMember(Name = "secretKey")]
        public string secretKey { get; set; }
        [DataMember(Name = "messagingAuthKey")]
        public string messagingAuthKey { get; set; }
        [DataMember(Name = "accessToken")]
        public string accessToken { get; set; }
    }
}

