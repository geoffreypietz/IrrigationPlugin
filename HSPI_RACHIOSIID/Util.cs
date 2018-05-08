using System;
using System.Collections;
using System.Collections.Generic;
using HomeSeerAPI;
using Scheduler.Classes;
using HSPI_Rachio_Irrigation_Plugin.Models;
using System.Linq;

namespace HSPI_Rachio_Irrigation_Plugin
{

    static class Util
    {

        // interface status
        // for InterfaceStatus function call
        public const int ERR_NONE = 0;
        public const int ERR_SEND = 1;
        public const int ERR_INIT = 2;
        public static HomeSeerAPI.IHSApplication hs;
        public static HomeSeerAPI.IAppCallbackAPI callback;
        public const string IFACE_NAME = "Rachio Irrigation Plugin";
        // set when SupportMultipleInstances is TRUE
        public static string Instance = "";
        public static string gEXEPath = "";
        public static bool gGlobalTempScaleF = true;
        public static SortedList colTrigs_Sync;
        public static SortedList colTrigs;
        public static SortedList colActs_Sync;
        public static SortedList colActs;

        public static bool StringIsNullOrEmpty(ref string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            return string.IsNullOrEmpty(s.Trim());
        }

        public enum LogType
        {
            LOG_TYPE_INFO = 0,
            LOG_TYPE_ERROR = 1,
            LOG_TYPE_WARNING = 2
        }

        public static void Log(string msg, LogType logType)
        {
            try
            {
                if (msg == null)
                    msg = "";
                if (!Enum.IsDefined(typeof(LogType), logType))
                {
                    logType = Util.LogType.LOG_TYPE_ERROR;
                }
                Console.WriteLine(msg);
                switch (logType)
                {
                    case LogType.LOG_TYPE_ERROR:
                        hs.WriteLog(Util.IFACE_NAME + " Error", msg);
                        break;
                    case LogType.LOG_TYPE_WARNING:
                        hs.WriteLog(Util.IFACE_NAME + " Warning", msg);
                        break;
                    case LogType.LOG_TYPE_INFO:
                        hs.WriteLog(Util.IFACE_NAME, msg);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in LOG of " + Util.IFACE_NAME + ": " + ex.Message);
            }

        }


        public static int MyDevice = -1;

        public static int MyTempDevice = -1;





        static internal List<DeviceDataPoint> Get_Device_List(List<DeviceDataPoint> deviceList)
        {
            DeviceClass dv = new DeviceClass();
            // Gets relevant devices from HomeSeer
            try
            {
                Scheduler.Classes.clsDeviceEnumeration EN = default(Scheduler.Classes.clsDeviceEnumeration);
                EN = (Scheduler.Classes.clsDeviceEnumeration)Util.hs.GetDeviceEnumerator();
                if (EN == null)
                    throw new Exception(IFACE_NAME + " failed to get a device enumerator from HomeSeer.");
                int dvRef;
                
                do
                {
                    dv = EN.GetNext();
                    if (dv == null)
                        continue;
                    if (dv.get_Interface(null) != IFACE_NAME)
                        continue;
                    dvRef = dv.get_Ref(null);

                    var ddp = new DeviceDataPoint(dvRef, dv);
                    deviceList.Add(ddp);

                } while (!(EN.Finished));
            }
            catch (Exception ex)
            {
                Log("Exception in Get_Device_List: " + ex.Message, LogType.LOG_TYPE_ERROR);
            }

            return deviceList;
        }

        static internal void Update_RachioDevice(DeviceDataPoint ddPoint, Device rachioDevice, RachioConnection rachio, Current_Schedule current)
        {
            string name;
            string type;
            string id = GetDeviceKeys(ddPoint.device, out name, out type);

            switch (name)
            {
                case "Rain Delay":
                    var delay = rachioDevice.rainDelayExpirationDate / 1000 - getNowSinceEpoch();
                    hs.SetDeviceValueByRef(ddPoint.dvRef, Math.Round(delay / 3600 / 24, 1), true);
                    break;
                case "Status":
                    if (rachioDevice.status == "ONLINE")
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                    }
                    else
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                    }
                    break;
                case "Watering State":
                    if (current.status != null)
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, 2, true);
                    }
                    else
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                    }
                    break;
            }
        }

        static internal void Update_Zone(DeviceDataPoint ddPoint, Zone z, RachioConnection rachio, Current_Schedule current)
        {
            string name;
            string type;
            string id = GetDeviceKeys(ddPoint.device, out name, out type);

            if (rachio.ZoneView==null || rachio.ZoneView[z.zoneNumber - 1])
            {
                hs.DeviceProperty_dvMISC(ddPoint.dvRef, Enums.eDeviceProperty.MISC_Clear, Enums.dvMISC.HIDDEN);
                //dv.MISC_Clear(hs, Enums.dvMISC.HIDDEN);
            }
            else
            {
                hs.DeviceProperty_dvMISC(ddPoint.dvRef, Enums.eDeviceProperty.MISC_Set, Enums.dvMISC.HIDDEN);
                //dv.MISC_Set(hs, Enums.dvMISC.HIDDEN);
            }

            switch (name)
            {
                case "Control":
                    if (current.zoneId != null)
                    {
                        if (current.zoneId == id)   // If the time On is greater than zero
                        {

                            var timeRemaining = current.zoneStartDate / 1000 + current.duration - getNowSinceEpoch();

                            if (timeRemaining > 0)
                            {
                                hs.SetDeviceValueByRef(ddPoint.dvRef, Math.Round(timeRemaining / 60, 1), true); // Set Control Device to On 
                            }
                            else
                            {
                                hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true); // Set Control Device to Off
                            }
                        }                      
                    }
                    else
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true); // Set Control Device to Off
                    }
                    break;
                case "Last Duration":
                    hs.SetDeviceValueByRef(ddPoint.dvRef, Math.Round(z.lastWateredDuration / 60), true);
                    break;
                case "Last Watered":
                    hs.SetDeviceValueByRef(ddPoint.dvRef, Math.Round((getNowSinceEpoch() - z.lastWateredDate / 1000) / 24 / 3600), true);
                    break;
                case "Max Runtime":
                    hs.SetDeviceValueByRef(ddPoint.dvRef, z.maxRuntime / 60, true);
                    break;
                case "Total Runtime":
                    hs.SetDeviceValueByRef(ddPoint.dvRef, Math.Round(z.runtime / 60, 1), true);
                    break;
                case "Zone Image":
                    hs.SetDeviceString(ddPoint.dvRef, z.imageUrl, true);
                    break;
            }
        }

        static internal void Update_Weather(DeviceDataPoint ddPoint, CurrentWeather currentW, Forecast forecast, RachioConnection rachio)
        {
            string name;
            string type;
            string id = GetDeviceKeys(ddPoint.device, out name, out type);

            switch (name)
            {
                case "Cloud Cover":
                    {
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.cloudCover * 100, true);

                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.cloudCover * 100, true);
                        }
                        break;
                    }

                case "Dew Point":
                    {
                        ddPoint.device.set_ScaleText(hs, getTemperatureUnits(rachio.units));
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.dewPoint, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.dewPoint, true);
                        }
                        break;
                    }
                case "Humidity":
                    {
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.humidity * 100, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.humidity * 100, true);
                        }
                        break;
                    }
                case "Precip Intensity":
                    {
                        ddPoint.device.set_ScaleText(hs, getDepthUnits(rachio.units) + "/hr");
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.precipIntensity, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.precipIntensity, true);
                        }
                        break;
                    }
                case "Precip Probability":
                    {
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.precipProbability * 100, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.precipProbability * 100, true);
                        }
                        break;
                    }
                case "Precipitation":
                    {
                        ddPoint.device.set_ScaleText(hs, getDepthUnits(rachio.units));
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.precipitation, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.precipitation, true);
                        }
                        break;
                    }
                case "Temperature":
                    {
                        ddPoint.device.set_ScaleText(hs, getTemperatureUnits(rachio.units));
                        hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.currentTemperature, true);
                        break;
                    }
                case "TemperatureMax":
                    {
                        ddPoint.device.set_ScaleText(hs, getTemperatureUnits(rachio.units));
                        hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.temperatureMax, true);
                        break;
                    }
                case "TemperatureMin":
                    {
                        ddPoint.device.set_ScaleText(hs, getTemperatureUnits(rachio.units));
                        hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.temperatureMin, true);
                        break;
                    }
                case "Weather Conditions":
                    {
                        if (currentW != null)
                        {
                            hs.SetDeviceString(ddPoint.dvRef, currentW.weatherSummary.ToString(), true);
                        }
                        else
                        {
                            hs.SetDeviceString(ddPoint.dvRef, forecast.weatherSummary.ToString(), true);
                        }
                        break;
                    }
                case "Weather Update":
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, getTimeSince(currentW.time), true);
                        break;
                    }
                case "Wind Speed":
                    {
                        ddPoint.device.set_ScaleText(hs, getSpeedUnits(rachio.units));
                        if (currentW != null)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, currentW.windSpeed, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, forecast.windSpeed, true);
                        }
                        break;
                    }
                case "Icon Url":
                    {
                        if (currentW != null)
                        {
                            hs.SetDeviceString(ddPoint.dvRef, currentW.iconUrl, true);
                        }
                        else
                        {
                            hs.SetDeviceString(ddPoint.dvRef, forecast.iconUrl, true);
                        }
                        break;
                    }
            }
        }

        static internal void Update_ScheduleRule(DeviceDataPoint ddPoint, ScheduleRule schedule, RachioConnection rachio)
        {
            string name;
            string type;
            string id = GetDeviceKeys(ddPoint.device, out name, out type);

            switch (name)
            {
                case "Seasonal Adjustment":
                    {
                        hs.SetDeviceValueByRef(ddPoint.dvRef, schedule.seasonalAdjustment, true);
                        break;
                    }
                case "Summary":
                    {
                        hs.SetDeviceString(ddPoint.dvRef, schedule.summary, true);
                        break;
                    }
                case "Status":
                    {
                        if (schedule.enabled)
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 1, true);
                        }
                        else
                        {
                            hs.SetDeviceValueByRef(ddPoint.dvRef, 0, true);
                        }
                        break;
                    }
            }
        }

        // Initial Method for updating/creating HomeSeer Devices based on Rachio API Data
        static internal void Find_Create_Devices(RachioConnection rachio)
        {
            var setAssociate = false;
            var deviceList = new List<DeviceDataPoint>();

            deviceList = Get_Device_List(deviceList);

            // Person p is the primary data object and contains device data
            using (var p = rachio.getPerson())
            {
                rachio.DeviceID = p.devices[0].id;
                // Current_Schedule current is another data object holding data about the currently running zones
                using (var current = rachio.getCurrentSchedule())
                {

                    if (Find_Create_RachioDevices(p, deviceList, rachio, current))
                    {
                        setAssociate = true;    // Set to true if a HomeSeer device is created
                    }

                    if (Find_Create_Zones(p, deviceList, rachio, current))
                    {
                        setAssociate = true;
                    }
                }

            /*    if (Find_Create_Weather(p, deviceList, rachio)) //Remove RACHIO weather devices 4/27/2018
                {
                    setAssociate = true;
                }*/

                if (Find_Create_ScheduleRules(p, deviceList, rachio))
                {
                    setAssociate = true;
                }

                // Set associated devices to "Root" device
                if (setAssociate)
                {
                    SetAssociatedDevices();
                }
            }
        }

        // Rachio Device
        static internal bool Find_Create_RachioDevices(Person p, List<DeviceDataPoint> deviceList, RachioConnection rachio, Current_Schedule current)
        {
            Device rachioDevice = p.devices[0]; // TODO: account for more than one Rachio Device
            bool associates = false;
            List<string> dStrings = getDeviceStrings();

            foreach (var dString in dStrings)
            {
                if (RachioDevice_Devices(dString, rachioDevice, deviceList, rachio, current)) // True if a device was created
                    associates = true;
            }

            return associates;
        }
        
        static internal bool RachioDevice_Devices(string dString, Device rachioDevice, List<DeviceDataPoint> deviceList, RachioConnection rachio, Current_Schedule current)
        {
            string name;
            string id;
            string type;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name, out type);
                if (id == rachioDevice.id && name == dString && type == "Device")
                {
                    Update_RachioDevice(ddPoint, rachioDevice, rachio, current);
                    return false;
                } 
            }
            
            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, dString, rachioDevice.name, rachioDevice.id, "Device");
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name, out type);
            switch (name)
            {
                case "Root":
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                        dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        SPair.Status = "Root";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/green.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }
                case "Rain Delay":
                    {
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Render = Enums.CAPIControlType.Single_Text_from_List;
                        SPair.Value = 0;
                        SPair.Status = "No Rain Delay";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        for (int i = 1; i < 8; i++)
                        {
                            SPair.Value = i;
                            SPair.Status = i + " Day Delay";
                            hs.DeviceVSP_AddPair(dvRef, SPair);
                        }

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 7;
                        GPair.Graphic = "/images/HomeSeer/contemporary/water.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Status":
                    {
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Render = Enums.CAPIControlType.Single_Text_from_List;
                        SPair.Value = 0;
                        SPair.Status = "Offline";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.Value = 1;
                        SPair.Status = "Online";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 2;
                        SPair.Status = "COMMUNITY OVERRIDE";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images//Homeseer/contemporary/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images//Homeseer/contemporary/on.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 2;
                        GPair.Graphic = "/images//Homeseer/contemporary/away.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Watering State":
                    {
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Render = Enums.CAPIControlType.Button;
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 1;
                        SPair.Status = "Stop Watering";
                        SPair.Value = 1;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Status = "Off";
                        SPair.Value = 0;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.Status = "Processing";
                        SPair.Value = 2;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images//HomeSeer/contemporary/stop.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images//HomeSeer/contemporary/pump-off.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 2;
                        GPair.Graphic = "/images//HomeSeer/contemporary/pump-on.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
            }
            return true;

        }

        static internal bool Find_Create_Zones(Person p, List<DeviceDataPoint> deviceList, RachioConnection rachio, Current_Schedule current)
        {
            bool create;
            bool associates = false;
            List<string> zStrings = getZoneStrings();

            foreach (var zone in p.devices[0].zones)
            {

                if (zone.enabled == true)
                {
                    foreach (var zString in zStrings)
                    {
                        create = Zone_Devices(zString, zone, deviceList, rachio, current);
                        if (create) // True if a device was created
                            associates = true;
                    } 
                }

            }

            return associates;
        }
        static internal bool Zone_Devices(string zString, Zone zone, List<DeviceDataPoint> deviceList, RachioConnection rachio, Current_Schedule current)
        {
            string name;
            string id;
            string type;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name, out type);
                if (id == zone.id && name == zString && type == "Zone")
                {
                    Update_Zone(ddPoint, zone, rachio, current);

                    return false;
                }
            }

            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, zString, zone.name, zone.id, "Zone");
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name, out type);
            switch (name)
            {
                case "Root":
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                        dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Status = "Root";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/yellow.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Control":
                    {
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        SPair.Status = "Off";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.RangeStart = .1;
                        SPair.RangeEnd = 180;
                        SPair.RangeStatusSuffix = " minutes remaining";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Control);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Render = Enums.CAPIControlType.Button;
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 3;
                        SPair.Status = "Off";
                        SPair.Value = 0;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.Render = Enums.CAPIControlType.TextBox_Number;
                        SPair.Render_Location.Row = 1;
                        SPair.Render_Location.Column = 1;
                        SPair.Status = "Enter Runtime: 1 - 180 mins";
                        SPair.RangeStart = .1;
                        SPair.RangeEnd = 180;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = .1;
                        GPair.RangeEnd = 181;
                        GPair.Graphic = "/images/HomeSeer/contemporary/on.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }

                case "Last Duration":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " Minutes";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 9999;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 9999;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/timers.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Last Watered":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " Days Ago";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 999;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 999;
                        GPair.Graphic = "/images/HomeSeer/contemporary/timers.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Max Runtime":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " Minutes";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 180;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 180;
                        GPair.Set_Value = 180;
                        GPair.Graphic = "/images/HomeSeer/contemporary/timers.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Total Runtime":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " Hours";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 9999;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 9999;
                        GPair.Set_Value = zone.runtime;
                        GPair.Graphic = "/images/HomeSeer/contemporary/timers.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        break;
                    }
                case "Zone Image":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        hs.DeviceVSP_AddPair(dvRef, SPair);
                        break;
                    }
            }

            return true;

        }



        static internal bool Find_Create_Weather(Person p, List<DeviceDataPoint> deviceList, RachioConnection rachio)
        {
            bool create;
            bool associates = false;
            
            List<string> cStrings = getCurrentStrings();
            List<string> fStrings = getForecastStrings();

            using (var total = rachio.getTotalForecast())
            {
                // Current Weather
                foreach (var cString in cStrings)
                {
                    create = Weather_Forecast_Devices(p.id, "Current", cString, total.currentW, null, deviceList, rachio);
                    if (create) // True if a device was created
                        associates = true;
                }

                // Forecast
                for (int i = 0; i < 7; i++)
                {
                    foreach (var fString in fStrings)
                    {
                        create = Weather_Forecast_Devices(p.id, "Day " + i, fString, null, total.forecastList[i], deviceList, rachio);
                        if (create) // True if a device was created
                            associates = true;

                    }
                } 
            }

            return associates;
        }



        static internal bool Weather_Forecast_Devices(string personId, string forecastName, string cfString, CurrentWeather currentW, Forecast forecast, List<DeviceDataPoint> deviceList, RachioConnection rachio)
        {
            string name;
            string id;
            string type;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name, out type);
                if (id == personId && name == cfString && type == forecastName)
                {
                    Update_Weather(ddPoint, currentW, forecast, rachio);
                    return false;
                }
            }

            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, cfString, forecastName, personId, forecastName);
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name, out type);
            switch (name)
            {
                case "Root":
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Status = "Root";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        if (currentW != null)
                        {
                            GPair.Graphic = "/images/HomeSeer/contemporary/blue.png";
                        }
                        else
                        {
                            GPair.Graphic = "/images/HomeSeer/contemporary/cyan.png";
                        }
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                //case "Cloud Cover":
                //case "Humidity":
                case "Precip Probability":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " %";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 100;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 100;
                        if (name.Equals("Cloud Cover"))
                        {
                            GPair.Graphic = "/images/HomeSeer/contemporary/daytime.png";
                        }
                        else
                        {
                            GPair.Graphic = "/images/HomeSeer/contemporary/water.gif";
                        }
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                //case "Dew Point":
                //case "Precip Intensity":
                case "Precipitation":
                case "Temperature":
                case "TemperatureMax":
                case "TemperatureMin":
                case "Wind Speed":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = -500;
                        GPair.RangeEnd = 500;
                        if (name.Equals("Dew Point") || name.Equals("Temperature"))
                        {
                            SPair.RangeStatusSuffix = " °" + VSVGPairs.VSPair.ScaleReplace;
                            SPair.HasScale = true;
                            GPair.Graphic = "/images/HomeSeer/contemporary/Thermometer-60.png";
                        }
                        else if (name.Equals("Precip Intensity") || name.Equals("Precipitation"))
                        {
                            SPair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                            SPair.HasScale = true;
                            GPair.Graphic = "/images/HomeSeer/contemporary/water.gif";
                        }
                        else if (name.Equals("TemperatureMax"))
                        {
                            SPair.RangeStatusSuffix = " °" + VSVGPairs.VSPair.ScaleReplace;
                            SPair.HasScale = true;
                            GPair.Graphic = "/images/HomeSeer/contemporary/Thermometer-100.png";
                        }
                        else if (name.Equals("TemperatureMin"))
                        {
                            SPair.RangeStatusSuffix = " °" + VSVGPairs.VSPair.ScaleReplace;
                            SPair.HasScale = true;
                            GPair.Graphic = "/images/HomeSeer/contemporary/Thermometer-00.png";
                        }
                        else if (name.Equals("Wind Speed"))
                        {
                            SPair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                            SPair.HasScale = true;
                            GPair.Graphic = "/images/HomeSeer/contemporary/fan-state-off.png";
                        }

                        hs.DeviceVSP_AddPair(dvRef, SPair);
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }

                case "Weather Conditions":
                case "Icon Url":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/daytime.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }

                case "Weather Update":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 99999;
                        SPair.RangeStatusSuffix = " minutes ago";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 0;
                        GPair.RangeEnd = 99999;
                        GPair.Graphic = "/images/HomeSeer/contemporary/timers.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
            }
            return true;
        }

        static internal bool Find_Create_ScheduleRules(Person p, List<DeviceDataPoint> deviceList, RachioConnection rachio)
        {
            ScheduleRule schedule = null;
            bool create;
            bool associates = false;
            List<string> schStrings = getScheduleStrings();

            foreach (var item in p.devices[0].scheduleRules)
            {

                schedule = item;
                foreach (var schString in schStrings)
                {
                    create = ScheduleRule_Devices(schString, schedule, deviceList, rachio);
                    if (create) // True if a device was created
                        associates = true;
                }
            }
            foreach (var item in p.devices[0].flexScheduleRules)
            {
                schedule = item;
                foreach (var schString in schStrings)
                {
                    create = ScheduleRule_Devices(schString, schedule, deviceList, rachio);
                    if (create) // True if a device was created
                        associates = true;
                }
            }

            return associates;
        }

        static internal bool ScheduleRule_Devices(string schString, ScheduleRule schedule, List<DeviceDataPoint> deviceList, RachioConnection rachio)
        {
            string name;
            string id;
            string type;

            foreach (var ddPoint in deviceList)
            {
                id = GetDeviceKeys(ddPoint.device, out name, out type);
                if (id == schedule.id && name == schString && type == "Schedule Rule")
                {
                    Update_ScheduleRule(ddPoint, schedule, rachio);
                    return false;
                }
            }

            DeviceClass dv = new DeviceClass();
            dv = GenericHomeSeerDevice(dv, schString, "Schedule Rule " + schedule.name, schedule.id, "Schedule Rule");
            var dvRef = dv.get_Ref(hs);
            id = GetDeviceKeys(dv, out name, out type);
            switch (name)
            {
                case "Root":
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                        dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Status = "Root";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/magenta.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                //case "Adjustment":
                //    {
                //        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);

                //        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                //        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                //        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                //        SPair.Value = 0;
                //        SPair.Status = "No Adjustment";
                //        hs.DeviceVSP_AddPair(dvRef, SPair);

                //        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                //        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                //        SPair.Render = Enums.CAPIControlType.TextBox_Number;
                //        SPair.Render_Location.Row = 1;
                //        SPair.Render_Location.Column = 1;
                //        SPair.Status = "Enter -1 to 1";
                //        SPair.Value = 1;
                //        SPair.RangeStart = -1;
                //        SPair.RangeEnd = 1;
                //        hs.DeviceVSP_AddPair(dvRef, SPair);


                //        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                //        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                //        GPair.RangeStart = .1;
                //        GPair.RangeEnd = 10;
                //        GPair.Graphic = "/images/HomeSeer/contemporary/timers.png";
                //        hs.DeviceVGP_AddPair(dvRef, GPair);

                //        break;
                //    }
                case "Summary":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/contemporary/daytime.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
                case "Status":
                    {
                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        SPair.Status = "Disabled";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        SPair.Value = 1;
                        SPair.Status = "Enabled";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images//HomeSeer/contemporary/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images//HomeSeer/contemporary/on.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);
                        break;
                    }
            }
            return true;

        }

        static internal string GetDeviceKeys(DeviceClass dev, out string name, out string type)
        {
            string id = "";
            name = "";
            type = "";
            PlugExtraData.clsPlugExtraData pData = dev.get_PlugExtraData_Get(hs);
            if (pData != null)
            {
                id = (string)pData.GetNamed("id");
                name = (string)pData.GetNamed("name");
                type = (string)pData.GetNamed("type");
            }
            return id;
        }

        static internal void SetDeviceKeys(DeviceClass dev, string id, string name, string type)
        {
            PlugExtraData.clsPlugExtraData pData = dev.get_PlugExtraData_Get(hs);
            if (pData == null)
                pData = new PlugExtraData.clsPlugExtraData();
            pData.AddNamed("id", id);
            pData.AddNamed("name", name);
            pData.AddNamed("type", type);
            dev.set_PlugExtraData_Set(hs, pData);
        }

        static internal DeviceClass GenericHomeSeerDevice(DeviceClass dv, string dvName, string dvName_long, string device_id, string type)
        {
            int dvRef;
            Console.WriteLine("Creating Device: " + dvName_long + "-" + dvName);
            var DT = new DeviceTypeInfo_m.DeviceTypeInfo();
            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
            if (dvName.Contains("Root"))
            {
                DT.Device_Type = 99;
            }
            hs.NewDeviceRef(dvName_long + "-" + dvName);
            dvRef = hs.GetDeviceRefByName(dvName_long + "-" + dvName);
            dv = (DeviceClass)hs.GetDeviceByRef(dvRef);
            dv.set_Address(hs, "");
            SetDeviceKeys(dv, device_id, dvName, type);
            //dv.set_Code(hs, device_id + "-" + dvName_long + "-" + dvName);
            dv.set_Location(hs, "Outside");
            dv.set_Location2(hs, "Rachio");
            dv.set_Interface(hs, IFACE_NAME);
            dv.set_Status_Support(hs, true);
            dv.set_Can_Dim(hs, false);
            dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
            dv.set_DeviceType_Set(hs, DT);
            dv.set_Relationship(hs, Enums.eRelationship.Child);
            return dv;
        }

        private static void Default_VS_Pairs_AddUpdateUtil(int dvRef, VSVGPairs.VSPair Pair)
        {
            if (Pair == null)
                return;
            if (dvRef < 1)
                return;
            if (!hs.DeviceExistsRef(dvRef))
                return;

            VSVGPairs.VSPair Existing = null;

            // The purpose of this procedure is to add the protected, default VS/VG pairs WITHOUT overwriting any user added
            //   pairs unless absolutely necessary (because they conflict).

            try
            {
                Existing = hs.DeviceVSP_Get(dvRef, Pair.Value, Pair.ControlStatus);
                //VSPairs.GetPairByValue(Pair.Value, Pair.ControlStatus)


                if (Existing != null)
                {
                    // This is unprotected, so it is a user's value/status pair.
                    if (Existing.ControlStatus == HomeSeerAPI.ePairStatusControl.Both & Pair.ControlStatus != HomeSeerAPI.ePairStatusControl.Both)
                    {
                        // The existing one is for BOTH, so try changing it to the opposite of what we are adding and then add it.
                        if (Pair.ControlStatus == HomeSeerAPI.ePairStatusControl.Status)
                        {
                            if (!hs.DeviceVSP_ChangePair(dvRef, Existing, HomeSeerAPI.ePairStatusControl.Control))
                            {
                                hs.DeviceVSP_ClearBoth(dvRef, Pair.Value);
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                            else
                            {
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                        }
                        else
                        {
                            if (!hs.DeviceVSP_ChangePair(dvRef, Existing, HomeSeerAPI.ePairStatusControl.Status))
                            {
                                hs.DeviceVSP_ClearBoth(dvRef, Pair.Value);
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                            else
                            {
                                hs.DeviceVSP_AddPair(dvRef, Pair);
                            }
                        }
                    }
                    else if (Existing.ControlStatus == HomeSeerAPI.ePairStatusControl.Control)
                    {
                        // There is an existing one that is STATUS or CONTROL - remove it if ours is protected.
                        hs.DeviceVSP_ClearControl(dvRef, Pair.Value);
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                    }
                    else if (Existing.ControlStatus == HomeSeerAPI.ePairStatusControl.Status)
                    {
                        // There is an existing one that is STATUS or CONTROL - remove it if ours is protected.
                        hs.DeviceVSP_ClearStatus(dvRef, Pair.Value);
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                    }

                }
                else
                {
                    // There is not a pair existing, so just add it.
                    hs.DeviceVSP_AddPair(dvRef, Pair);

                }


            }
            catch (Exception)
            {
            }


        }

        // This is called at the end of device creation
        // It works by first finding the root device of the designated family (ie Device, Current Weather, Todays Forecast, Tomorrows Forecast)
        // Then, it finds the expected associates and adds them to the root (eg CurrentWeatherRootDevice.AssociateDevice_Add(hs, WindSpeedRef#))
        static internal void SetAssociatedDevices()
        {
            List<DeviceDataPoint> deviceList = new List<DeviceDataPoint>();
            string name;
            string id;
            string type;

            deviceList = Get_Device_List(deviceList);

            foreach (var ddPoint in deviceList)
            {

                id = GetDeviceKeys(ddPoint.device, out name, out type);

                if (name == "Root" && type == "Device")   // True if the Device Root has been found
                {
                    ddPoint.device.AssociatedDevice_ClearAll(hs);

                    foreach (var aDDPoint in deviceList)
                    {
                        string aName;
                        string aType;
                        string aId = GetDeviceKeys(aDDPoint.device, out aName, out aType);
                        if (aId == id && aType == type && aName != "Root")
                        {
                            ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.device.get_Ref(hs));
                        }
                    }
                }

                if (name == "Root" && type == "Current")   // True if the Current Weather Root has been found
                {
                    ddPoint.device.AssociatedDevice_ClearAll(hs);

                    foreach (var aDDPoint in deviceList)
                    {
                        string aName;
                        string aType;
                        string aId = GetDeviceKeys(aDDPoint.device, out aName, out aType);
                        if (aId == id && aType == type && aName != "Root")
                        {
                            ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.device.get_Ref(hs));
                        }
                    }
                }

                for (int i = 0; i < 7; i++)
                {
                    if (name == "Root" && type == ("Day " + i))   // True if the Todays Forecast Root has been found
                    {
                        ddPoint.device.AssociatedDevice_ClearAll(hs);

                        foreach (var aDDPoint in deviceList)
                        {
                            string aName;
                            string aType;
                            string aId = GetDeviceKeys(aDDPoint.device, out aName, out aType);
                            if (aId == id && aType == type && aName != "Root")
                            {
                                ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.device.get_Ref(hs));
                            }
                        }
                    } 
                }


                if (name == "Root" && type == "Schedule Rule")   // True if the Schedule Rule Root has been found
                {
                    ddPoint.device.AssociatedDevice_ClearAll(hs);

                    foreach (var aDDPoint in deviceList)
                    {
                        string aName;
                        string aType;
                        string aId = GetDeviceKeys(aDDPoint.device, out aName, out aType);
                        if (aId == id && aType == type && aName != "Root")
                        {
                            ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.device.get_Ref(hs));
                        }
                    }
                }

                if (name == "Root" && type == "Zone")   // True if the Zone Root has been found
                {
                    ddPoint.device.AssociatedDevice_ClearAll(hs);

                    foreach (var aDDPoint in deviceList)
                    {
                        string aName;
                        string aType;
                        string aId = GetDeviceKeys(aDDPoint.device, out aName, out aType);
                        if (aId == id && aType == type && aName != "Root")
                        {
                            ddPoint.device.AssociatedDevice_Add(hs, aDDPoint.device.get_Ref(hs));
                        }
                    }
                }
            }
        }

        static internal List<string> getDeviceStrings()
        {
            var dStrings = new List<string>
            {
                "Rain Delay",
                "Status",
                "Watering State",
                "Root"
            };

            return dStrings;
        }

        static internal List<string> getZoneStrings()
        {
            var dStrings = new List<string>
            {
                "Control",
                "Last Duration",
                "Last Watered",
                "Max Runtime",
                "Total Runtime",
                "Zone Image",
                "Root"
            };

            return dStrings;
        }
        static internal List<string> getCurrentStrings()
        {
            var dStrings = new List<string>
            {
                //"Cloud Cover",
                //"Dew Point",
                //"Humidity",
                //"Precip Intensity",
                "Precip Probability",
                "Precipitation",
                "Temperature",
                "Weather Conditions",
                "Weather Update",
                "Wind Speed",
                "Icon Url",
                "Root"
            };

            return dStrings;
        }

        static internal List<string> getForecastStrings()
        {
            var dStrings = new List<string>
            {
                //"Cloud Cover",
                //"Humidity",
                //"Precip Intensity",
                "Precip Probability",
                "Precipitation",
                "TemperatureMax",
                "TemperatureMin",
                "Weather Conditions",
                "Wind Speed",
                "Icon Url",
                "Root"
            };

            return dStrings;
        }

        static internal List<string> getScheduleStrings()
        {
            var dStrings = new List<string>
            {
                "Root",
                //"Seasonal Adjustment",
                "Summary",
                "Status"
            };

            return dStrings;
        }

        static internal string getTemperatureUnits(string units)
        {
            if (units.Equals("METRIC"))
            {
                return "C";
            }
            else
            {
                return "F";
            }
        }
        static internal string getDepthUnits(string units)
        {
            if (units.Equals("METRIC"))
            {
                return "cm";
            }
            else
            {
                return "in";
            }
        }
        static internal string getSpeedUnits(string units)
        {
            if (units.Equals("METRIC"))
            {
                return "km/h";
            }
            else
            {
                return "mph";
            }
        }
        // Gets the difference between event time(since) and now in minutes
        static internal double getTimeSince(double since)
        {
            since = getNowSinceEpoch() - since;
            since = Math.Round(since / 60);  // to minutes
            return since;
        }
        static internal double getNowSinceEpoch()
        {
            TimeSpan unixTime = DateTime.UtcNow - new DateTime(1970, 1, 1);
            double secondsSinceEpoch = (double)unixTime.TotalSeconds;
            return Math.Round(secondsSinceEpoch);
        }

    }

}
