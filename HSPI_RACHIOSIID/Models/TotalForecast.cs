using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HSPI_RACHIOSIID.Models
{
    [DataContract]
    [KnownType(typeof(CurrentWeather))]
    [KnownType(typeof(Forecast))]
    class TotalForecast : IDisposable
    {
        [DataMember(Name = "current")]
        public CurrentWeather currentW { get; set; }
        [DataMember(Name = "forecast")]
        public List<Forecast> forecastList { get; set; }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TotalForecast()
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
