using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HSPI_Rachio_Irrigation_Plugin.Models
{
    [DataContract]
    class Forecast
    {
        [DataMember(Name = "time")]
        public double time { get; set; }
        [DataMember(Name = "precipIntensity")]
        public double precipIntensity { get; set; }
        [DataMember(Name = "precipProbability")]
        public double precipProbability { get; set; }
        [DataMember(Name = "precipitation")]
        public double precipitation { get; set; }
        [DataMember(Name = "temperatureMin")]
        public double temperatureMin { get; set; }
        [DataMember(Name = "temperatureMax")]
        public double temperatureMax { get; set; }
        [DataMember(Name = "windSpeed")]
        public double windSpeed { get; set; }
        [DataMember(Name = "humidity")]
        public double humidity { get; set; }
        [DataMember(Name = "cloudCover")]
        public double cloudCover { get; set; }
        [DataMember(Name = "dewPoint")]
        public double dewPoint { get; set; }
        [DataMember(Name = "weatherType")]
        public string weatherType { get; set; }
        [DataMember(Name = "unitType")]
        public string unitType { get; set; }
        [DataMember(Name = "weatherSummary")]
        public string weatherSummary { get; set; }
        [DataMember(Name = "iconUrl")]
        public string iconUrl { get; set; }
    }
}
