using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    class CurrentWeather
    {
        [DataMember(Name = "current")]
        public string current { get; set; }
        [DataMember(Name = "time")]
        public double time { get; set; }
        [DataMember(Name = "precipIntensity")]
        public double precipIntensity { get; set; }
        [DataMember(Name = "precipProbability")]
        public double precipProbability { get; set; }
        [DataMember(Name = "precipitation")]
        public double precipitation { get; set; }
        [DataMember(Name = "windSpeed")]
        public int windSpeed { get; set; }
        [DataMember(Name = "humidity")]
        public double humidity { get; set; }
        [DataMember(Name = "cloudCover")]
        public double cloudCover { get; set; }
        [DataMember(Name = "dewPoint")]
        public int dewPoint { get; set; }
        [DataMember(Name = "weatherType")]
        public string weatherType { get; set; }
        [DataMember(Name = "unitType")]
        public string unitType { get; set; }
        [DataMember(Name = "currentTemperature")]
        public int currentTemperature { get; set; }
        [DataMember(Name = "weatherSummary")]
        public string weatherSummary { get; set; }
        [DataMember(Name = "dailyWeatherType")]
        public string dailyWeatherType { get; set; }
        [DataMember(Name = "prettyTime")]
        public string prettyTime { get; set; }
        [DataMember(Name = "forecast")]
        public List<Forecast> forecastList { get; set; }
    }
}
