using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace HSPI_Rachio_Irrigation_Plugin.Models
{
    [DataContract]
    class Login : IDisposable
    {
        [DataMember(Name = "loggedIn")]
        public bool loggedIn { get; set; }
        [DataMember(Name = "accessToken")]
        public string accessToken { get; set; }
        [DataMember(Name = "units")]
        public string units { get; set; }
        [DataMember(Name = "updateFrequency")]
        public int updateFrequency { get; set; }
        [DataMember(Name = "loggingLevel")]
        public string loggingLevel { get; set; }
        [DataMember(Name = "ZoneView")]
        public List<bool> ZoneView { get; set; }

        public Login(string apiKey, string unitType, int updateInterval, string loggingType, List<bool> ZoneChecks)
        {
            accessToken = apiKey;
            units = unitType;
            updateFrequency = updateInterval;
            loggingLevel = loggingType;
            loggedIn = true;
            ZoneView = ZoneChecks;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Login()
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

