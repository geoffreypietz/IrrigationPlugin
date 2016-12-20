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
        //[DataMember(Name = "customNozzle")]
        //public Object customNozzle { get; set; }
        [DataMember(Name = "availableWater")]
        public double availableWater { get; set; }
        [DataMember(Name = "rootZoneDepth")]
        public double rootZoneDepth { get; set; }
        [DataMember(Name = "anagementAllowedDepletion")]
        public double managementAllowedDepletion { get; set; }
        [DataMember(Name = "efficiency")]
        public double efficiency { get; set; }
        [DataMember(Name = "yardAreaSquareFeet")]
        public double yardAreaSquareFeet { get; set; }
        [DataMember(Name = "depthOfWater")]
        public double depthOfWater { get; set; }
        [DataMember(Name = "adjustedManagementAllowedDepletion")]
        public double adjustedManagementAllowedDepletion { get; set; }
        [DataMember(Name = "runtime")]
        public int runtime { get; set; }


    }
}
