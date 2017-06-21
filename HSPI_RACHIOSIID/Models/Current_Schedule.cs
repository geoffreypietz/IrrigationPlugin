using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    public class Current_Schedule : IDisposable
    {
        [DataMember(Name = "deviceId")]
        public string deviceId { get; set; }

        [DataMember(Name = "status")]
        public string status { get; set; }

        [DataMember(Name = "duration")]
        public int duration { get; set; }

        [DataMember(Name = "zoneId")]
        public string zoneId { get; set; }

        [DataMember(Name = "zoneStartDate")]
        public long zoneStartDate { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Current_Schedule()
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
