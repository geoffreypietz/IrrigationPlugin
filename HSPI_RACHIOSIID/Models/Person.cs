using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace HSPI_Rachio_Irrigation_Plugin.Models
{
    [DataContract]
    public class Person : IDisposable
    {
        [DataMember(Name = "id")]
        public string id { get; set; }
        [DataMember(Name = "devices")]
        public List<Device> devices { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Person()
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

