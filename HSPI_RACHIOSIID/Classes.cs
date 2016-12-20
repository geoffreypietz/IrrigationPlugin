using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace HSPI_RACHIOSIID
{

    static class Classes
    {

        // ==========================================================================
        // ==========================================================================
        // ==========================================================================
        //       These class objects are used to hold plug-in specific information 
        //   about its various triggers and actions.  If there is no information 
        //   needed other than the Trigger/Action number and/or the SubTrigger
        //   /SubAction number, then these are not needed as they are intended to 
        //   store additional information beyond those selection values.  The UID
        //   (Unique Trigger ID or Unique Action ID) can be used as the key to the
        //   storage of these class objects when the plug-in is running.  When the 
        //   plug-in is not running, the serialized copy of these classes is stored
        //   and restored by HomeSeer.
        // ==========================================================================
        // ==========================================================================
        // ==========================================================================


        [Serializable()]
        internal class MyAction1EvenTon
        {

            private int mvarUID;
            public int ActionUID
            {
                get { return mvarUID; }
                set { mvarUID = value; }
            }

            private bool mvarConfigured;
            public bool Configured
            {
                get { return mvarConfigured; }
            }

            [Serializable()]
            internal enum eSetTo
            {
                Not_Set = 0,
                Rounded = 1,
                Unrounded = 2
            }

            private eSetTo mvarSet;
            public eSetTo SetTo
            {
                get
                {
                    if (!mvarConfigured)
                        return eSetTo.Not_Set;
                    return mvarSet;
                }
                set
                {
                    mvarConfigured = true;
                    mvarSet = value;
                }
            }
        }

        [Serializable()]
        internal class MyAction2Euro
        {

            private int mvarUID;
            public int ActionUID
            {
                get { return mvarUID; }
                set { mvarUID = value; }
            }

            private bool mvarConfigured;
            public bool Configured
            {
                get { return mvarConfigured; }
            }

            [Serializable()]
            internal enum eVAction
            {
                Not_Set = 0,
                SetEuro = 1,
                SetNorthAmerica = 2,
                ResetAverage = 3
            }

            private eVAction mvarSet;
            public eVAction ThisAction
            {
                get
                {
                    if (!mvarConfigured)
                        return eVAction.Not_Set;
                    return mvarSet;
                }
                set
                {
                    mvarConfigured = true;
                    mvarSet = value;
                }
            }
        }

        [Serializable()]
        internal class MyTrigger1Ton
        {

            private int mvarUID;
            public int TriggerUID
            {
                get { return mvarUID; }
                set { mvarUID = value; }
            }

            private double mvarTriggerWeight;
            public double TriggerWeight
            {
                get { return mvarTriggerWeight; }
                set { mvarTriggerWeight = value; }
            }

            private bool mvarCondition;
            public bool Condition
            {
                get { return mvarCondition; }
                set { mvarCondition = value; }
            }

            //Private mvarWeight As Double
            //Public Property Weight As Double
            //    Get
            //        Return mvarWeight
            //    End Get
            //    Set(value As Double)
            //        mvarWeight = value
            //    End Set
            //End Property

            private bool mvarEvenTon;
            public bool EvenTon
            {
                get { return mvarEvenTon; }
                set { mvarEvenTon = value; }
            }


        }

        [Serializable()]
        internal class MyTrigger2Shoe
        {


            private bool mvarSubTrig2 = false;
            private int mvarUID;
            public int TriggerUID
            {
                get { return mvarUID; }
                set { mvarUID = value; }
            }

            public bool SubTrigger2
            {
                get { return mvarSubTrig2; }
                set { mvarSubTrig2 = value; }
            }

            private bool mvarCondition;
            public bool Condition
            {
                get { return mvarCondition; }
                set { mvarCondition = value; }
            }
            private double mvarTriggerValue;
            public double TriggerValue
            {
                get { return mvarTriggerValue; }
                set { mvarTriggerValue = value; }
            }

            private bool mvarVoltTypeEuro;
            public bool EuroVoltage
            {
                get { return mvarVoltTypeEuro; }
                set { mvarVoltTypeEuro = value; }
            }
            public bool NorthAMVoltage
            {
                get { return !mvarVoltTypeEuro; }
                set { mvarVoltTypeEuro = !value; }
            }
        }
    }
}
