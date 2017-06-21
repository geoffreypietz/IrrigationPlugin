using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    [KnownType(typeof(Zone))]
    [KnownType(typeof(Person))]
    [KnownType(typeof(ScheduleRule))]


    public class Device
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "status")]
        public string status { get; set; }
        [DataMember(Name = "zones")]
        public List<Zone> zones { get; set; }
        [DataMember(Name = "name")]
        public string name { get; set; }
        [DataMember(Name = "rainDelayExpirationDate")]
        public double rainDelayExpirationDate { get; set; }
        [DataMember(Name = "scheduleRules")]
        public List<ScheduleRule> scheduleRules { get; set; }
        [DataMember(Name = "flexScheduleRules")]
        public List<ScheduleRule> flexScheduleRules { get; set; }

    }
}
