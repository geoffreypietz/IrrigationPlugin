using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HSPI_Rachio_Irrigation_Plugin.Models
{
    [DataContract]
    [KnownType(typeof(Person))]
    public class ScheduleRule
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "summary")]
        public string summary { get; set; }
        [DataMember(Name = "name")]
        public string name { get; set; }
        [DataMember(Name = "enabled")]
        public bool enabled { get; set; }
        [DataMember(Name = "seasonalAdjustment")]
        public double seasonalAdjustment { get; set; }
    }
}
