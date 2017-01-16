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
        static internal int ZoneDeviceValues(Zone z, int i)
        {
            int zoneValue;
            if(i==1)
            {
                if (z.enabled == false)
                    zoneValue = 0;
                else
                    zoneValue = 1;
            }
            else if (i == 2)
            {
                zoneValue = 5;
            }
                
            else if(i == 3)
            {
                zoneValue = 5;
            }
                
            else if (i == 4)
            {
                zoneValue = 3;
            }
                
            else
            {
                zoneValue = z.runtime;
            }
                

            
            return zoneValue;
        }

        static internal void Update_Zone(Zone z, DeviceClass dv, RachioConnection rachio)
        {
            int dvRef;
            CAPI.CAPIControl objCAPIControl;

            dvRef = dv.get_Ref(hs);
            
            for(int i=1;i<6;i++)
            {
                if(z.name.Contains("Control"))
                {
                    if(ZoneDeviceValues(z,i)==1)
                    {
                        objCAPIControl = hs.CAPIGetSingleControlByUse(dvRef, ePairControlUse._On);
                    }
                    else
                        objCAPIControl = hs.CAPIGetSingleControlByUse(dvRef, ePairControlUse._Off);
                }
                else
                hs.SetDeviceValueByRef(dvRef + i - 6, ZoneDeviceValues(z,i), true);
            }
        }

        static string[] zoneStrings = new string[6] { "Control", "Last Duration", "Last Watered", "Max Runtime", "Total Runtime", "Root"};
        static internal void Find_Create_Devices(RachioConnection rachio)
        {

            List<DeviceClass> deviceList = new List<DeviceClass>();
            DeviceClass dv = default(DeviceClass);
            bool Found;
            string testAddress;
            Person p = rachio.getPerson();

            deviceList = Get_Device_List(deviceList, dv);

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

                            Update_Zone(zone, device, rachio);
                            Found = true;
                            break;
                        }
                    }
                    if (!Found)
                    {
                        int dvRef;
                        string dvName;
                        if (true)
                        {
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
                                dv.set_Address(hs, "RACHIOSIID");
                                dv.set_Code(hs, zone.name + "-" + zString);
                                dv.set_Location(hs, "RachioSIID");
                                dv.set_Location2(hs, "RachioSIID");
                                
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
                                    SPair.ControlUse = ePairControlUse.Not_Specified;
                                    
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

                                    VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                                    Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                                    Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                    Pair.Value = 0;
                                    Pair.Status = "Off";
                                    Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                                    Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                                    Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                    Pair.Value = 1;
                                    Pair.Status = "On";
                                    Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                                    VSVGPairs.VGPair GPair = new VSVGPairs.VGPair();
                                    GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                    GPair.Set_Value = 0;
                                    GPair.Graphic = "/images/hspi_ultrarachio3/device_offline.png";
                                    hs.DeviceVGP_AddPair(dvRef, GPair);



                                    GPair = new VSVGPairs.VGPair();
                                    GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                                    GPair.Set_Value = 1;
                                    GPair.Graphic = "/images/hspi_ultrarachio3/device_online.png";
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
                                    GPair.Graphic = "/images/hspi_ultrarachio3/schedule_rule_disabled.png.";
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
                }
                catch (Exception)
                {

                    throw;
                }



                /*try
                {
                    if (!Found)
                    {
                        int dvRef = 0;
                        int count = 0;
                        Console.WriteLine("Creating Devices...");

                        // ALL DEVICES

                        dv.set_Address(hs, "RACHIOSIID");
                        dv.set_Code(hs, z.name);
                        dv.set_Location(hs, "Rachio_SIID");
                        dv.set_Location2(hs, "Rachio_SIID");
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        //dv.MISC_Set(hs,Enums.dvMISC.HIDDEN);
                        dv.set_Status_Support(hs, true);

                        // ZONES
                        
                            Console.WriteLine(z.name);
                            //Zone Root
                            dvRef = hs.NewDeviceRef(z.name);
                            MyDevice = dvRef; //for auto update
                            if (dvRef > 0)
                            {

                                dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);

                                dv.set_Code(hs, dvRef.ToString());
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

                            
                        


                    }

                }
                catch (Exception ex)
                {
                    hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Devices/Create: " + ex.Message);
                }
            */

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
    }

}
