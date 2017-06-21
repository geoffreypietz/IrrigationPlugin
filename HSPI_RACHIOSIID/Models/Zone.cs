using System;
using System.Runtime.Serialization;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    [KnownType(typeof(Person))]
    [KnownType(typeof(Device))]
    public class Zone
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "zoneNumber")]
        public int zoneNumber { get; set; }
        [DataMember(Name = "name")]
        public string name { get; set; }
        [DataMember(Name = "enabled")]
        public bool enabled { get; set; }
        [DataMember(Name = "imageUrl")]
        public string imageUrl { get; set; }
        [DataMember(Name = "lastWateredDuration")]
        public double lastWateredDuration { get; set; }
        [DataMember(Name = "lastWateredDate")]
        public double lastWateredDate { get; set; }
        [DataMember(Name = "maxRuntime")]
        public double maxRuntime { get; set; }
        [DataMember(Name = "runtime")]
        public double runtime { get; set; }

    }
}
