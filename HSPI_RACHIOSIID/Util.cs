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
        public const string IFACE_NAME = "RachioSIID";
        //public const string IFACE_NAME = "RachioSIID";
        // set when SupportMultipleInstances is TRUE
        public static string Instance = "";
        public static string gEXEPath = "";

        public static bool gGlobalTempScaleF = true;
        public static System.Collections.SortedList colTrigs_Sync;
        public static System.Collections.SortedList colTrigs;
        public static System.Collections.SortedList colActs_Sync;

        public static System.Collections.SortedList colActs;
        private static System.Threading.AutoResetEvent Demo_ARE;

        private static System.Threading.Thread Demo_Thread;
        public static bool StringIsNullOrEmpty(ref string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;
            return string.IsNullOrEmpty(s.Trim());
        }


        static internal void Demo_Start()
        {
            bool StartIt = false;
            if (Demo_ARE == null)
            {
                Demo_ARE = new System.Threading.AutoResetEvent(false);
            }
            if (Demo_Thread == null)
            {
                StartIt = true;
            }
            else if (!Demo_Thread.IsAlive)
            {
                StartIt = true;
            }
            if (!StartIt)
                return;

            Demo_Thread = new System.Threading.Thread(Demo_Proc);
            Demo_Thread.Name = "Sample Plug-In Number Generator for Trigger Demonstration";
            Demo_Thread.Priority = System.Threading.ThreadPriority.BelowNormal;
            Demo_Thread.Start();

        }

        static internal int GetDecimals(double D)
        {
            string s = "";
            char[] c = new char[1];
            c[0] = '0';
            // Trailing zeros to be removed.
            D = Math.Abs(D) - Math.Abs(Math.Truncate(D));
            // Remove the whole number so the result always starts with "0." which is a known quantity.
            s = D.ToString("F30");
            s = s.TrimEnd(c);
            return s.Length - 2;
            // Minus 2 because that is the length of "0."
        }

        static internal Random RNum = new Random(2000);
        static internal double Demo_Generate_Weight()
        {
            int Mult = 0;
            double W = 0;
            // The sole purpose of this procedure is to generate random weights
            //   for the purpose of testing the triggers and actions in this plug-in.

            try
            {
                do
                {
                    Mult = RNum.Next(3);
                } while (!(Mult > 0));
                W = (RNum.NextDouble() * 2001) * Mult;
                // Generates a random weight between 0 and 6003 lbs.
            }
            catch (Exception ex)
            {
                Log(IFACE_NAME + " Error: Exception in demo number generation for Trigger 1: " + ex.Message, LogType.LOG_TYPE_WARNING);
            }

            return W;

        }

        internal class Volt_Demo
        {

            public Volt_Demo(bool Euro) : base()
            {
                mvarVoltTypeEuro = Euro;
            }

            private static bool mvarVoltTypeEuro;
            private double mvarVoltage;
            private double mvarAverageVoltage;
            private double mvarSumVoltage;
            private System.DateTime mvarStart = System.DateTime.MinValue;

            private int mvarCount;
            public double Voltage
            {
                get { return mvarVoltage; }
            }
            public double AverageVoltage
            {
                get { return mvarAverageVoltage; }
            }
            public System.DateTime AverageSince
            {
                get { return mvarStart; }
            }
            public int AverageCount
            {
                get { return mvarCount; }
            }

            internal void ResetAverage()
            {
                mvarSumVoltage = 0;
                mvarAverageVoltage = 0;
                mvarCount = 0;
            }


            internal Random RNum = new Random((mvarVoltTypeEuro ? 220 : 110));

            internal void Demo_Generate_Value()
            {
                // The sole purpose of this procedure is to generate random voltages for 
                //   purposes of testing the triggers and actions in this plug-in.


                try
                {
                    if (HSPI.bShutDown)
                        return;

                    // Initialize time if it has not been done.
                    if (mvarStart == System.DateTime.MinValue)
                    {
                        mvarStart = DateTime.Now;
                    }

                    mvarCount += 1;

                    if (mvarVoltTypeEuro)
                    {
                        do
                        {
                            mvarVoltage = (RNum.NextDouble() * 240) + RNum.Next(20);
                        } while (mvarVoltage < 205);
                        Util.Log("Voltage (European) = " + mvarVoltage.ToString(), LogType.LOG_TYPE_INFO);
                    }
                    else
                    {
                        do
                        {
                            mvarVoltage = (RNum.NextDouble() * 120) + RNum.Next(10);
                        } while (mvarVoltage < 100);
                        Util.Log("Voltage (North American) = " + mvarVoltage.ToString(), LogType.LOG_TYPE_INFO);
                    }
                    mvarSumVoltage += mvarVoltage;
                    mvarAverageVoltage = mvarSumVoltage / mvarCount;

                    if (double.MaxValue - mvarSumVoltage <= 300)
                    {
                        mvarStart = DateTime.Now;
                        mvarSumVoltage = mvarVoltage;
                        mvarCount = 1;
                    }

                }
                catch (Exception ex)
                {
                    Util.Log(Util.IFACE_NAME + " Error: Exception in Value generation for Trigger 2: " + ex.Message, LogType.LOG_TYPE_WARNING);
                }

            }

        }

        static internal Volt_Demo Volt = null;

        static internal Volt_Demo VoltEuro = null;
        static internal void Demo_Proc()
        {
            Classes.MyTrigger1Ton Trig1 = null;
            Classes.MyTrigger2Shoe Trig2 = null;
            Random RND = new Random(1000);
            int T = 0;
            double Weight = 0;
            double WeightEven = 0;

            try
            {
                do
                {
                    if (HSPI.bShutDown)
                        break;

                    if (colTrigs == null)
                    {
                        Demo_ARE.WaitOne(10000);
                        if (HSPI.bShutDown)
                            break;
                        continue;
                    }
                    if (colTrigs.Count < 1)
                    {
                        Demo_ARE.WaitOne(10000);
                        if (HSPI.bShutDown)
                            break;
                        continue;
                    }

                    T = RND.Next(10000, 30000);
                    if (HSPI.bShutDown)
                        break;
                    Demo_ARE.WaitOne(T);
                    if (HSPI.bShutDown)
                        break;

                    Weight = Demo_Generate_Weight();
                    int i = 0;
                    i = Convert.ToInt32(Weight / 2000);
                    WeightEven = i * 2000;

                    Log("----------------------------------------------------------------", LogType.LOG_TYPE_INFO);
                    Log("Weight = " + Weight.ToString() + ", Even = " + WeightEven.ToString(), LogType.LOG_TYPE_INFO);

                    IPlugInAPI.strTrigActInfo[] TrigsToCheck = null;
                    strTrigger Trig = default(strTrigger);

                    // We have generated a new Weight, so let's see if we need to trigger events.
                    try
                    {
                        // Step 1: Ask HomeSeer for any triggers that are for this plug-in and are Type 1
                        TrigsToCheck = null;
                        TrigsToCheck = callback.TriggerMatches(Util.IFACE_NAME, 1, -1);
                    }
                    catch (Exception)
                    {
                    }
                    // Step 2: If triggers were returned, we need to check them against our trigger value.
                    if (TrigsToCheck != null && TrigsToCheck.Length > 0)
                    {
                        foreach (IPlugInAPI.strTrigActInfo TC in TrigsToCheck)
                        {
                            if (TC.DataIn != null && TC.DataIn.Length > 10)
                            {
                                Trig = TriggerFromData(TC.DataIn);
                            }
                            else
                            {
                                Trig = TriggerFromInfo(TC);
                            }
                            if (!Trig.Result)
                                continue;
                            if (Trig.TrigObj == null)
                                continue;
                            if (Trig.WhichTrigger != eTriggerType.OneTon)
                                continue;
                            try
                            {
                                Trig1 = (Classes.MyTrigger1Ton)Trig.TrigObj;
                            }
                            catch (Exception)
                            {
                                Trig1 = null;
                            }
                            if (Trig1 == null)
                                continue;
                            if (Trig1.EvenTon)
                            {
                                Log("Checking Weight Trigger (Even), " + WeightEven.ToString() + " against trigger of " + Trig1.TriggerWeight.ToString(), LogType.LOG_TYPE_INFO);
                                if (WeightEven > Trig1.TriggerWeight)
                                {
                                    Log("Weight trigger is TRUE - calling FIRE! for event ID " + TC.evRef.ToString(), LogType.LOG_TYPE_WARNING);
                                    // Step 3: If a trigger matches, call FIRE!
                                    callback.TriggerFire(Util.IFACE_NAME, TC);
                                }
                            }
                            else
                            {
                                Log("Checking Weight Trigger, " + Weight.ToString() + " against trigger of " + Trig1.TriggerWeight.ToString(), LogType.LOG_TYPE_INFO);
                                if (Weight > Trig1.TriggerWeight)
                                {
                                    Log("Weight trigger is TRUE - calling FIRE! for event ID " + TC.evRef.ToString(), LogType.LOG_TYPE_WARNING);
                                    callback.TriggerFire(Util.IFACE_NAME, TC);
                                    // Step 3: If a trigger matches, call FIRE!
                                }
                            }
                        }
                    }


                    if (Volt == null)
                        Volt = new Volt_Demo(false);
                    if (VoltEuro == null)
                        VoltEuro = new Volt_Demo(true);
                    Volt.Demo_Generate_Value();
                    VoltEuro.Demo_Generate_Value();


                    // We have generated a new Voltage, so let's see if we need to trigger events.
                    try
                    {
                        // Step 1: Ask HomeSeer for any triggers that are for this plug-in and are Type 2, SubType 1
                        TrigsToCheck = null;
                        TrigsToCheck = callback.TriggerMatches(Util.IFACE_NAME, 2, 1);
                    }
                    catch (Exception)
                    {
                    }
                    // Step 2: If triggers were returned, we need to check them against our trigger value.
                    if (TrigsToCheck != null && TrigsToCheck.Length > 0)
                    {
                        foreach (IPlugInAPI.strTrigActInfo TC in TrigsToCheck)
                        {
                            if (TC.DataIn != null && TC.DataIn.Length > 10)
                            {
                                Trig = TriggerFromData(TC.DataIn);
                            }
                            else
                            {
                                Trig = TriggerFromInfo(TC);
                            }
                            if (!Trig.Result)
                                continue;
                            if (Trig.TrigObj == null)
                                continue;
                            if (Trig.WhichTrigger != eTriggerType.TwoVolts)
                                continue;
                            try
                            {
                                Trig2 = (Classes.MyTrigger2Shoe)Trig.TrigObj;
                            }
                            catch (Exception)
                            {
                                Trig2 = null;
                            }
                            if (Trig2 == null)
                                continue;
                            if (Trig2.SubTrigger2)
                                continue;
                            // Only checking SubType 1 right now.
                            if (Trig2.EuroVoltage)
                            {
                                Log("Checking Voltage Trigger: " + Math.Round(VoltEuro.Voltage, GetDecimals(Trig2.TriggerValue)).ToString() + " vs Trigger Value " + Trig2.TriggerValue.ToString(), LogType.LOG_TYPE_INFO);
                                if (Math.Round(VoltEuro.Voltage, GetDecimals(Trig2.TriggerValue)) == Trig2.TriggerValue)
                                {
                                    // Step 3: If a trigger matches, call FIRE!
                                    Log("Voltage trigger is TRUE - calling FIRE! for event ID " + TC.evRef.ToString(), LogType.LOG_TYPE_WARNING);
                                    callback.TriggerFire(Util.IFACE_NAME, TC);
                                }
                            }
                            else
                            {
                                Log("Checking Voltage Trigger: " + Math.Round(Volt.Voltage, GetDecimals(Trig2.TriggerValue)).ToString() + " vs Trigger Value " + Trig2.TriggerValue.ToString(), LogType.LOG_TYPE_INFO);
                                if (Math.Round(Volt.Voltage, GetDecimals(Trig2.TriggerValue)) == Trig2.TriggerValue)
                                {
                                    // Step 3: If a trigger matches, call FIRE!
                                    Log("Voltage trigger is TRUE - calling FIRE! for event ID " + TC.evRef.ToString(), LogType.LOG_TYPE_WARNING);
                                    callback.TriggerFire(Util.IFACE_NAME, TC);
                                }
                            }
                        }
                    }


                    // We have generated a new Voltage, so let's see if we need to trigger events.
                    try
                    {
                        // Step 1: Ask HomeSeer for any triggers that are for this plug-in and are Type 2, SubType 2
                        // (We did Type 2 SubType 1 up above)
                        TrigsToCheck = null;
                        TrigsToCheck = callback.TriggerMatches(Util.IFACE_NAME, 2, 2);
                    }
                    catch (Exception)
                    {
                    }
                    // Step 2: If triggers were returned, we need to check them against our trigger value.
                    if (TrigsToCheck != null && TrigsToCheck.Length > 0)
                    {
                        foreach (IPlugInAPI.strTrigActInfo TC in TrigsToCheck)
                        {
                            if (TC.DataIn != null && TC.DataIn.Length > 10)
                            {
                                Trig = TriggerFromData(TC.DataIn);
                            }
                            else
                            {
                                Trig = TriggerFromInfo(TC);
                            }
                            if (!Trig.Result)
                                continue;
                            if (Trig.TrigObj == null)
                                continue;
                            if (Trig.WhichTrigger != eTriggerType.TwoVolts)
                                continue;
                            try
                            {
                                Trig2 = (Classes.MyTrigger2Shoe)Trig.TrigObj;
                            }
                            catch (Exception)
                            {
                                Trig2 = null;
                            }
                            if (Trig2 == null)
                                continue;
                            if (!Trig2.SubTrigger2)
                                continue;
                            // Only checking SubType 2 right now.
                            if (Trig2.EuroVoltage)
                            {
                                Log("Checking Avg Voltage Trigger: " + Math.Round(VoltEuro.AverageVoltage, GetDecimals(Trig2.TriggerValue)).ToString() + " vs Trigger Value " + Trig2.TriggerValue.ToString(), LogType.LOG_TYPE_INFO);
                                if (Math.Round(VoltEuro.AverageVoltage, GetDecimals(Trig2.TriggerValue)) == Trig2.TriggerValue)
                                {
                                    // Step 3: If a trigger matches, call FIRE!
                                    Log("Average Voltage trigger is TRUE - calling FIRE! for event ID " + TC.evRef.ToString(), LogType.LOG_TYPE_WARNING);
                                    callback.TriggerFire(Util.IFACE_NAME, TC);
                                }
                            }
                            else
                            {
                                Log("Checking Avg Voltage Trigger: " + Math.Round(Volt.AverageVoltage, GetDecimals(Trig2.TriggerValue)).ToString() + " vs Trigger Value " + Trig2.TriggerValue.ToString(), LogType.LOG_TYPE_INFO);
                                if (Math.Round(Volt.AverageVoltage, GetDecimals(Trig2.TriggerValue)) == Trig2.TriggerValue)
                                {
                                    // Step 3: If a trigger matches, call FIRE!
                                    Log("Average Voltage trigger is TRUE - calling FIRE! for event ID " + TC.evRef.ToString(), LogType.LOG_TYPE_WARNING);
                                    callback.TriggerFire(Util.IFACE_NAME, TC);
                                }
                            }
                        }
                    }


                } while (true);

            }
            catch (Exception)
            {
            }
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

        internal enum eTriggerType
        {
            OneTon = 1,
            TwoVolts = 2,
            Unknown = 0
        }
        internal enum eActionType
        {
            Unknown = 0,
            Weight = 1,
            Voltage = 2
        }

        internal struct strTrigger
        {
            public eTriggerType WhichTrigger;
            public object TrigObj;
            public bool Result;
        }
        internal struct strAction
        {
            public eActionType WhichAction;
            public object ActObj;
            public bool Result;
        }

        static internal strTrigger TriggerFromData(byte[] Data)
        {
            strTrigger ST = new strTrigger();
            ST.WhichTrigger = eTriggerType.Unknown;
            ST.Result = false;
            if (Data == null)
                return ST;
            if (Data.Length < 1)
                return ST;

            bool bRes = false;
            Classes.MyTrigger1Ton Trig1 = new Classes.MyTrigger1Ton();
            Classes.MyTrigger2Shoe Trig2 = new Classes.MyTrigger2Shoe();
            try
            {
                object objTrig1 = Trig1;
                bRes = DeSerializeObject(Data, ref objTrig1, Trig1.GetType());
                if (bRes) Trig1 = (Classes.MyTrigger1Ton)objTrig1;
            }
            catch (Exception)
            {
                bRes = false;
            }
            if (bRes & Trig1 != null)
            {
                ST.WhichTrigger = eTriggerType.OneTon;
                ST.TrigObj = Trig1;
                ST.Result = true;
                return ST;
            }
            try
            {
                object objTrig2 = Trig2;
                bRes = DeSerializeObject(Data, ref objTrig2, Trig2.GetType());
                if (bRes) Trig2 = (Classes.MyTrigger2Shoe)objTrig2;
            }
            catch (Exception)
            {
                bRes = false;
            }
            if (bRes & Trig2 != null)
            {
                ST.WhichTrigger = eTriggerType.TwoVolts;
                ST.TrigObj = Trig2;
                ST.Result = true;
                return ST;
            }
            ST.WhichTrigger = eTriggerType.Unknown;
            ST.TrigObj = null;
            ST.Result = false;
            return ST;
        }

        static internal strAction ActionFromData(byte[] Data)
        {
            strAction ST = new strAction();
            ST.WhichAction = eActionType.Unknown;
            ST.Result = false;
            if (Data == null)
                return ST;
            if (Data.Length < 1)
                return ST;

            bool bRes = false;
            Classes.MyAction1EvenTon Act1 = new Classes.MyAction1EvenTon();
            Classes.MyAction2Euro Act2 = new Classes.MyAction2Euro();
            try
            {
                object objAct1 = Act1;
                bRes = DeSerializeObject(Data, ref objAct1, Act1.GetType());
                if (bRes) Act1 = (Classes.MyAction1EvenTon)objAct1;
            }
            catch (Exception)
            {
                bRes = false;
            }
            if (bRes & Act1 != null)
            {
                ST.WhichAction = eActionType.Weight;
                ST.ActObj = Act1;
                ST.Result = true;
                return ST;
            }
            try
            {
                object objAct2 = Act2;
                bRes = DeSerializeObject(Data, ref objAct2, Act2.GetType());
                if (bRes) Act2 = (Classes.MyAction2Euro)objAct2;
            }
            catch (Exception)
            {
                bRes = false;
            }
            if (bRes & Act2 != null)
            {
                ST.WhichAction = eActionType.Voltage;
                ST.ActObj = Act2;
                ST.Result = true;
                return ST;
            }
            ST.WhichAction = eActionType.Unknown;
            ST.ActObj = null;
            ST.Result = false;
            return ST;
        }

        public static void Add_Update_Trigger(object Trig)
        {
            if (Trig == null)
                return;
            string sKey = "";
            if (Trig is Classes.MyTrigger1Ton)
            {
                Classes.MyTrigger1Ton Trig1 = null;
                try
                {
                    Trig1 = (Classes.MyTrigger1Ton)Trig;
                }
                catch (Exception)
                {
                    Trig1 = null;
                }
                if (Trig1 != null)
                {
                    if (Trig1.TriggerUID < 1)
                        return;
                    sKey = "K" + Trig1.TriggerUID.ToString();
                    if (colTrigs.ContainsKey(sKey))
                    {
                        lock (colTrigs.SyncRoot)
                        {
                            colTrigs.Remove(sKey);
                        }
                    }
                    colTrigs.Add(sKey, Trig1);
                }
            }
            else if (Trig is Classes.MyTrigger2Shoe)
            {
                Classes.MyTrigger2Shoe Trig2 = null;
                try
                {
                    Trig2 = (Classes.MyTrigger2Shoe)Trig;
                }
                catch (Exception)
                {
                    Trig2 = null;
                }
                if (Trig2 != null)
                {
                    if (Trig2.TriggerUID < 1)
                        return;
                    sKey = "K" + Trig2.TriggerUID.ToString();
                    if (colTrigs.ContainsKey(sKey))
                    {
                        lock (colTrigs.SyncRoot)
                        {
                            colTrigs.Remove(sKey);
                        }
                    }
                    colTrigs.Add(sKey, Trig2);
                }
            }
        }

        public static void Add_Update_Action(object Act)
        {
            if (Act == null)
                return;
            string sKey = "";
            if (Act is Classes.MyAction1EvenTon)
            {
                Classes.MyAction1EvenTon Act1 = null;
                try
                {
                    Act1 = (Classes.MyAction1EvenTon)Act;
                }
                catch (Exception)
                {
                    Act1 = null;
                }
                if (Act1 != null)
                {
                    if (Act1.ActionUID < 1)
                        return;
                    sKey = "K" + Act1.ActionUID.ToString();
                    if (colActs.ContainsKey(sKey))
                    {
                        lock (colActs.SyncRoot)
                        {
                            colActs.Remove(sKey);
                        }
                    }
                    colActs.Add(sKey, Act1);
                }
            }
            else if (Act is Classes.MyAction2Euro)
            {
                Classes.MyAction2Euro Act2 = null;
                try
                {
                    Act2 = (Classes.MyAction2Euro)Act;
                }
                catch (Exception)
                {
                    Act2 = null;
                }
                if (Act2 != null)
                {
                    if (Act2.ActionUID < 1)
                        return;
                    sKey = "K" + Act2.ActionUID.ToString();
                    if (colActs.ContainsKey(sKey))
                    {
                        lock (colActs.SyncRoot)
                        {
                            colActs.Remove(sKey);
                        }
                    }
                    colActs.Add(sKey, Act2);
                }
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
                            if (DT.Device_API == DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat && DT.Device_Type == (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Temperature)
                            {
                                // this is our temp device
                                Found = true;
                                MyTempDevice = dev.get_Ref(null);
                                hs.SetDeviceValueByRef(dev.get_Ref(null), 72, false);

                            }

                            if (DT.Device_API == DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat && DT.Device_Type == (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Setpoint)
                            {
                                Found = true;
                                if (DT.Device_SubType == (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Setpoint.Heating_1)
                                {
                                    hs.SetDeviceValueByRef(dv.get_Ref(null), 68, false);
                                }
                            }

                            if (DT.Device_API == DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat && DT.Device_Type == (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Setpoint)
                            {
                                Found = true;
                                if (DT.Device_SubType == (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Setpoint.Cooling_1)
                                {
                                    hs.SetDeviceValueByRef(dv.get_Ref(null), 75, false);
                                }
                            }

                            if (DT.Device_API == DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In && DT.Device_Type == 69)
                            {
                                Found = true;
                                MyDevice = dev.get_Ref(null);

                                // Now (mostly for demonstration purposes) - work with the PlugExtraData object.
                                PlugExtraData.clsPlugExtraData EDO = null;
                                EDO = dev.get_PlugExtraData_Get(null);
                                if (EDO != null)
                                {
                                    object obj = null;
                                    obj = EDO.GetNamed("My Special Object");
                                    if (obj != null)
                                    {
                                        Log("Plug-In Extra Data Object Retrieved = " + obj.ToString(), LogType.LOG_TYPE_INFO);
                                    }
                                    obj = EDO.GetNamed("My Count");
                                    int MC = 1;
                                    if (obj == null)
                                    {
                                        if (!EDO.AddNamed("My Count", MC))
                                        {
                                            Log("Error adding named data object to plug-in sample device!", LogType.LOG_TYPE_ERROR);
                                            break;
                                        }
                                        dev.set_PlugExtraData_Set(hs, EDO);
                                        hs.SaveEventsDevices();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            MC = Convert.ToInt32(obj);
                                        }
                                        catch (Exception)
                                        {
                                            MC = -1;
                                        }
                                        if (MC < 0)
                                            break;
                                        Log("Retrieved count from plug-in sample device is: " + MC.ToString(), LogType.LOG_TYPE_INFO);
                                        MC += 1;
                                        // Now put it back - need to remove the old one first.
                                        EDO.RemoveNamed("My Count");
                                        EDO.AddNamed("My Count", MC);
                                        dev.set_PlugExtraData_Set(hs, EDO);
                                        hs.SaveEventsDevices();
                                    }
                                }


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
                    VSVGPairs.VGPair GPair = null;
                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with buttons and slider");
                    if (dvRef > 0)
                    {
                        MyDevice = dvRef;
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        string[] disp = new string[2];
                        disp[0] = "This is a test of the";
                        disp[1] = "Emergency Broadcast Display Data system";
                        dv.set_AdditionalDisplayData(hs, disp);
                        dv.set_Address(hs, "HOME");
                        dv.set_Code(hs, "A1");
                        // set a code if needed, but not required
                        //dv.Can_Dim(hs) = True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 69;
                        // our own device type
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();
                        dv.set_PlugExtraData_Set(hs, EDO);
                        // Now just for grins, let's modify it.
                        string HW = "Hello World";
                        if (EDO.GetNamed("My Special Object") != null)
                        {
                            EDO.RemoveNamed("My Special Object");
                        }
                        EDO.AddNamed("My Special Object", HW);
                        // Need to re-save it.
                        dv.set_PlugExtraData_Set(hs, EDO);

                        // add an ON button and value
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 0;
                        Pair.Status = "Off";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Pair.Render_Location.Row = 1;
                        Pair.Render_Location.Column = 1;
                        Pair.ControlUse = ePairControlUse._Off;
                        // set this for UI apps like HSTouch so they know this is for OFF
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 0;
                        GPair.Graphic = "/images/HomeSeer/status/off.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        // add DIM values
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        Pair.ControlUse = ePairControlUse._Dim;
                        // set this for UI apps like HSTouch so they know this is for lighting control dimming
                        Pair.RangeStart = 1;
                        Pair.RangeEnd = 99;
                        Pair.RangeStatusPrefix = "Dim ";
                        Pair.RangeStatusSuffix = "%";
                        Pair.Render = Enums.CAPIControlType.ValuesRangeSlider;
                        Pair.Render_Location.Row = 2;
                        Pair.Render_Location.Column = 1;
                        Pair.Render_Location.ColumnSpan = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        // add graphic pairs for the dim levels
                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 1;
                        GPair.RangeEnd = 5.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-00.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 6;
                        GPair.RangeEnd = 15.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-10.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 16;
                        GPair.RangeEnd = 25.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-20.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 26;
                        GPair.RangeEnd = 35.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-30.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 36;
                        GPair.RangeEnd = 45.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-40.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 46;
                        GPair.RangeEnd = 55.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-50.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 56;
                        GPair.RangeEnd = 65.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-60.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 66;
                        GPair.RangeEnd = 75.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-70.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 76;
                        GPair.RangeEnd = 85.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-80.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 86;
                        GPair.RangeEnd = 95.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/dim-90.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.Range;
                        GPair.RangeStart = 96;
                        GPair.RangeEnd = 98.99999999;
                        GPair.Graphic = "/images/HomeSeer/status/on.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        // add an OFF button and value
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 100;
                        Pair.Status = "On";
                        Pair.ControlUse = ePairControlUse._On;
                        // set this for UI apps like HSTouch so they know this is for lighting control ON
                        Pair.Render = Enums.CAPIControlType.Button;
                        Pair.Render_Location.Row = 1;
                        Pair.Render_Location.Column = 2;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 100;
                        GPair.Graphic = "/images/HomeSeer/status/on.gif";
                        hs.DeviceVGP_AddPair(dvRef, GPair);

                        // add an last level button and value
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 255;
                        Pair.Status = "On Last Level";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Pair.Render_Location.Row = 1;
                        Pair.Render_Location.Column = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        // add a button that executes a special command but does not actually set any device value, here we will speak something
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Control);
                        // set the type to CONTROL only so that this value will never be displayed as a status
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 1000;
                        // we use a value that is not used as a status, this value will be handled in SetIOMult, see that function for the handling
                        Pair.Status = "Speak Hello";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Pair.Render_Location.Row = 1;
                        Pair.Render_Location.Column = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);




                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        PlugExtraData.clsPlugExtraData PED = dv.get_PlugExtraData_Get(hs);
                        if (PED == null)
                            PED = new PlugExtraData.clsPlugExtraData();
                        PED.AddNamed("Test", new bool());
                        PED.AddNamed("Device", dv);
                        dv.set_PlugExtraData_Set(hs, PED);
                        dv.set_Status_Support(hs, true);
                        dv.set_UserNote(hs, "This is my user note - how do you like it? This device is version " + dv.Version.ToString());
                        //hs.SetDeviceString(ref, "Not Set", False)  ' this will override the name/value pairs
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with list values");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.set_Can_Dim(hs) = True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 70;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add list values, will appear as drop list control
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.Values;
                        Pair.Value = 1;
                        Pair.Status = "1";
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.Values;
                        Pair.Value = 2;
                        Pair.Status = "2";
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        dv.set_Status_Support(hs, true);
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with radio type control");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 71;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add values, will appear as a radio control and only allow one option to be selected at a time
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.Radio_Option;
                        Pair.Value = 1;
                        Pair.Status = "Value 1";
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 2;
                        Pair.Status = "Value 2";
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 3;
                        Pair.Status = "Value 3";
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        dv.set_Status_Support(hs, true);
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with list text single selection");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 72;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add list values, will appear as drop list control
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.Single_Text_from_List;
                        Pair.Value = 1;
                        Pair.Status = "String 1";
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.Single_Text_from_List;
                        Pair.Value = 2;
                        Pair.Status = "String 2";
                        hs.DeviceVSP_AddPair(dvRef, Pair);



                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        dv.set_Status_Support(hs, true);
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with list text multiple selection");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 73;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add list values, will appear as drop list control
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Control);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.List_Text_from_List;
                        Pair.StringListAdd = "String 1";
                        Pair.StringListAdd = "String 2";
                        hs.DeviceVSP_AddPair(dvRef, Pair);





                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        dv.set_Status_Support(hs, true);
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with text box text");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.set_Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "Sample Device with textbox input");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 74;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add text value it will appear in an editable text box
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.TextBox_String;
                        Pair.Value = 0;
                        Pair.Status = "Default Text";
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        dv.set_Status_Support(hs, true);
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with text box number");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.set_Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "Sample Device with textbox input");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 75;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add text value it will appear in an editable text box
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Render = Enums.CAPIControlType.TextBox_Number;
                        Pair.Value = 0;
                        Pair.Status = "Default Number";
                        Pair.Value = 0;
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);
                        //dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                        dv.set_Status_Support(hs, true);
                    }

                    // this demonstrates some controls that are displayed in a pop-up dialog on the device utility page
                    // this device is also set so the values/graphics pairs cannot be edited and no graphics displays for the status
                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with pop-up control");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.set_Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 76;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");



                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        // add an OFF button and value
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 0;
                        Pair.Status = "Off";
                        Pair.Render = Enums.CAPIControlType.Button;
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        // add an ON button and value

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 100;
                        Pair.Status = "On";
                        Pair.Render = Enums.CAPIControlType.Button;
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        // add DIM values
                        Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        Pair.RangeStart = 1;
                        Pair.RangeEnd = 99;
                        Pair.RangeStatusPrefix = "Dim ";
                        Pair.RangeStatusSuffix = "%";
                        Pair.Render = Enums.CAPIControlType.ValuesRangeSlider;

                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP);
                        // cause control to be displayed in a pop-up dialog
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);

                        dv.set_Status_Support(hs, true);
                    }

                    // this is a device that pop-ups and uses row/column attributes to position the controls on the form
                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device with pop-up control row/column");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 77;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");


                        // add an array of buttons formatted like a number pad
                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 1;
                        Pair.Status = "1";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Pair.Render_Location.Column = 1;
                        Pair.Render_Location.Row = 1;
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        Pair.Value = 2;
                        Pair.Status = "2";
                        Pair.Render_Location.Column = 2;
                        Pair.Render_Location.Row = 1;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 3;
                        Pair.Status = "3";
                        Pair.Render_Location.Column = 3;
                        Pair.Render_Location.Row = 1;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 4;
                        Pair.Status = "4";
                        Pair.Render_Location.Column = 1;
                        Pair.Render_Location.Row = 2;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 5;
                        Pair.Status = "5";
                        Pair.Render_Location.Column = 2;
                        Pair.Render_Location.Row = 2;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 6;
                        Pair.Status = "6";
                        Pair.Render_Location.Column = 3;
                        Pair.Render_Location.Row = 2;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 7;
                        Pair.Status = "7";
                        Pair.Render_Location.Column = 1;
                        Pair.Render_Location.Row = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 8;
                        Pair.Status = "8";
                        Pair.Render_Location.Column = 2;
                        Pair.Render_Location.Row = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 9;
                        Pair.Status = "9";
                        Pair.Render_Location.Column = 3;
                        Pair.Render_Location.Row = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 10;
                        Pair.Status = "*";
                        Pair.Render_Location.Column = 1;
                        Pair.Render_Location.Row = 4;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 0;
                        Pair.Status = "0";
                        Pair.Render_Location.Column = 2;
                        Pair.Render_Location.Row = 4;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 11;
                        Pair.Status = "#";
                        Pair.Render_Location.Column = 3;
                        Pair.Render_Location.Row = 4;
                        hs.DeviceVSP_AddPair(dvRef, Pair);
                        Pair.Value = 12;
                        Pair.Status = "Clear";
                        Pair.Render_Location.Column = 1;
                        Pair.Render_Location.Row = 5;
                        Pair.Render_Location.ColumnSpan = 3;
                        hs.DeviceVSP_AddPair(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP);
                        // cause control to be displayed in a pop-up dialog
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);

                        dv.set_Status_Support(hs, true);
                    }

                    // this device is created so that no graphics are displayed and the value/graphics pairs cannot be edited
                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Device no graphics");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        //dv.set_Can_Dim(hs, True
                        dv.set_Device_Type_String(hs, "My Sample Device");
                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                        DT.Device_Type = 76;
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        dv.MISC_Set(hs, Enums.dvMISC.NO_GRAPHICS_DISPLAY);
                        // causes no graphics to display and value/graphics pairs cannot be edited
                        dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP);
                        // cause control to be displayed in a pop-up dialog
                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);

                        dv.set_Status_Support(hs, true);
                    }

                    // build a thermostat device group,all of the following thermostat devices are grouped under this root device
                    gGlobalTempScaleF = Convert.ToBoolean(hs.GetINISetting("Settings", "gGlobalTempScaleF", "True").Trim());
                    // get the F or C setting from HS setup
                    Scheduler.Classes.DeviceClass therm_root_dv = null;
                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Root Device");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        therm_root_dv = dv;
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Z-Wave Thermostat");
                        // this device type is set up in the default HSTouch projects so we set it here so the default project displays
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Root;
                        DT.Device_SubType = 0;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY);
                        dv.set_Relationship(hs, Enums.eRelationship.Parent_Root);

                        hs.SaveEventsDevices();
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Fan Device");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Fan");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Fan_Mode_Set;
                        DT.Device_SubType = 0;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 0;
                        Pair.Status = "Auto";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 1;
                        Pair.Status = "On";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.set_Status_Support(hs, true);
                        hs.SaveEventsDevices();
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Mode Device");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Mode");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Operating_Mode;
                        DT.Device_SubType = 0;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 0;
                        Pair.Status = "Off";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 1;
                        Pair.Status = "Heat";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);
                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 1;
                        GPair.Graphic = "/images/HomeSeer/status/Heat.png";
                        Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 2;
                        Pair.Status = "Cool";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);
                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 2;
                        GPair.Graphic = "/images/HomeSeer/status/Cool.png";
                        Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 3;
                        Pair.Status = "Auto";
                        Pair.Render = Enums.CAPIControlType.Button;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);
                        GPair = new VSVGPairs.VGPair();
                        GPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        GPair.Set_Value = 3;
                        GPair.Graphic = "/images/HomeSeer/status/Auto.png";
                        Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.set_Status_Support(hs, true);
                        hs.SaveEventsDevices();
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Heat Setpoint");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Heat Setpoint");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Setpoint;
                        DT.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Setpoint.Heating_1;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        Pair.RangeStart = -2147483648L;
                        Pair.RangeEnd = 2147483647;
                        Pair.RangeStatusPrefix = "";
                        Pair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                        Pair.IncludeValues = true;
                        Pair.ValueOffset = 0;
                        Pair.RangeStatusDecimals = 0;
                        Pair.HasScale = true;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Control);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        // 39F = 4C
                        // 50F = 10C
                        // 90F = 32C
                        if (gGlobalTempScaleF)
                        {
                            Pair.RangeStart = 50;
                            Pair.RangeEnd = 90;
                        }
                        else
                        {
                            Pair.RangeStart = 10;
                            Pair.RangeEnd = 32;
                        }
                        Pair.RangeStatusPrefix = "";
                        Pair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                        Pair.IncludeValues = true;
                        Pair.ValueOffset = 0;
                        Pair.RangeStatusDecimals = 0;
                        Pair.HasScale = true;
                        Pair.Render = Enums.CAPIControlType.TextBox_Number;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        // The scale does not matter because the global temperature scale setting
                        //   will override and cause the temperature to always display in the user's
                        //   selected scale, so use that in setting up the ranges.
                        //If dv.ZWData.Sensor_Scale = 1 Then  ' Fahrenheit
                        if (gGlobalTempScaleF)
                        {
                            // Set up the ranges for Fahrenheit
                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -50;
                            GPair.RangeEnd = 5;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 5.00000001;
                            GPair.RangeEnd = 15.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 16;
                            GPair.RangeEnd = 25.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 26;
                            GPair.RangeEnd = 35.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 36;
                            GPair.RangeEnd = 45.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 46;
                            GPair.RangeEnd = 55.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 56;
                            GPair.RangeEnd = 65.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 66;
                            GPair.RangeEnd = 75.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 76;
                            GPair.RangeEnd = 85.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 86;
                            GPair.RangeEnd = 95.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 96;
                            GPair.RangeEnd = 104.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 105;
                            GPair.RangeEnd = 150.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        }
                        else
                        {
                            // Celsius
                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -45;
                            GPair.RangeEnd = -15;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -14.999999;
                            GPair.RangeEnd = -9.44;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -9.43999999;
                            GPair.RangeEnd = -3.88;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -3.8799999;
                            GPair.RangeEnd = 1.66;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 1.67;
                            GPair.RangeEnd = 7.22;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 7.23;
                            GPair.RangeEnd = 12.77;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 12.78;
                            GPair.RangeEnd = 18.33;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 18.34;
                            GPair.RangeEnd = 23.88;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 23.89;
                            GPair.RangeEnd = 29.44;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 29.45;
                            GPair.RangeEnd = 35;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 35.0000001;
                            GPair.RangeEnd = 40;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 40.0000001;
                            GPair.RangeEnd = 75;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        }

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.set_Status_Support(hs, true);
                        hs.SaveEventsDevices();
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Cool Setpoint");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Cool Setpoint");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Setpoint;
                        DT.Device_SubType = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceSubType_Setpoint.Cooling_1;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        Pair.RangeStart = -2147483648L;
                        Pair.RangeEnd = 2147483647;
                        Pair.RangeStatusPrefix = "";
                        Pair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                        Pair.IncludeValues = true;
                        Pair.ValueOffset = 0;
                        Pair.RangeStatusDecimals = 0;
                        Pair.HasScale = true;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Control);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        // 39F = 4C
                        // 50F = 10C
                        // 90F = 32C
                        if (gGlobalTempScaleF)
                        {
                            Pair.RangeStart = 50;
                            Pair.RangeEnd = 90;
                        }
                        else
                        {
                            Pair.RangeStart = 10;
                            Pair.RangeEnd = 32;
                        }
                        Pair.RangeStatusPrefix = "";
                        Pair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                        Pair.IncludeValues = true;
                        Pair.ValueOffset = 0;
                        Pair.RangeStatusDecimals = 0;
                        Pair.HasScale = true;
                        Pair.Render = Enums.CAPIControlType.TextBox_Number;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        // The scale does not matter because the global temperature scale setting
                        //   will override and cause the temperature to always display in the user's
                        //   selected scale, so use that in setting up the ranges.
                        //If dv.ZWData.Sensor_Scale = 1 Then  ' Fahrenheit
                        if (gGlobalTempScaleF)
                        {
                            // Set up the ranges for Fahrenheit
                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -50;
                            GPair.RangeEnd = 5;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 5.00000001;
                            GPair.RangeEnd = 15.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 16;
                            GPair.RangeEnd = 25.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 26;
                            GPair.RangeEnd = 35.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 36;
                            GPair.RangeEnd = 45.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 46;
                            GPair.RangeEnd = 55.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 56;
                            GPair.RangeEnd = 65.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 66;
                            GPair.RangeEnd = 75.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 76;
                            GPair.RangeEnd = 85.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 86;
                            GPair.RangeEnd = 95.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 96;
                            GPair.RangeEnd = 104.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 105;
                            GPair.RangeEnd = 150.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        }
                        else
                        {
                            // Celsius
                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -45;
                            GPair.RangeEnd = -15;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -14.999999;
                            GPair.RangeEnd = -9.44;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -9.43999999;
                            GPair.RangeEnd = -3.88;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -3.8799999;
                            GPair.RangeEnd = 1.66;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 1.67;
                            GPair.RangeEnd = 7.22;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 7.23;
                            GPair.RangeEnd = 12.77;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 12.78;
                            GPair.RangeEnd = 18.33;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 18.34;
                            GPair.RangeEnd = 23.88;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 23.89;
                            GPair.RangeEnd = 29.44;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 29.45;
                            GPair.RangeEnd = 35;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 35.0000001;
                            GPair.RangeEnd = 40;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 40.0000001;
                            GPair.RangeEnd = 75;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        }

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.set_Status_Support(hs, true);
                        hs.SaveEventsDevices();
                    }
                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Temp");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Temp");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Temperature;
                        DT.Device_SubType = 1;
                        // temp
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        Pair.PairType = VSVGPairs.VSVGPairType.Range;
                        Pair.RangeStart = -2147483648L;
                        Pair.RangeEnd = 2147483647;
                        Pair.RangeStatusPrefix = "";
                        Pair.RangeStatusSuffix = " " + VSVGPairs.VSPair.ScaleReplace;
                        Pair.IncludeValues = true;
                        Pair.ValueOffset = 0;
                        Pair.HasScale = true;
                        Pair.RangeStatusDecimals = 0;
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        if (gGlobalTempScaleF)
                        {
                            // Set up the ranges for Fahrenheit
                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = -50;
                            GPair.RangeEnd = 5;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 5.00000001;
                            GPair.RangeEnd = 15.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 16;
                            GPair.RangeEnd = 25.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 26;
                            GPair.RangeEnd = 35.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;
                            GPair.RangeStart = 36;
                            GPair.RangeEnd = 45.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 46;
                            GPair.RangeEnd = 55.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 56;
                            GPair.RangeEnd = 65.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 66;
                            GPair.RangeEnd = 75.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 76;
                            GPair.RangeEnd = 85.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 86;
                            GPair.RangeEnd = 95.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 96;
                            GPair.RangeEnd = 104.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 105;
                            GPair.RangeEnd = 150.99999999;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                        }
                        else
                        {
                            // Celsius
                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = -45;
                            GPair.RangeEnd = -15;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = -14.999999;
                            GPair.RangeEnd = -9.44;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = -9.43999999;
                            GPair.RangeEnd = -3.88;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = -3.8799999;
                            GPair.RangeEnd = 1.66;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 1.67;
                            GPair.RangeEnd = 7.22;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 7.23;
                            GPair.RangeEnd = 12.77;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 12.78;
                            GPair.RangeEnd = 18.33;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 18.34;
                            GPair.RangeEnd = 23.88;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 23.89;
                            GPair.RangeEnd = 29.44;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 29.45;
                            GPair.RangeEnd = 35;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 35.0000001;
                            GPair.RangeEnd = 40;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            GPair = new VSVGPairs.VGPair();
                            GPair.PairType = VSVGPairs.VSVGPairType.Range;

                            GPair.RangeStart = 40.0000001;
                            GPair.RangeEnd = 75;
                            GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png";
                            Default_VG_Pairs_AddUpdateUtil(dvRef, GPair);

                            dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                            dv.set_Status_Support(hs, true);
                            hs.SaveEventsDevices();
                        }
                    }


                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Fan State");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Fan State");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Fan_Status;
                        DT.Device_SubType = 0;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

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

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.set_Status_Support(hs, true);
                        hs.SaveEventsDevices();
                    }

                    dvRef = hs.NewDeviceRef(IFACE_NAME + " Thermostat Mode Status");
                    if (dvRef > 0)
                    {
                        dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                        dv.set_Address(hs, "HOME");
                        dv.set_Device_Type_String(hs, "Thermostat Mode Status");
                        dv.set_Interface(hs, IFACE_NAME);
                        dv.set_InterfaceInstance(hs, "");
                        dv.set_Location(hs, IFACE_NAME);
                        dv.set_Location2(hs, "Sample Devices");

                        DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                        DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Thermostat;
                        DT.Device_Type = (int)DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat.Operating_State;
                        DT.Device_SubType = 0;
                        DT.Device_SubType_Description = "";
                        dv.set_DeviceType_Set(hs, DT);
                        dv.set_Relationship(hs, Enums.eRelationship.Child);
                        if (therm_root_dv != null)
                        {
                            therm_root_dv.AssociatedDevice_Add(hs, dvRef);
                        }
                        dv.AssociatedDevice_Add(hs, therm_root_dv.get_Ref(hs));

                        VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 0;
                        Pair.Status = "Idle";
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 1;
                        Pair.Status = "Heating";
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Status);
                        Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                        Pair.Value = 2;
                        Pair.Status = "Cooling";
                        Default_VS_Pairs_AddUpdateUtil(dvRef, Pair);

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                        dv.set_Status_Support(hs, true);
                        hs.SaveEventsDevices();
                    }
                }

            }
            catch (Exception ex)
            {
                hs.WriteLog(IFACE_NAME + " Error", "Exception in Find_Create_Devices/Create: " + ex.Message);
            }

        }

        private static void Default_VG_Pairs_AddUpdateUtil(int dvRef, VSVGPairs.VGPair Pair)
        {
            if (Pair == null)
                return;
            if (dvRef < 1)
                return;
            if (!hs.DeviceExistsRef(dvRef))
                return;

            VSVGPairs.VGPair Existing = null;

            // The purpose of this procedure is to add the protected, default VS/VG pairs WITHOUT overwriting any user added
            //   pairs unless absolutely necessary (because they conflict).

            try
            {
                Existing = hs.DeviceVGP_Get(dvRef, Pair.Value);
                //VGPairs.GetPairByValue(Pair.Value)

                if (Existing != null)
                {
                    hs.DeviceVGP_Clear(dvRef, Pair.Value);
                    hs.DeviceVGP_AddPair(dvRef, Pair);
                }
                else
                {
                    // There is not a pair existing, so just add it.
                    hs.DeviceVGP_AddPair(dvRef, Pair);
                }


            }
            catch (Exception)
            {
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
        private static void CreateOneDevice(string dev_name)
        {
            int dvRef = 0;
            Scheduler.Classes.DeviceClass dv = default(Scheduler.Classes.DeviceClass);

            dvRef = hs.NewDeviceRef(dev_name);
            Console.WriteLine("Creating device named: " + dev_name);
            if (dvRef > 0)
            {
                dv = (Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(dvRef);
                dv.set_Address(hs, "HOME");
                //dv.set_Can_Dim(hs, True
                dv.set_Device_Type_String(hs, "My Sample Device");
                DeviceTypeInfo_m.DeviceTypeInfo DT = new DeviceTypeInfo_m.DeviceTypeInfo();
                DT.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;
                DT.Device_Type = 69;
                dv.set_DeviceType_Set(hs, DT);
                dv.set_Interface(hs, IFACE_NAME);
                dv.set_InterfaceInstance(hs, "");
                dv.set_Last_Change(hs, new DateTime(1929, 5, 21, 11, 0, 0));
                dv.set_Location(hs, IFACE_NAME);
                dv.set_Location2(hs, "Sample Devices");


                // add an ON button and value
                VSVGPairs.VSPair Pair = default(VSVGPairs.VSPair);
                Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                Pair.Value = 100;
                Pair.Status = "On";
                Pair.Render = Enums.CAPIControlType.Button;
                hs.DeviceVSP_AddPair(dvRef, Pair);

                // add an OFF button and value
                Pair = new VSVGPairs.VSPair(HomeSeerAPI.ePairStatusControl.Both);
                Pair.PairType = VSVGPairs.VSVGPairType.SingleValue;
                Pair.Value = 0;
                Pair.Status = "Off";
                Pair.Render = Enums.CAPIControlType.Button;
                hs.DeviceVSP_AddPair(dvRef, Pair);

                // add DIM values
                Pair = new VSVGPairs.VSPair(ePairStatusControl.Both);
                Pair.PairType = VSVGPairs.VSVGPairType.Range;
                Pair.RangeStart = 1;
                Pair.RangeEnd = 99;
                Pair.RangeStatusPrefix = "Dim ";
                Pair.RangeStatusSuffix = "%";
                Pair.Render = Enums.CAPIControlType.ValuesRangeSlider;

                hs.DeviceVSP_AddPair(dvRef, Pair);

                //dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP)     ' cause control to be displayed in a pop-up dialog
                dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES);
                dv.MISC_Set(hs, Enums.dvMISC.NO_LOG);

                dv.set_Status_Support(hs, true);
            }
        }

        static internal strTrigger TriggerFromInfo(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {
            string sKey = "";
            sKey = "K" + TrigInfo.UID.ToString();
            if (colTrigs != null)
            {
                if (colTrigs.ContainsKey(sKey))
                {
                    object obj = null;
                    obj = colTrigs[sKey];
                    if (obj != null)
                    {
                        strTrigger Ret = default(strTrigger);
                        Ret.Result = false;
                        if (obj is Classes.MyTrigger1Ton)
                        {
                            Ret.WhichTrigger = eTriggerType.OneTon;
                            Ret.TrigObj = obj;
                            Ret.Result = true;
                            return Ret;
                        }
                        else if (obj is Classes.MyTrigger2Shoe)
                        {
                            Ret.WhichTrigger = eTriggerType.TwoVolts;
                            Ret.TrigObj = obj;
                            Ret.Result = true;
                            return Ret;
                        }
                    }
                }
            }
            strTrigger Bad = default(strTrigger);
            Bad.WhichTrigger = eTriggerType.Unknown;
            Bad.Result = false;
            Bad.TrigObj = null;
            return Bad;
        }
        static internal strAction ActionFromInfo(HomeSeerAPI.IPlugInAPI.strTrigActInfo ActInfo)
        {
            string sKey = "";
            sKey = "K" + ActInfo.UID.ToString();
            if (colActs != null)
            {
                if (colActs.ContainsKey(sKey))
                {
                    object obj = null;
                    obj = colActs[sKey];
                    if (obj != null)
                    {
                        strAction Ret = default(strAction);
                        Ret.Result = false;
                        if (obj is Classes.MyAction1EvenTon)
                        {
                            Ret.WhichAction = eActionType.Weight;
                            Ret.ActObj = obj;
                            Ret.Result = true;
                            return Ret;
                        }
                        else if (obj is Classes.MyAction2Euro)
                        {
                            Ret.WhichAction = eActionType.Voltage;
                            Ret.ActObj = obj;
                            Ret.Result = true;
                            return Ret;
                        }
                    }
                }
            }
            strAction Bad = default(strAction);
            Bad.WhichAction = eActionType.Unknown;
            Bad.Result = false;
            Bad.ActObj = null;
            return Bad;
        }


        static internal bool SerializeObject(object ObjIn, ref byte[] bteOut)
        {
            if (ObjIn == null)
                return false;
            MemoryStream str = new MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            try
            {
                sf.Serialize(str, ObjIn);
                bteOut = new byte[Convert.ToInt32(str.Length - 1) + 1];
                bteOut = str.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Log(IFACE_NAME + " Error: Serializing object " + ObjIn.ToString() + " :" + ex.Message, LogType.LOG_TYPE_ERROR);
                return false;
            }

        }

        static internal bool SerializeObject(object ObjIn, ref string HexOut)
        {
            if (ObjIn == null)
                return false;
            MemoryStream str = new MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            byte[] bteOut = null;

            try
            {
                sf.Serialize(str, ObjIn);
                bteOut = new byte[Convert.ToInt32(str.Length - 1) + 1];
                bteOut = str.ToArray();
                HexOut = "";
                for (int i = 0; i <= bteOut.Length - 1; i++)
                {
                    HexOut += bteOut[i].ToString("x2").ToUpper();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log(IFACE_NAME + " Error: Serializing (Hex) object " + ObjIn.ToString() + " :" + ex.Message, LogType.LOG_TYPE_ERROR);
                return false;
            }

        }

        public static bool DeSerializeObject(byte[] bteIn, ref object ObjOut, System.Type OType)
        {
            // Almost immediately there is a test to see if ObjOut is NOTHING.  The reason for this
            //   when the ObjOut is suppose to be where the deserialized object is stored, is that 
            //   I could find no way to test to see if the deserialized object and the variable to 
            //   hold it was of the same type.  If you try to get the type of a null object, you get
            //   only a null reference exception!  If I do not test the object type beforehand and 
            //   there is a difference, then the InvalidCastException is thrown back in the CALLING
            //   procedure, not here, because the cast is made when the ByRef object is cast when this
            //   procedure returns, not earlier.  In order to prevent a cast exception in the calling
            //   procedure that may or may not be handled, I made it so that you have to at least 
            //   provide an initialized ObjOut when you call this - ObjOut is set to nothing after it 
            //   is typed.
            if (bteIn == null)
                return false;
            if (bteIn.Length < 1)
                return false;
            MemoryStream str = null;
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            object ObjTest = null;
            System.Type TType = null;
            try
            {
                ObjOut = null;
                str = new MemoryStream(bteIn);
                ObjTest = sf.Deserialize(str);
                if (ObjTest == null)
                    return false;
                TType = ObjTest.GetType();
                if (!TType.Equals(OType))
                    return false;
                ObjOut = ObjTest;
                if (ObjOut == null)
                    return false;
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Log(IFACE_NAME + " Error: DeSerializing object: " + ex.Message, LogType.LOG_TYPE_ERROR);
                return false;
            }

        }

        public static bool DeSerializeObject(string HexIn, ref object ObjOut, System.Type OType)
        {
            // Almost immediately there is a test to see if ObjOut is NOTHING.  The reason for this
            //   when the ObjOut is suppose to be where the deserialized object is stored, is that 
            //   I could find no way to test to see if the deserialized object and the variable to 
            //   hold it was of the same type.  If you try to get the type of a null object, you get
            //   only a null reference exception!  If I do not test the object type beforehand and 
            //   there is a difference, then the InvalidCastException is thrown back in the CALLING
            //   procedure, not here, because the cast is made when the ByRef object is cast when this
            //   procedure returns, not earlier.  In order to prevent a cast exception in the calling
            //   procedure that may or may not be handled, I made it so that you have to at least 
            //   provide an initialized ObjOut when you call this - ObjOut is set to nothing after it 
            //   is typed.
            if (HexIn == null)
                return false;
            if (string.IsNullOrEmpty(HexIn.Trim()))
                return false;
            MemoryStream str = null;
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            object ObjTest = null;
            System.Type TType = null;

            byte[] bteIn = null;
            int HowMany = 0;

            try
            {
                HowMany = Convert.ToInt32((HexIn.Length / 2) - 1);
                bteIn = new byte[HowMany + 1];
                for (int i = 0; i <= HowMany; i++)
                {
                    //bteIn(i) = CByte("&H" & HexIn.Substring(i * 2, 2))
                    bteIn[i] = byte.Parse(HexIn.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                }
                ObjOut = null;
                str = new MemoryStream(bteIn);
                ObjTest = sf.Deserialize(str);
                if (ObjTest == null)
                    return false;
                TType = ObjTest.GetType();
                if (!TType.Equals(OType))
                    return false;
                ObjOut = ObjTest;
                if (ObjOut == null)
                    return false;
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Log(IFACE_NAME + " Error: DeSerializing object: " + ex.Message, LogType.LOG_TYPE_ERROR);
                return false;
            }

        }
    }

}
