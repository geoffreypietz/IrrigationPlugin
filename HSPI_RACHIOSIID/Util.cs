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
        static internal void Find_Create_Devices()
        {
            
            System.Collections.Generic.List<Scheduler.Classes.DeviceClass> col = new System.Collections.Generic.List<Scheduler.Classes.DeviceClass>();
            Scheduler.Classes.DeviceClass dv = default(Scheduler.Classes.DeviceClass);
            bool Found = false;

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
                        continue;



                    if (dv.get_Interface(null) != null)
                    {
                        if (dv.get_Interface(null).Trim() == IFACE_NAME)
                        {
                            col.Add(dv);
                        }
                    }
                } while (!(EN.Finished));
            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Devices/Enumerator: " + ex.Message);
            }

            try
            {
                DeviceTypeInfo_m.DeviceTypeInfo DT = null;
                if (col != null && col.Count > 0)
                {
                    foreach (Scheduler.Classes.DeviceClass dev in col)
                    {
                        if (dev == null)
                            continue;
                        if (dev.get_Interface(hs) != IFACE_NAME)
                            continue;
                        DT = dev.get_DeviceType_Get(hs);
                        if (DT != null)
                        {
                            if (DT.Device_API == DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In && DT.Device_Type == 71)
                            {
                                Found = true;
                                MyDevice = dev.get_Ref(null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Devices/Find: " + ex.Message);
            }

            try
            {





                if (!Found)
                {
                    int dvRef = 0;

                    RachioConnection rc = new RachioConnection();
                    Person p = rc.getPerson();
                    int count = 0;
                    Console.WriteLine("Creating Devices...");

                    // ALL DEVICES
                    dv.set_Location(hs, "Rachio_SIID");
                    dv.set_Location2(hs, "Rachio_SIID");
                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                    //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    //dv.MISC_Set(hs,Enums.dvMISC.HIDDEN);
                    dv.set_Status_Support(hs, true);

                    // ZONES
                    foreach (Zone z in p.devices[0].zones)
                    {
                        //Zone Root
                        dvRef = hs.NewDeviceRef(z.name);
                        MyDevice = dvRef; //for auto update
                        if (dvRef > 0)
                        {

                            dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, "HOME");
                            //dv.Can_Dim(hs, True
                            dv.set_Device_Type_String(hs, "Rachio Zone" + z.zoneNumber + " Root");
                            DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                            DT.Device_Type = 71;
                            DT.Device_SubType = z.zoneNumber;
                            dv.set_DeviceType_Set(hs, DT);
                            dv.set_Interface(hs, IFACE_NAME);
                            dv.set_InterfaceInstance(hs, "");
                            dv.set_Last_Change(hs, new DateTime());             
                        }
                        count++;

                        //Zone Control
                        dvRef = hs.NewDeviceRef(z.name);
                        MyDevice = dvRef; //for auto update
                        if (dvRef > 0)
                        {

                            dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, "HOME");
                            //dv.Can_Dim(hs, True
                            dv.set_Device_Type_String(hs, "Rachio Zone" + z.zoneNumber + " Control");
                            DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                            DT.Device_Type = 71;
                            DT.Device_SubType = z.zoneNumber;
                            dv.set_DeviceType_Set(hs, DT);
                            dv.set_Interface(hs, IFACE_NAME);
                            dv.set_InterfaceInstance(hs, "");
                            dv.set_Last_Change(hs, new DateTime());



                            VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                            // add values, will appear as a radio control and only allow one option to be selected at a time
                            Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                            Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                            Pair.Render = Enums.CAPIControlType.List_Text_from_List;
                            Pair.Value = 0;
                            Pair.Status = "Value 0";
                            hs.DeviceVSP_AddPair(dvRef, Pair);
                            Pair.Value = 1;
                            Pair.Status = "Value 1";
                            hs.DeviceVSP_AddPair(dvRef, Pair);
                            Pair.Value = 2;
                            Pair.Status = "Value 2";
                            hs.DeviceVSP_AddPair(dvRef, Pair);


                            
                        }
                        count++;

                        // Zone Last Duration
                        dvRef = hs.NewDeviceRef(z.name);
                        MyDevice = dvRef; //for auto update
                        if (dvRef > 0)
                        {

                            dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, "HOME");
                            //dv.Can_Dim(hs, True
                            dv.set_Device_Type_String(hs, "Rachio Zone" + z.zoneNumber + " Last Duration");
                            DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                            DT.Device_Type = 71;
                            DT.Device_SubType = z.zoneNumber;
                            dv.set_DeviceType_Set(hs, DT);
                            dv.set_Interface(hs, IFACE_NAME);
                            dv.set_InterfaceInstance(hs, "");
                            dv.set_Last_Change(hs, new DateTime());
                        }
                        count++;

                        // Zone Last Watered
                        dvRef = hs.NewDeviceRef(z.name);
                        MyDevice = dvRef; //for auto update
                        if (dvRef > 0)
                        {

                            dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, "HOME");
                            //dv.Can_Dim(hs, True
                            dv.set_Device_Type_String(hs, "Rachio Zone" + z.zoneNumber + " Last Watered");
                            DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                            DT.Device_Type = 71;
                            DT.Device_SubType = z.zoneNumber;
                            dv.set_DeviceType_Set(hs, DT);
                            dv.set_Interface(hs, IFACE_NAME);
                            dv.set_InterfaceInstance(hs, "");
                            dv.set_Last_Change(hs, new DateTime());
                        }
                        count++;

                        // Zone Max Runtime
                        dvRef = hs.NewDeviceRef(z.name);
                        MyDevice = dvRef; //for auto update
                        if (dvRef > 0)
                        {

                            dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, "HOME");
                            //dv.Can_Dim(hs, True
                            dv.set_Device_Type_String(hs, "Rachio Zone" + z.zoneNumber + " Max Runtime");
                            DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                            DT.Device_Type = 71;
                            DT.Device_SubType = z.zoneNumber;
                            dv.set_DeviceType_Set(hs, DT);
                            dv.set_Interface(hs, IFACE_NAME);
                            dv.set_InterfaceInstance(hs, "");
                            dv.set_Last_Change(hs, new DateTime());
                        }
                        count++;

                        // Zone Total Runtime
                        dvRef = hs.NewDeviceRef(z.name);
                        MyDevice = dvRef; //for auto update
                        if (dvRef > 0)
                        {

                            dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                            dv.set_Address(hs, "HOME");
                            //dv.Can_Dim(hs, True
                            dv.set_Device_Type_String(hs, "Rachio Zone" + z.zoneNumber + " Total Runtime");
                            DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                            DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                            DT.Device_Type = 71;
                            DT.Device_SubType = z.zoneNumber;
                            dv.set_DeviceType_Set(hs, DT);
                            dv.set_Interface(hs, IFACE_NAME);
                            dv.set_InterfaceInstance(hs, "");
                            dv.set_Last_Change(hs, new DateTime());
                        }
                        count++;
                    }
                    

                }

            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Devices/Create: " + ex.Message);
            }

        }
    }

}
