﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

using UnityEngine;
using KSP;

namespace KerbalAlarmClock
{
    public partial class KACWorker
    {
        public void WindowLayout_AddTypeANDN()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Node Type:", KACResources.styleAddHeading);

            if (KACWorkerGameState.CurrentVessel.Landed && KACWorkerGameState.CurrentVesselTarget!=null &&KACWorkerGameState.CurrentVesselTarget is Vessel)
            {
                int intTemp=0;
                DrawRadioList(ref intTemp, "Launch Rendezvous");
                if (AddType!= KACAlarm.AlarmType.LaunchRendevous)
                {
                    AddType = KACAlarm.AlarmType.LaunchRendevous;
                    AddTypeChanged();
                }
            }
            else
            {
                int intOption = 0;
                if (AddType == KACAlarm.AlarmType.LaunchRendevous)
                {
                    AddType = KACAlarm.AlarmType.AscendingNode;
                    AddTypeChanged();
                }
                if (AddType != KACAlarm.AlarmType.AscendingNode) intOption = 1;
                if (DrawRadioList(ref intOption, "Ascending", "Descending"))
                {
                    if (intOption == 0)
                        AddType = KACAlarm.AlarmType.AscendingNode;
                    else
                        AddType = KACAlarm.AlarmType.DescendingNode;
                    AddTypeChanged();
                }
            }
            GUILayout.EndHorizontal();

            //if (KACWorkerGameState.CurrentVesselTarget is Vessel || KACWorkerGameState.CurrentVesselTarget is CelestialBody)
            //{
            //    GUILayout.BeginHorizontal();
            //    GUILayout.Label("Target:",KACResources.styleAddHeading, GUILayout.Width(100));
            //    GUILayout.Label(string.Format("{0} ({1})", KACWorkerGameState.CurrentVesselTarget.GetName(), KACWorkerGameState.CurrentVesselTarget.GetType()),KACResources.styleContent);
            //    GUILayout.EndHorizontal();
            //}
        }

        private void WindowLayout_AddPane_AscendingNodeEquatorial()
        {
            Double dblNodeTime = 0;
            Boolean blnNodeFound = KACWorkerGameState.CurrentVessel.orbit.AscendingNodeEquatorialExists();
            if (blnNodeFound)
            {
                try { dblNodeTime = KACWorkerGameState.CurrentVessel.orbit.TimeOfAscendingNodeEquatorial(KACWorkerGameState.CurrentTime.UT); }
                catch { blnNodeFound = false; }
            }
            WindowLayout_AddPane_NodeEvent(blnNodeFound, dblNodeTime - KACWorkerGameState.CurrentTime.UT);
        }

        private void WindowLayout_AddPane_DescendingNodeEquatorial()
        {
            Double dblNodeTime = 0;
            Boolean blnNodeFound = KACWorkerGameState.CurrentVessel.orbit.DescendingNodeEquatorialExists();
            if (blnNodeFound)
            {
                try { dblNodeTime = KACWorkerGameState.CurrentVessel.orbit.TimeOfDescendingNodeEquatorial(KACWorkerGameState.CurrentTime.UT); }
                catch { blnNodeFound = false; }
            }
            WindowLayout_AddPane_NodeEvent(blnNodeFound, dblNodeTime - KACWorkerGameState.CurrentTime.UT);
        }

        private void WindowLayout_AddPane_AscendingNode()
        {
            Double dblNodeTime = 0;
            Boolean blnNodeFound = KACWorkerGameState.CurrentVessel.orbit.AscendingNodeExists(KACWorkerGameState.CurrentVesselTarget.GetOrbit());
            if (blnNodeFound)
            {
                try { dblNodeTime = KACWorkerGameState.CurrentVessel.orbit.TimeOfAscendingNode(KACWorkerGameState.CurrentVesselTarget.GetOrbit(), KACWorkerGameState.CurrentTime.UT); }
                catch { blnNodeFound = false; }
            }
            WindowLayout_AddPane_NodeEvent(blnNodeFound, dblNodeTime - KACWorkerGameState.CurrentTime.UT);
        }
        private void WindowLayout_AddPane_DescendingNode()
        {
            Double dblNodeTime = 0;
            Boolean blnNodeFound = KACWorkerGameState.CurrentVessel.orbit.DescendingNodeExists(KACWorkerGameState.CurrentVesselTarget.GetOrbit());
            if (blnNodeFound)
            {
                try { dblNodeTime = KACWorkerGameState.CurrentVessel.orbit.TimeOfDescendingNode(KACWorkerGameState.CurrentVesselTarget.GetOrbit(), KACWorkerGameState.CurrentTime.UT); }
                catch { blnNodeFound = false; }
            }
            WindowLayout_AddPane_NodeEvent(blnNodeFound, dblNodeTime - KACWorkerGameState.CurrentTime.UT);
        }

        private void WindowLayout_AddPane_LaunchRendevous()
        {
            Double dblNodeTime = 0;
            Boolean blnNodeFound = true;
            dblNodeTime = KACWorkerGameState.CurrentTime.UT + LaunchTiming.TimeToPlane(KACWorkerGameState.CurrentVessel.mainBody,
                                                                                        KACWorkerGameState.CurrentVessel.latitude,
                                                                                        KACWorkerGameState.CurrentVessel.longitude,
                                                                                        KACWorkerGameState.CurrentVesselTarget.GetOrbit());
            WindowLayout_AddPane_NodeEvent(blnNodeFound, dblNodeTime - KACWorkerGameState.CurrentTime.UT);
        }
    }
}
