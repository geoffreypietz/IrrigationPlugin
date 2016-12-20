using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    [KnownType(typeof(Zone))]
    [KnownType(typeof(Device))]
    public class Person
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "username")]
        public string username { get; set; }
        [DataMember(Name = "fullName")]
        public string fullName { get; set; }
        [DataMember(Name = "email")]
        public string email { get; set; }
        [DataMember(Name = "devices")]
        public List<Device> devices { get; set; }
        [DataMember(Name = "enabled")]
        public bool enabled { get; set; }

        public const string STATUS_ONLINE = "ONLINE";
    }

}

