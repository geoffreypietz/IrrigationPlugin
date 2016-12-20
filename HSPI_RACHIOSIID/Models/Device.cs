using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    [KnownType(typeof(Zone))]
    [KnownType(typeof(Person))]



    public class Device
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "status")]
        public string status { get; set; }
        [DataMember(Name = "zones")]
        public List<Zone> zones { get; set; }
        [DataMember(Name = "timeZone")]
        public string timeZone { get; set; }
        [DataMember(Name = "latitude")]
        public double latitude { get; set; }
        [DataMember(Name = "longitude")]
        public double longitude { get; set; }
        [DataMember(Name = "zip")]
        public string zip { get; set; }
        [DataMember(Name = "name")]
        public string name { get; set; }
        //[DataMember(Name = "scheduleRules")]
        //public Object scheduleRules { get; set; }
        [DataMember(Name = "serialNumber")]
        public string serialNumber { get; set; }
        [DataMember(Name = "macAddress")]
        public string macAddress { get; set; }
        [DataMember(Name = "elevation")]
        public double elevation { get; set; }
        [DataMember(Name = "webhooks")]
        public string webhooks { get; set; }
        [DataMember(Name = "paused")]
        public bool paused { get; set; }
        [DataMember(Name = "on")]
        public bool on { get; set; }
        //[DataMember(Name = "flexScheduleRules")]
        //public Object flexScheduleRules { get; set; }
        [DataMember(Name = "utcOffset")]
        public int utcOffset { get; set; }


    }
}
