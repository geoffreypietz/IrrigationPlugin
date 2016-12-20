using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    public class Current_Schedule
    {
        [DataMember(Name = "deviceId")]
        public string deviceId { get; set; }

        [DataMember(Name = "scheduleId")]
        public string scheduleId { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "status")]
        public string status { get; set; }

        [DataMember(Name = "startDate")]
        public long startDate { get; set; }

        [DataMember(Name = "duration")]
        public int duration { get; set; }

        [DataMember(Name = "zoneId")]
        public string zoneId { get; set; }

        [DataMember(Name = "zoneStartDate")]
        public long zoneStartDate { get; set; }

        [DataMember(Name = "zoneDuration")]
        public int zoneDuration { get; set; }

        [DataMember(Name = "cycleCount")]
        public int cycleCount { get; set; }

        [DataMember(Name = "totalCycleCount")]
        public int totalCycleCount { get; set; }

        [DataMember(Name = "cycling")]
        public bool cycling { get; set; }

        [DataMember(Name = "durationNoCycle")]
        public int durationNoCycle { get; set; }

    }
}
