using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using HomeSeerAPI;
using Scheduler;
using Scheduler.Classes;
using System.Reflection;
using System.Text;


using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using HSPI_RACHIOSIID.Models;
using static HomeSeerAPI.DeviceTypeInfo_m.DeviceTypeInfo;

namespace HSPI_RACHIOSIID
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
        public const string IFACE_NAME = "RACHIOSIID";
        //public const string IFACE_NAME = "Sample Plugin";
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





        static internal List<DeviceClass> Get_Device_List(List<DeviceClass> deviceList, DeviceClass dv)
        {
            // Gets relevant devices from HomeSeer
            try
            {
                Scheduler.Classes.clsDeviceEnumeration EN = default(Scheduler.Classes.clsDeviceEnumeration);
                EN = (Scheduler.Classes.clsDeviceEnumeration)Util.hs.GetDeviceEnumerator();
                if (EN == null)
                    throw new Exception(IFACE_NAME + " failed to get a device enumerator from HomeSeer.");
                do
                {
                    dv = EN.GetNext();
                    if (dv == null)
                    { continue; }





                    if (dv.get_Address(hs).Contains(IFACE_NAME))
                    {

                        deviceList.Add(dv);
                    }

                } while (!(EN.Finished));
            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Devices/Enumerator: " + ex.Message);
            }

            return deviceList;
        }

        static internal void Update_RachioDevice(Device rachioDevice, DeviceClass dv, RachioConnection rachio)
        {

        }

        static internal void Update_Zone(Zone z, DeviceClass dv, RachioConnection rachio, Current_Schedule currentS)
        {
            int dvRef;
            CAPI.CAPIControl objCAPIControl;

            dvRef = dv.get_Ref(hs); // Root Ref #

            for (int i = 1; i < 6; i++)
            {
                if (i == 1)    // Control devices
                {
                    if (rachio.getTimeRemainingForZone(z) > 0)   // If the time On is greater than zero
                    {

                        hs.SetDeviceValueByRef(dvRef - 5, 1, true); // Set Control Device to On
                        hs.SetDeviceString(dvRef - 5, rachio.getStatusForZone(z), true);


                    }
                    else
                    {

                        hs.SetDeviceValueByRef(dvRef - 5, 0, true); // Set Control Device to Off

                        hs.SetDeviceString(dvRef - 5, "Off", true);

                    }
                }
                else if (i == 2)  // Last Duration devices
                {
                    if (rachio.getTimeRemainingForZone(z) > 0)  // This only changes if the zone is On, that is, a new duration
                    {
                        double dur = currentS.duration / 60;
                        dur = Math.Round(dur, 1);

                        hs.SetDeviceValueByRef(dvRef - 4, dur, true);
                    }
                }
                else if (i == 3)
                {

                    double watered = rachio.getLastWateredForZone(z);
                    watered = Math.Round(watered / 24 / 3600, 1);

                    hs.SetDeviceValueByRef(dvRef - 3, watered, true);
                }
                else if (i == 4)
                {
                    hs.SetDeviceValueByRef(dvRef - 2, 3, true);
                }
                else if (i == 5)
                {
                    double runtime = z.runtime;
                    runtime = Math.Round(runtime / 60, 1);
                    hs.SetDeviceValueByRef(dvRef - 1, runtime, true);
                }
            }
        }

        static internal void Update_Weather(CurrentWeather currentW, DeviceClass dv, RachioConnection rachio)
        {

        }

        
        static internal void Find_Create_Devices(RachioConnection rachio)
        {

            List<DeviceClass> deviceList = new List<DeviceClass>();
            DeviceClass dv = default(DeviceClass);

            Person p = rachio.getPerson();


            deviceList = Get_Device_List(deviceList, dv);

            Find_Create_RachioDevices(dv, p, deviceList, rachio);
            Find_Create_Zones(dv, p, deviceList, rachio);
            Find_Create_Weather(dv, p, deviceList, rachio);

        }

        static string[] deviceStrings = new string[4] { "Device Rain Delay", "Device Status", "Watering State", "Device Root" };
        static internal void Find_Create_RachioDevices(DeviceClass dv, Person p, List<DeviceClass> deviceList, RachioConnection rachio)
        {
            Device rachioDevice = p.devices[0];
            bool Found;
            string testAddress;
            try
            {
                foreach(var dString in deviceStrings)
                {
                    RachioDevice_Devices(dv, false, IFACE_NAME + dString, rachioDevice, deviceList, rachio);
                }
            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_RachioDevices: " + ex.Message);
            }
        }

        static internal void RachioDevice_Devices(DeviceClass dv, bool Found, string testAddress, Device rachioDevice, List<DeviceClass> deviceList, RachioConnection rachio)
        {
            foreach (var device in deviceList)
            {

                if (device.get_Address(hs).Contains(testAddress))
                {
                    Update_RachioDevice(rachioDevice, device, rachio);
                    Found = true;
                    break;
                }
            }
            if (!Found)
            {
                int dvRef;
                string dvName;


                int[] childRef = new int[10];

                dvName = testAddress;
                Console.WriteLine("Creating Device: " + dvName);
                var DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


                hs.NewDeviceRef(dvName);
                dvRef = hs.GetDeviceRefByName(dvName);
                dv = (DeviceClass)hs.GetDeviceByRef(dvRef);
                dv.set_Address(hs, IFACE_NAME);
                dv.set_Code(hs, testAddress.Substring(11));
                dv.set_Location(hs, IFACE_NAME);
                dv.set_Location2(hs, IFACE_NAME);
                dv.set_Interface(hs, IFACE_NAME);

                dv.set_Status_Support(hs, true);
                dv.set_Can_Dim(hs, false);
                dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);

                // Weather/Forecast Root specific
                if (dvName.Contains("Root"))
                {
                    dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                    DT.Device_Type = 99;
                    dv.set_DeviceType_Set(hs, DT);

                    for (int i = 0; i < 11; i++)
                    {
                        dv.AssociatedDevice_Add(hs, childRef[i]);

                    }

                    VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                    SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                    SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                    SPair.Status = "Root";
                    hs.DeviceVSP_AddPair(dvRef, SPair);

                    VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                    GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                    GPair.Set_Value = 0;
                    GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                    hs.DeviceVGP_AddPair(dvRef, GPair);

                }



                // Cloud Cover Specific
                if (dvName.Contains("Cloud Cover"))
                {
                    dv.set_Relationship(hs, Enums.eRelationship.Child);

                    VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                    SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                    SPair.PairType = VSVGPairs.VSVGPairType.Range;
                    SPair.HasScale = true;
                    SPair.RangeStatusSuffix = " %";
                    SPair.RangeStart = -500;
                    SPair.RangeEnd = 500;
                    if (currentW != null)
                    {
                        SPair.Value = currentW.cloudCover;
                    }
                    else
                    {
                        SPair.Value = forecast.cloudCover;
                    }


                    hs.DeviceVSP_AddPair(dvRef, SPair);

                    VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                    GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                    GPair.Set_Value = 0;
                    GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                    hs.DeviceVGP_AddPair(dvRef, GPair);

                    childRef[1] = dvRef;
                }
            }

        static string[] zoneStrings = new string[6] { "Control", "Last Duration", "Last Watered", "Max Runtime", "Total Runtime", "Root" };
        static internal void Find_Create_Zones(DeviceClass dv, Person p, List<DeviceClass> deviceList, RachioConnection rachio)
        {
            Current_Schedule current = rachio.getCurrentSchedule();
            bool Found;
            string testAddress;
            foreach (Zone zone in p.devices[0].zones)
            {

                Found = false;
                testAddress = "RACHIOSIID-" + zone.name + "-Root";
                try
                {
                    foreach (var device in deviceList)
                    {

                        if (device.get_Address(hs).Contains(testAddress))
                        {
                            Update_Zone(zone, device, rachio, current);
                            Found = true;
                            break;
                        }
                    }
                    if (!Found)
                    {
                        int dvRef;
                        string dvName;


                        int[] childRef = new int[5];
                        foreach (var zString in zoneStrings)
                        {
                            dvName = "RACHIOSIID-" + zone.name + "-" + zString;
                            Console.WriteLine("Creating Device: " + dvName);
                            var DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


                            hs.NewDeviceRef(dvName);
                            dvRef = hs.GetDeviceRefByName(dvName);
                            dv = (DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, IFACE_NAME);
                            dv.set_Code(hs, zone.name + "-" + zString);
                            dv.set_Location(hs, IFACE_NAME);
                            dv.set_Location2(hs, IFACE_NAME);
                            dv.set_Interface(hs, IFACE_NAME);

                            dv.set_Status_Support(hs, true);
                            dv.set_Can_Dim(hs, false);
                            dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);

                            // Root specific
                            if (zString.Equals("Root"))
                            {
                                dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                                dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);
                                DT.Device_Type = 99;
                                dv.set_DeviceType_Set(hs, DT);

                                for (int i = 0; i < 5; i++)
                                {
                                    dv.AssociatedDevice_Add(hs, childRef[i]);

                                }

                                VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                                SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);


                                SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                SPair.Status = "Root";

                                hs.DeviceVSP_AddPair(dvRef, SPair);

                                VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                                GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                GPair.Set_Value = 0;
                                GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                                hs.DeviceVGP_AddPair(dvRef, GPair);

                            }
                            // Control specific
                            if (zString.Equals("Control"))
                            {
                                dv.set_Relationship(hs, Enums.eRelationship.Child);
                                dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                                dv.set_DeviceType_Set(hs, DT);

                                // add an OFF button and value
                                VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                                SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                                SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                SPair.Value = 0;
                                SPair.Status = "Off";
                                SPair.Render = Enums.CAPIControlType.Button;
                                SPair.Render_Location.Row = 1;
                                SPair.Render_Location.Column = 1;
                                SPair.ControlUse = ePairControlUse._Off;
                                // set this for UI apps like HSTouch so they know this is for OFF
                                hs.DeviceVSP_AddPair(dvRef, SPair);

                                // add an ON button and value
                                SPair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                                SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                SPair.Value = 1;
                                SPair.Status = "On";
                                SPair.ControlUse = ePairControlUse._On;
                                // set this for UI apps like HSTouch so they know this is for lighting control ON
                                SPair.Render = Enums.CAPIControlType.Button;
                                SPair.Render_Location.Row = 1;
                                SPair.Render_Location.Column = 2;
                                hs.DeviceVSP_AddPair(dvRef, SPair);



                                VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                                GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                GPair.Set_Value = 0;
                                GPair.Graphic = "/images/hspi_ultrarachio3/zone_off.png";
                                hs.DeviceVGP_AddPair(dvRef, GPair);



                                GPair = new VSVGPairs.VGPair();
                                GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                GPair.Set_Value = 1;
                                GPair.Graphic = "/images/hspi_ultrarachio3/zone_on.png";
                                hs.DeviceVGP_AddPair(dvRef, GPair);

                                childRef[0] = dvRef;
                            }
                            // Last Duration specific
                            if (zString.Equals("Last Duration"))
                            {
                                dv.set_Relationship(hs, Enums.eRelationship.Child);
                                dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);
                                dv.set_DeviceType_Set(hs, DT);

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
                                GPair.Graphic = "/images/hspi_ultrarachio3/time.png";
                                hs.DeviceVGP_AddPair(dvRef, GPair);

                                childRef[1] = dvRef;
                            }
                            // Last Watered specific
                            if (zString.Equals("Last Watered"))
                            {
                                dv.set_Relationship(hs, Enums.eRelationship.Child);
                                dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);
                                dv.set_DeviceType_Set(hs, DT);

                                VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                                SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                                SPair.PairType = VSVGPairs.VSVGPairType.Range;
                                SPair.HasScale = true;
                                SPair.RangeStatusSuffix = " Days Ago";
                                SPair.RangeStart = 0;
                                SPair.RangeEnd = 9999;
                                hs.DeviceVSP_AddPair(dvRef, SPair);

                                VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                                GPair.PairType = VSVGPairs.VSVGPairType.Range;
                                GPair.RangeStart = 0;
                                GPair.RangeEnd = 9999;
                                GPair.Set_Value = 0;
                                GPair.Graphic = "/images/hspi_ultrarachio3/schedule_rule_enabled.png.";
                                hs.DeviceVGP_AddPair(dvRef, GPair);

                                childRef[2] = dvRef;
                            }
                            // Max Runtime specific
                            if (zString.Equals("Max Runtime"))
                            {
                                dv.set_Relationship(hs, Enums.eRelationship.Child);
                                dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);
                                dv.set_DeviceType_Set(hs, DT);

                                VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                                SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                                SPair.PairType = VSVGPairs.VSVGPairType.Range;
                                SPair.HasScale = true;
                                SPair.RangeStatusSuffix = " Hours";
                                SPair.RangeStart = 0;
                                SPair.RangeEnd = 3;
                                hs.DeviceVSP_AddPair(dvRef, SPair);

                                VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                                GPair.PairType = VSVGPairs.VSVGPairType.Range;
                                GPair.RangeStart = 0;
                                GPair.RangeEnd = 3;
                                GPair.Set_Value = 3;
                                GPair.Graphic = "/images/hspi_ultrarachio3/time.png";
                                hs.DeviceVGP_AddPair(dvRef, GPair);

                                childRef[3] = dvRef;
                            }
                            // Total Runtime specific
                            if (zString.Equals("Total Runtime"))
                            {


                                dv.set_Relationship(hs, Enums.eRelationship.Child);
                                dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);
                                dv.set_DeviceType_Set(hs, DT);

                                VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                                SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                                SPair.PairType = VSVGPairs.VSVGPairType.Range;
                                SPair.HasScale = true;
                                SPair.RangeStatusSuffix = " Hours";
                                SPair.RangeStart = 0;
                                SPair.RangeEnd = 999999;

                                hs.DeviceVSP_AddPair(dvRef, SPair);



                                VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                                GPair.PairType = VSVGPairs.VSVGPairType.Range;
                                GPair.RangeStart = 0;
                                GPair.RangeEnd = 999999;
                                GPair.Set_Value = zone.runtime;
                                GPair.Graphic = "/images/hspi_ultrarachio3/time.png";
                                hs.DeviceVGP_AddPair(dvRef, GPair);


                                childRef[4] = dvRef;
                            }
                            hs.SaveEventsDevices();
                        }


                    }
                }
                catch (Exception ex)
                {

                    hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Zones: " + ex.Message);
                }
            }
        }

        static string[] weatherStrings = new string[11] { "Cloud Cover", "Dew Point", "Humidity", "Precip Intensity", "Precip Probability", "Precipitation", "Temperature", "Weather Conditions", "Weather Update", "Wind Speed", "Weather Root" };
        static string[] forecastStrings = new string[11] { "Cloud Cover", "Dew Point", "Humidity", "Precip Intensity", "Precip Probability", "Precipitation", "TemperatureMax", "TemperatureMin", "Weather Conditions", "Wind Speed", "Weather Root" };

        static internal void Find_Create_Weather(DeviceClass dv, Person p, List<DeviceClass> deviceList, RachioConnection rachio)
        {

            CurrentWeather currentW = rachio.getCurrentWeather();

            // Current Weather
            foreach (var wString in weatherStrings)
            {
                Weather_Forecast_Devices(dv, false, IFACE_NAME + "-Current-" + wString, currentW, null, deviceList, rachio);

            }
            // Todays Forecast
            foreach (var fString in forecastStrings)
            {
                Weather_Forecast_Devices(dv, false, IFACE_NAME + "-Todays-" + fString, null, currentW.forecastList[0], deviceList, rachio);
            }
            // Tomorrows Forecast
            foreach (var fString in forecastStrings)
            {
                Weather_Forecast_Devices(dv, false, IFACE_NAME + "-Tomorrows-" + fString, null, currentW.forecastList[1], deviceList, rachio);
            }
        }

        static internal void Weather_Forecast_Devices(DeviceClass dv, bool Found, string testAddress, CurrentWeather currentW, Forecast forecast, List<DeviceClass> deviceList, RachioConnection rachio)
        {

            try
            {
                foreach (var device in deviceList)
                {

                    if (device.get_Address(hs).Contains(testAddress))
                    {
                        Update_Weather(currentW, device, rachio);
                        Found = true;
                        break;
                    }
                }
                if (!Found)
                {
                    int dvRef;
                    string dvName;


                    int[] childRef = new int[10];

                    dvName = testAddress;
                    Console.WriteLine("Creating Device: " + dvName);
                    var DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                    DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


                    hs.NewDeviceRef(dvName);
                    dvRef = hs.GetDeviceRefByName(dvName);
                    dv = (DeviceClass)hs.GetDeviceByRef(dvRef);
                    dv.set_Address(hs, IFACE_NAME);
                    dv.set_Code(hs, testAddress.Substring(11));
                    dv.set_Location(hs, IFACE_NAME);
                    dv.set_Location2(hs, IFACE_NAME);
                    dv.set_Interface(hs, IFACE_NAME);

                    dv.set_Status_Support(hs, true);
                    dv.set_Can_Dim(hs, false);
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                    dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);

                    // Weather/Forecast Root specific
                    if (dvName.Contains("Root"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);
                        DT.Device_Type = 99;
                        dv.set_DeviceType_Set(hs, DT);

                        for (int i = 0; i < 11; i++)
                        {
                            dv.AssociatedDevice_Add(hs, childRef[i]);

                        }

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Status = "Root";
                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                    }



                    // Cloud Cover Specific
                    if (dvName.Contains("Cloud Cover"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " %";
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.cloudCover;
                        }
                        else
                        {
                            SPair.Value = forecast.cloudCover;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[1] = dvRef;
                    }

                    // Dew Point
                    if (dvName.Contains("Dew Point"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;                       
                        SPair.RangeStatusSuffix = " " + getTemperatureUnits(rachio.units);                       
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 100;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.dewPoint;
                        }
                        else
                        {
                            SPair.Value = forecast.dewPoint;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[2] = dvRef;
                    }

                    // Humidity
                    if (dvName.Contains("Humidity"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " %";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 100;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.humidity;
                        }
                        else
                        {
                            SPair.Value = forecast.humidity;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[3] = dvRef;
                    }

                    // Precip Intensity
                    if (dvName.Contains("Precip Intensity"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " " + getDepthUnits(rachio.units) + "/hr";
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.precipIntensity;
                        }
                        else
                        {
                            SPair.Value = forecast.precipIntensity;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[4] = dvRef;
                    }

                    // Precip Probability
                    if (dvName.Contains("Precip Probability"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " %";
                        SPair.RangeStart = 0;
                        SPair.RangeEnd = 100;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.precipProbability;
                        }
                        else
                        {
                            SPair.Value = forecast.precipProbability;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[5] = dvRef;
                    }

                    // Precipitation
                    if (dvName.Contains("Precipitation"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " " + getDepthUnits(rachio.units);
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.precipitation;
                        }
                        else
                        {
                            SPair.Value = forecast.precipitation;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[6] = dvRef;
                    }

                    // Temperature
                    if (dvName.Contains("Temperature") && !dvName.Contains("Max") && !dvName.Contains("Min"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " " + getTemperatureUnits(rachio.units);
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        SPair.Value = currentW.currentTemperature;

                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[7] = dvRef;
                    }

                    // Temperature
                    if (dvName.Contains("Temperature Max"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " " + getTemperatureUnits(rachio.units);
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        SPair.Value = forecast.temperatureMax;

                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[7] = dvRef;
                    }

                    // Temperature Min
                    if (dvName.Contains("Temperature Min"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " " + getTemperatureUnits(rachio.units);
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        SPair.Value = forecast.temperatureMin;

                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[8] = dvRef;
                    }

                    // Weather Conditions
                    if (dvName.Contains("Weather Conditions"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        if (currentW != null)
                        {
                            SPair.Status = currentW.weatherSummary;
                        }
                        else
                        {
                            SPair.Status = forecast.weatherSummary;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        if (currentW != null)
                        {
                            childRef[8] = dvRef; 
                        }
                        else
                        {
                            childRef[9] = dvRef;
                        }
                    }

                    // Weather Update
                    if (dvName.Contains("Temperature Min"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        SPair.Value = 0;
                        SPair.Status = getTimeSince(currentW.time) + " minutes ago";

                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[9] = dvRef;
                    }

                    // Wind Speed
                    if (dvName.Contains("Wind Speed"))
                    {
                        dv.set_Relationship(hs, Enums.eRelationship.Child);

                        VSVGPairs.VSPair SPair = default(VSVGPairs.VSPair);
                        SPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
                        SPair.PairType = VSVGPairs.VSVGPairType.Range;
                        SPair.HasScale = true;
                        SPair.RangeStatusSuffix = " " + getSpeedUnits(rachio.units);
                        SPair.RangeStart = -500;
                        SPair.RangeEnd = 500;
                        if (currentW != null)
                        {
                            SPair.Value = currentW.windSpeed;
                        }
                        else
                        {
                            SPair.Value = forecast.windSpeed;
                        }


                        hs.DeviceVSP_AddPair(dvRef, SPair);

                        VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/hspi_ultrarachio3/device_root.png";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        childRef[10] = dvRef;
                    }

                    hs.SaveEventsDevices();



                }
            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Weather: " + ex.Message);
            }
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
        static internal string getTemperatureUnits(string units)
        {
            if (units.Equals("METRIC"))
            {
                return "°C";
            }
            else
            {
                return "°F";
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
            TimeSpan unixTime = DateTime.UtcNow - new DateTime(1970, 1, 1);
            double secondsSinceEpoch = (double)unixTime.TotalSeconds;
            since = since / 1000 - secondsSinceEpoch;
            since = Math.Round(since / 60, 1);  // to minutes
            return since;
        }
    }

}
