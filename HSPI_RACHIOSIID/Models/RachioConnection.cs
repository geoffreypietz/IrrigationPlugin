using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        private RestRequest getRequestGetOrPut(Method method, string json)
        {
            var request = new RestRequest(method);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("authorization", "Bearer " + APIKey);
            request.AddHeader("content-type", "text/json");
            if (json != null)
            {
                request.AddParameter("text/json", json, ParameterType.RequestBody);
            }
            return request;
        }

        // GET
        public PersonId getPersonId()
        {
            var client = new RestClient("https://api.rach.io/1/public/person/info");
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.GET, null);
            IRestResponse initial_response = client.Execute(request);

            return JsonConvert.DeserializeObject<PersonId>(initial_response.Content);
        }
        public Person getPerson()
        {
            var client = new RestClient("https://api.rach.io/1/public/person/" + PersonID);
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.GET, null);
            IRestResponse initial_response = client.Execute(request);

            return JsonConvert.DeserializeObject<Person>(initial_response.Content);
        }
        public Current_Schedule getCurrentSchedule()
        {
            var client = new RestClient("https://api.rach.io/1/public/device/" + DeviceID + "/current_schedule");
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.GET, null);
            IRestResponse initial_response = client.Execute(request);

            return JsonConvert.DeserializeObject<Current_Schedule>(initial_response.Content);
        }
        public TotalForecast getTotalForecast()
        {
            var client = new RestClient("https://api.rach.io/1/public/device/" + DeviceID + "/forecast?units=" + units);
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.GET, null);
            IRestResponse initial_response = client.Execute(request);

            return JsonConvert.DeserializeObject<TotalForecast>(initial_response.Content);
        }

        // PUT
        public void setApiJson(string json, string urlAddon)
        {
            var client = new RestClient("https://api.rach.io/1/public/" + urlAddon);
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.PUT, json);
            IRestResponse initial_response = client.Execute(request);
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
