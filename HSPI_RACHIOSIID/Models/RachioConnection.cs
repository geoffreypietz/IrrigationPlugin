using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HSPI_RACHIOSIID.Models
{
    class RachioConnection : IDisposable
    {
        public string APIKey;
        private string PersonID;
        public string DeviceID;
        public string units;
        public List<bool> ZoneView;
        public RachioConnection()
        {
            string json = Util.hs.GetINISetting("RACHIO", "login", "", Util.IFACE_NAME + ".ini");
            using (Login Login = getLoginInfo(json))
            {
                if (Login != null)
                {
                    if (Login.loggedIn) //POSSIBLE ISSUE
                    {
                        APIKey = Login.accessToken;
                    }

                    PersonID = getPersonId().id;
                    units = Login.units;
                    ZoneView = new List<bool>();
                    ZoneView = Login.ZoneView;
                }
            }
        }

        public static Login getLoginInfo(string json)
        {
            return JsonConvert.DeserializeObject<Login>(json);
        }

        public bool HasAccessToken()
        {
            return !string.IsNullOrEmpty(APIKey);
        }

        //Request
        private IRestResponse getRequestGetOrPut(Method method, string json, RestClient client)
        {
            var request = new RestRequest(method);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("authorization", "Bearer " + APIKey);
            request.AddHeader("content-type", "text/json");
            if (json != null)
            {
                request.AddParameter("text/json", json, ParameterType.RequestBody);
            }

            IRestResponse initial_response = client.Execute(request);
            try
            {
              var APIcallsLeft=  initial_response.Headers.Where(i => i.Name.ToLower()== "x-ratelimit-remaining").ToList()[0];
                Util.Log("RACHIOSIID "+ APIcallsLeft.Value.ToString()+" API calls remaining for the day", Util.LogType.LOG_TYPE_INFO);

            }
            catch (Exception e)
            {
                Util.Log(e.Message,Util.LogType.LOG_TYPE_ERROR);

            }




            return initial_response;
        }

        // GET
        public PersonId getPersonId()
        {

            if (HSPI.personId == null)
            {
                var client = new RestClient("https://api.rach.io/1/public/person/info");
                client.FollowRedirects = false;
              //  Util.Log("personID", Util.LogType.LOG_TYPE_WARNING);

                IRestResponse initial_response = getRequestGetOrPut(Method.GET, null, client);
                HSPI.personId = JsonConvert.DeserializeObject<PersonId>(initial_response.Content);
            }


            return HSPI.personId;
        }
        public Person getPerson()
        {
            if (HSPI.person == null)
            {
                var client = new RestClient("https://api.rach.io/1/public/person/" + PersonID);
                client.FollowRedirects = false;
               // Util.Log("person", Util.LogType.LOG_TYPE_WARNING);
                var initial_response = getRequestGetOrPut(Method.GET, null, client);
               
                HSPI.person = JsonConvert.DeserializeObject<Person>(initial_response.Content);
            }

            return HSPI.person;
        }
        public Current_Schedule getCurrentSchedule()
        {
            var client = new RestClient("https://api.rach.io/1/public/device/" + DeviceID + "/current_schedule");
            client.FollowRedirects = false;
          //  Util.Log("schedule", Util.LogType.LOG_TYPE_WARNING);
            IRestResponse initial_response = getRequestGetOrPut(Method.GET, null, client);
         
            return JsonConvert.DeserializeObject<Current_Schedule>(initial_response.Content);
        }
        public TotalForecast getTotalForecast()
        {
            var client = new RestClient("https://api.rach.io/1/public/device/" + DeviceID + "/forecast?units=" + units);
            client.FollowRedirects = false;
           // Util.Log("forcast", Util.LogType.LOG_TYPE_WARNING);
            IRestResponse initial_response = getRequestGetOrPut(Method.GET, null, client);

            return JsonConvert.DeserializeObject<TotalForecast>(initial_response.Content);
        }

        // PUT
        public void setApiJson(string json, string urlAddon)
        {
            var client = new RestClient("https://api.rach.io/1/public/" + urlAddon);
            client.FollowRedirects = false;
     
            IRestResponse initial_response = getRequestGetOrPut(Method.PUT, json, client);
        }

        // Disposable Interface
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RachioConnection()
        {
            Debug.Assert(Disposed, "WARNING: Object finalized without being disposed!");
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }

                DisposeUnmanagedResources();
                Disposed = true;
            }
        }

        protected virtual void DisposeManagedResources() { }
        protected virtual void DisposeUnmanagedResources() { }
    }
}
