using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace HSPI_RACHIOSIID.Models
{
    class RachioConnection
    {
        public string APIKey;
        private string PersonID;
        private string DeviceID;
        private string units;
        public RachioConnection()
        {
            string userPrefs = System.IO.File.ReadAllText(@"Data/hspi_rachiosiid/userprefs.txt");
            Login Login = getLoginInfo(userPrefs);


            if (Login.loggedIn) //POSSIBLE ISSUE
            {
                APIKey = Login.accessToken;
            }

            PersonID = getPersonId().id;
            DeviceID = getPerson().devices[0].id;
            units = Login.units;
            //Person p = getPerson ();

        }
        public static Login getLoginInfo(string json)
        {
            return getJSONAsObject<Login>(json);
        }

        //Helpers
        private HttpWebRequest getRequestWithURLGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);


            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.MediaType = "application/json";
            WebHeaderCollection headers = request.Headers;
            headers.Add(HttpRequestHeader.Authorization, "Bearer " + APIKey);
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            return request;
        }
        private HttpWebRequest getRequestWithURLPut(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.MediaType = "application/json";
            WebHeaderCollection headers = request.Headers;
            headers.Add(HttpRequestHeader.Authorization, "Bearer " + APIKey);
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            return request;
        }

        private string getResponseJson(HttpWebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json;
            var responseStream = response.GetResponseStream();
            var streamReader = new StreamReader(responseStream);
            json = streamReader.ReadToEnd();
            responseStream.Close();
            streamReader.Close();
            response.Close();
            Console.Write(json);
            return json;
        }
        //JSON
        private void addJsonToRequest(HttpWebRequest request, string json)
        {

            byte[] bdata = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = bdata.Length;
            Stream write = request.GetRequestStream();
            write.Write(bdata, 0, bdata.Length);
            write.Close();
        }
        private T getResponseAsObject<T>(HttpWebRequest request)
        {
            WebResponse response = request.GetResponse();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            var stream = response.GetResponseStream();
            object responseObject = serializer.ReadObject(stream);
            response.Close();
            stream.Close();
            T obj = (T)responseObject;
            return obj;
        }
        private static T getJSONAsObject<T>(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            System.IO.Stream stream = new MemoryStream(Encoding.Default.GetBytes(json));
            object responseObject = serializer.ReadObject(stream);
            stream.Close();
            T obj = (T)responseObject;
            return obj;
        }
        //API Methods
        public PersonId getPersonId()
        {
            HttpWebRequest request = getRequestWithURLGet("https://api.rach.io/1/public/person/info");
            return getResponseAsObject<PersonId>(request);
        }
        public void turnOnZoneIDForTime(string zoneID, double duration)
        {
            HttpWebRequest request = getRequestWithURLPut("https://api.rach.io/1/public/zone/start");
            addJsonToRequest(request, "{\"id\" : \"" + zoneID + "\", \"duration\" : " + duration + "}");
            getResponseJson(request);
        }
        public void stopWaterForDevice(string deviceID)
        {
            HttpWebRequest request = getRequestWithURLPut("https://api.rach.io/1/public/device/stop_water");
            addJsonToRequest(request, "{\"id\" : \"" + deviceID + "\"}");
            getResponseJson(request);
        }
        public Person getPerson()
        {
            HttpWebRequest request = getRequestWithURLGet("https://api.rach.io/1/public/person/" + PersonID);
            return getResponseAsObject<Person>(request);
        }
        public Current_Schedule getCurrentSchedule()
        {
            HttpWebRequest request = getRequestWithURLGet("https://api.rach.io/1/public/device/" + DeviceID + "/current_schedule");
            return getResponseAsObject<Current_Schedule>(request);
        }
        public StartEndTime getStartEndTime(string start, string end)
        {
            HttpWebRequest request = getRequestWithURLGet("https://api.rach.io/1/public/device/" + DeviceID + "/event?startTime=" + start + "&endTime=" + end);
            return getResponseAsObject<StartEndTime>(request);
        }
        public CurrentWeather getCurrentWeather()
        {
            HttpWebRequest request = getRequestWithURLGet("https://api.rach.io/1/public/device/" + DeviceID + "/forecast?units=" + units);
            return getResponseAsObject<CurrentWeather>(request);
        }
        //API addons
        public double getTimeRemainingForZone(Zone z)
        {
            Current_Schedule cs = getCurrentSchedule();
            if (cs != null && cs.zoneId != null && cs.zoneId != "" && cs.zoneId == z.id)
            {
                TimeSpan unixTime = DateTime.UtcNow - new DateTime(1970, 1, 1);
                double secondsSinceEpoch = (double)unixTime.TotalSeconds;
                return cs.zoneStartDate / 1000 + cs.duration - secondsSinceEpoch;
            }
            return 0;
        }

        public double getLastWateredForZone(Zone z)
        {
            Current_Schedule cs = getCurrentSchedule();
            if (cs != null && cs.zoneId != null && cs.zoneId != "" && cs.zoneId == z.id)
            {
                Console.WriteLine(cs.zoneStartDate);
                TimeSpan unixTime = DateTime.UtcNow - new DateTime(1970, 1, 1);
                double secondsSinceEpoch = (double)unixTime.TotalSeconds;
                return secondsSinceEpoch - cs.zoneStartDate / 1000 + cs.duration;
            }
            return 0;
        }

        public string getStatusForZone(Zone z)
        {
            double timeRemaining = getTimeRemainingForZone(z) / 60;
            timeRemaining = Math.Round(timeRemaining, 1);
            return (timeRemaining != 0) ? (timeRemaining + " more minutes") : "Off";
        }

        //Converters
        public string getZoneIDForZoneNumberInDevice(int zonenumber, int deviceNumber)
        {

            return getZoneForZoneNumberInDevice(zonenumber, deviceNumber).id;
        }
        public Zone getZoneForZoneNumberInDevice(int zonenumber, int deviceNumber)
        {
            Person p = getPerson();
            foreach (Zone z in p.devices[deviceNumber].zones)
            {
                if (z.zoneNumber == zonenumber)
                {
                    return z;
                }
            }
            return new Zone();
        }

    }
}
