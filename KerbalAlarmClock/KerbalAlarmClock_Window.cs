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


        #region "OnGUI Stuff"
        System.Random rnd = new System.Random();
        public void SetupDrawStuff()
        {
            GUI.skin = HighLogic.Skin;
            if (KACResources.styleWindow == null)
            {
                KACResources.SetStyles();
                //styleWindow = new GUIStyle(GUI.skin.window);
            }
        }

        #region "Tooltip Work"
        //Tooltip variables
        //Store the tooltip text from throughout the code
        String strToolTipText = "";
        String strLastTooltipText = "";
        //is it displayed and where
        Boolean blnToolTipDisplayed = false;
        Rect rectToolTipPosition;
        int intTooltipVertOffset = 12;
        int intTooltipMaxWidth = 250;
        //timer so it only displays for a preriod of time
        float fltTooltipTime = 0f;
        float fltMaxToolTipTime = 15f;


        private void DrawToolTip()
        {
            if (strToolTipText != "" && (fltTooltipTime < fltMaxToolTipTime))
            {
                GUIContent contTooltip = new GUIContent(strToolTipText);
                if (!blnToolTipDisplayed || (strToolTipText != strLastTooltipText))
                {
                    //reset display time if text changed
                    fltTooltipTime = 0f;
                    //Calc the size of the Tooltip
                    rectToolTipPosition = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y + intTooltipVertOffset, 0, 0);
                    float minwidth, maxwidth;
                    KACResources.styleTooltipStyle.CalcMinMaxWidth(contTooltip, out minwidth, out maxwidth); // figure out how wide one line would be
                    rectToolTipPosition.width = Math.Min(intTooltipMaxWidth - KACResources.styleTooltipStyle.padding.horizontal, maxwidth); //then work out the height with a max width
                    rectToolTipPosition.height = KACResources.styleTooltipStyle.CalcHeight(contTooltip, rectToolTipPosition.width); // heers the result
                    //Make sure its not off the right of the screen
                    if (rectToolTipPosition.x + rectToolTipPosition.width > Screen.width) rectToolTipPosition.x = Screen.width - rectToolTipPosition.width;
                }
                //Draw the Tooltip
                GUI.Label(rectToolTipPosition, contTooltip, KACResources.styleTooltipStyle);
                //On top of everything
                GUI.depth = 0;

                //update how long the tip has been on the screen and reset the flags
                fltTooltipTime += Time.deltaTime;
                blnToolTipDisplayed = true;
            }
            else
            {
                //clear the flags
                blnToolTipDisplayed = false;
            }
            if (strToolTipText != strLastTooltipText) fltTooltipTime = 0f;
            strLastTooltipText = strToolTipText;
        }

        public void SetTooltipText()
        {
            if (Event.current.type == EventType.Repaint)
            {
                strToolTipText = GUI.tooltip;
            }
        }
        #endregion

        #region "Stuff to get icon and windows by scene"
        public Boolean IconShowByActiveScene
        {
            get
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: return Settings.IconShow_SpaceCenter;
                    case GameScenes.TRACKSTATION: return Settings.IconShow_TrackingStation;
                    default: return true;
                }
            }
            set
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: Settings.IconShow_SpaceCenter = value; break;
                    case GameScenes.TRACKSTATION: Settings.IconShow_TrackingStation = value; break;
                    default: 
                        //Settings.WindowVisible = value; 
                        break;
                }
            }
        }

        public Rect IconPosByActiveScene
        {
            get
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: return Settings.IconPos_SpaceCenter;
                    case GameScenes.TRACKSTATION: return Settings.IconPos_TrackingStation;
                    default: return Settings.IconPos;
                }
            }
            set
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: Settings.IconPos_SpaceCenter = value; break;
                    case GameScenes.TRACKSTATION: Settings.IconPos_TrackingStation = value; break;
                    default: Settings.IconPos = value; break;
                }
            }
        }


        public Boolean WindowVisibleByActiveScene
        {
            get
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: return Settings.WindowVisible_SpaceCenter;
                    case GameScenes.TRACKSTATION: return Settings.WindowVisible_TrackingStation;
                    default: return Settings.WindowVisible;
                }
            }
            set
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: Settings.WindowVisible_SpaceCenter = value; break;
                    case GameScenes.TRACKSTATION: Settings.WindowVisible_TrackingStation = value; break;
                    default: Settings.WindowVisible = value; break;
                }
            }
        }

        public Boolean WindowMinimizedByActiveScene
        {
            get
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: return Settings.WindowMinimized_SpaceCenter;
                    case GameScenes.TRACKSTATION: return Settings.WindowMinimized_TrackingStation;
                    default: return Settings.WindowMinimized;
                }
            }
            set
            {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER: Settings.WindowMinimized_SpaceCenter = value; break;
                    case GameScenes.TRACKSTATION: Settings.WindowMinimized_TrackingStation = value; break;
                    default: Settings.WindowMinimized = value; break;
                }
            }
        }

        public Rect WindowPosByActiveScene
        {
            get {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER:    return Settings.WindowPos_SpaceCenter;
                    case GameScenes.TRACKSTATION:   return Settings.WindowPos_TrackingStation;
                    default:                        return Settings.WindowPos;
                }
            }
            set {
                switch (KACWorkerGameState.CurrentGUIScene)
                {
                    case GameScenes.SPACECENTER:    Settings.WindowPos_SpaceCenter = value;         break;
                    case GameScenes.TRACKSTATION:   Settings.WindowPos_TrackingStation = value;     break;
                    default:                        Settings.WindowPos = value;                     break;
                }
            }
        }
        #endregion

        /// <summary>
        /// Draw the icon on the screen
        /// </summary>
        public void DrawIcons()
        {
            if (!Settings.UseBlizzyToolbarIfAvailable || btnToolbarKAC == null)
            {
            Texture2D iconToShow;
            //Replace this with workerstate object that can test for pause and catch errors - is it doing this in flight mode??
            if (!(KACWorkerGameState.CurrentGUIScene == GameScenes.FLIGHT) || (!KACWorkerGameState.PauseMenuOpen && !KACWorkerGameState.FlightResultsDialogOpen))
            {
                if (FlightDriver.Pause)
                {
                    iconToShow = KACResources.GetPauseIcon();
                }
                else if (KACWorkerGameState.CurrentlyUnderWarpInfluence)
                {
                    iconToShow = KACResources.GetWarpIcon();
                }
                else
                {
                    if (Settings.Alarms.ActiveEnabledFutureAlarms(HighLogic.CurrentGame.Title))
                    {
                        if (WindowVisibleByActiveScene)
                            iconToShow = KACResources.iconAlarmShow;
                        else
                            iconToShow = KACResources.iconAlarm;
                    }
                    else
                    {
                        if (WindowVisibleByActiveScene)
                            iconToShow = KACResources.iconNormShow;
                        else
                            iconToShow = KACResources.iconNorm;
                    }
                }

                //draw the icon button
                if (IconShowByActiveScene)
                {
                    if (GUI.Button(IconPosByActiveScene, new GUIContent(iconToShow, "Click to Toggle"), KACResources.styleIconStyle))
                    {
                        //switch (KACWorkerGameState.CurrentGUIScene)
                        //{
                        //    case GameScenes.SPACECENTER: Settings.WindowVisible_SpaceCenter = !Settings.WindowVisible_SpaceCenter; break;
                        //    case GameScenes.TRACKSTATION: Settings.WindowVisible_TrackingStation = !Settings.WindowVisible_TrackingStation;break;
                        //    default: Settings.WindowVisible = !Settings.WindowVisible;break;
                        //}
                        WindowVisibleByActiveScene = !WindowVisibleByActiveScene;
                        Settings.Save();
                    }
                    //GUI.Label(new Rect(152,32,200,20), GUI.tooltip,KACResources.styleTooltipStyle);
                }
            }
        }
            else
            {
                //Do for Blizzies Toolbar
                if (btnToolbarKAC != null) { 
                    String TexturePath = "";
                    if (!(KACWorkerGameState.CurrentGUIScene == GameScenes.FLIGHT) || (!KACWorkerGameState.PauseMenuOpen && !KACWorkerGameState.FlightResultsDialogOpen))
                    {
                        if (FlightDriver.Pause)
                        {
                            TexturePath = KACResources.GetPauseIconTexturePath();
                        }
                        else if (KACWorkerGameState.CurrentlyUnderWarpInfluence)
                        {
                            TexturePath = KACResources.GetWarpIconTexturePath();
                        }
                        else
                        {
                            if (Settings.Alarms.ActiveEnabledFutureAlarms(HighLogic.CurrentGame.Title))
                            {
                                if (WindowVisibleByActiveScene)
                                    TexturePath = "TriggerTech/ToolbarIcons/KACIcon-AlarmShow";
                                else
                                    TexturePath = "TriggerTech/ToolbarIcons/KACIcon-Alarm";
                            }
                            else
                            {
                                if (WindowVisibleByActiveScene)
                                    TexturePath = "TriggerTech/ToolbarIcons/KACIcon-NormShow";
                                else
                                    TexturePath = "TriggerTech/ToolbarIcons/KACIcon-Norm";
                            }
                        }

                        btnToolbarKAC.TexturePath = TexturePath;
                    }
                }
            }
        }

//        internal Camera EZGUICamera;

//        internal UIButton UIBlockerMain;
//        internal UIButton UIBlockerSettings;
//        internal UIButton UIBlockerAdd;
//        internal UIButton UIBlockerAddMessages;
//        internal UIButton UIBlockerEdit;
//        internal UIButton UIBlockerEarthAlarm;

//        public void InitBlockers()
//        {
//            //Get the EZGUI Cam
//            DebugLogFormatted("Get Cam");
//            EZGUICamera = Camera.allCameras.ToList().Find(c => c.name == "EZGUI Cam");

//            Settings.UIBlockersRunning = true;
//            UIBlockerMain = CreateUIBlocker("UIBlockerMain");
//            UIBlockerSettings = CreateUIBlocker("UIBlockerSettings");
//            UIBlockerAdd = CreateUIBlocker("UIBlockerAdd");
//            UIBlockerAddMessages = CreateUIBlocker("UIBlockerAddMessages");
//            UIBlockerEdit = CreateUIBlocker("UIBlockerEdit");
//            UIBlockerEarthAlarm = CreateUIBlocker("UIBlockerEarthAlarm");
//        }

//        private UIButton CreateUIBlocker(String Name)
//        {
//            UIButton retUIBlocker=null;

//            try
//            {
//                //Create the UI Blockers
//                DebugLogFormatted("Create Blocker: {0}",Name);
//                retUIBlocker = UIButton.Create(Name, Vector3.zero);
//                retUIBlocker.renderCamera = EZGUICamera;
//                retUIBlocker.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");
//                retUIBlocker.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
//                //just a default size to start with
//                retUIBlocker.Setup(100, 100, new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = KACResources.txtUIBlocker });

//#if DEBUG
//                retUIBlocker.AddValueChangedDelegate(delegate(IUIObject obj) { ScreenMessages.PostScreenMessage(Name + " Blocker Clicked", 2f); });
//#endif
//            }
//            catch (Exception ex)
//            {
//                DebugLogFormatted("ERROR: Unable to set up UIBlocker-Disabling Blocking Functionality-{0}",ex.Message);
//                Settings.UIBlockersRunning = false;                
//            } return retUIBlocker;
//        }

//        public void PositionBlockers()
//        {
//            PositionBlocker(UIBlockerMain, WindowPosByActiveScene, WindowVisibleByActiveScene);
//            PositionBlocker(UIBlockerSettings, _WindowSettingsRect, WindowVisibleByActiveScene && _ShowSettings);
//            PositionBlocker(UIBlockerAdd, _WindowAddRect, WindowVisibleByActiveScene && _ShowAddPane);
//            PositionBlocker(UIBlockerAddMessages, _WindowAddMessagesRect, WindowVisibleByActiveScene && _ShowAddMessages);
//            PositionBlocker(UIBlockerEdit, _WindowEditRect, WindowVisibleByActiveScene && _ShowEditPane);
//            PositionBlocker(UIBlockerEarthAlarm, _WindowEarthAlarmRect, WindowVisibleByActiveScene && _ShowEarthAlarm);
//        }

        //private void PositionBlocker(UIButton uiBlocker,Rect WindowToBlock,Boolean Visible)
        //{
        //    if (uiBlocker != null)
        //    {
        //        //get the top left corner of the window
        //        Vector3 uiBlockerPos = EZGUICamera.ScreenToWorldPoint(new Vector3(WindowToBlock.x, Screen.height - WindowToBlock.y, 0));
        //        if (Visible && Settings.UIBlockersEnabled)
        //            uiBlockerPos.z = 0;
        //        else
        //            uiBlockerPos.z = -100;
        //        uiBlocker.transform.position = uiBlockerPos;

        //        if (uiBlocker.width != WindowToBlock.width ||
        //            uiBlocker.height != WindowToBlock.height)
        //        {
        //            DebugLogFormatted("Resizing UIBlocker:{0}",uiBlocker.name);
        //            uiBlocker.Setup(WindowToBlock.width, WindowToBlock.height);
        //        }
        //    }
        //}


        internal void KSCInputLocking()
        {
            Boolean MouseOverAnyWindow = false;

            //Check if the mouse is over any KAC window
            MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(WindowPosByActiveScene, WindowVisibleByActiveScene);
            MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(_WindowAddRect, WindowVisibleByActiveScene && _ShowAddPane);
            MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(_WindowSettingsRect, WindowVisibleByActiveScene && _ShowSettings);
            MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(_WindowAddMessagesRect, WindowVisibleByActiveScene && _ShowAddMessages);
            MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(_WindowEditRect, WindowVisibleByActiveScene && _ShowEditPane);
            MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(_WindowEarthAlarmRect, WindowVisibleByActiveScene && _ShowEarthAlarm);
            foreach (KACAlarm tmpAlarm in Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title))
            {
                if (tmpAlarm.AlarmWindowID != 0 && tmpAlarm.Actioned)
                {
                    MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(tmpAlarm.AlarmWindow, !tmpAlarm.AlarmWindowClosed);
                }
            }

            //If the setting is on and the mouse is over any window then lock it
            if (Settings.KSCUIBlockersEnabled && MouseOverAnyWindow)
            {
                if (!(InputLockManager.GetControlLock("KACLockOfKSCFacilities") == ControlTypes.KSC_FACILITIES))
                {
#if DEBUG
                    DebugLogFormatted("AddingLock-{0}", "KACLockOfKSCFacilities");
#endif
                    InputLockManager.SetControlLock(ControlTypes.KSC_FACILITIES, "KACLockOfKSCFacilities");
                }
            }
                //Otherwise make sure the lock is removed
            else
            {
                if (InputLockManager.GetControlLock("KACLockOfKSCFacilities") == ControlTypes.KSC_FACILITIES)
                {
#if DEBUG 
                    DebugLogFormatted("Removing-{0}", "KACLockOfKSCFacilities"); 
#endif
                    InputLockManager.RemoveControlLock("KACLockOfKSCFacilities");
                }
            }
            
            //LockKSCIfMouseOver("windowMainLock", WindowPosByActiveScene, WindowVisibleByActiveScene);
            //LockKSCIfMouseOver("windowAddLock", _WindowAddRect, WindowVisibleByActiveScene && _ShowAddPane);
            //LockKSCIfMouseOver("windowSettingsLock", _WindowSettingsRect, WindowVisibleByActiveScene && _ShowSettings);
            //LockKSCIfMouseOver("windowAddMessagesLock", _WindowAddMessagesRect, WindowVisibleByActiveScene && _ShowAddMessages);
            //LockKSCIfMouseOver("windowEditLock", _WindowEditRect, WindowVisibleByActiveScene && _ShowEditPane);
            //LockKSCIfMouseOver("windowEarthLock", _WindowEarthAlarmRect, WindowVisibleByActiveScene && _ShowEarthAlarm);

            //foreach (KACAlarm tmpAlarm in Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title))
            //{
            //    if (tmpAlarm.AlarmWindowID != 0 && tmpAlarm.Actioned )
            //    {
            //        LockKSCIfMouseOver(tmpAlarm.AlarmWindowID + "Lock", tmpAlarm.AlarmWindow, !tmpAlarm.AlarmWindowClosed);
            //    }
            //}
        }

        private Boolean MouseOverWindow(Rect WindowRect, Boolean WindowVisible)
        {
            return WindowVisible && WindowRect.Contains(Event.current.mousePosition);
        }

//        private void LockKSCIfMouseOver(String LockName,Rect WindowRect,Boolean WindowVisible)
//        {
//            if (Settings.KSCUIBlockersEnabled && WindowVisible && WindowRect.Contains(Event.current.mousePosition))
//            {
//                if (!(InputLockManager.GetControlLock(LockName) == ControlTypes.KSC_FACILITIES)) {
//#if DEBUG 
//                    DebugLogFormatted("AddingLock-{0}", LockName); 
//#endif
//                    InputLockManager.SetControlLock(ControlTypes.KSC_FACILITIES, LockName);
//                }
//            }
//            else
//            {
//                if (InputLockManager.GetControlLock(LockName) == ControlTypes.KSC_FACILITIES) {
//#if DEBUG 
//                    DebugLogFormatted("Removing-{0}", LockName); 
//#endif
//                    InputLockManager.RemoveControlLock(LockName);
//                }
//            }
//        }

        //Basic setup of draw stuff
        private static int _WindowMainID = 0;

        //is the add pane visible
        private Boolean _ShowAddPane = false;
        private static int _WindowAddID = 0;
        static Rect _WindowAddRect;

        //is the add pane messages pane visible
        private Boolean _ShowAddMessages = false;
        private static int _WindowAddMessagesID = 0;
        static Rect _WindowAddMessagesRect;

        //Settings Window
        private Boolean _ShowSettings = false;
        private static int _WindowSettingsID = 0;
        private static Rect _WindowSettingsRect;

        //Edit Window
        private Boolean _ShowEditPane = false;
        private static int _WindowEditID = 0;
        private static Rect _WindowEditRect;

        //Earth Alarm Window
        private Boolean _ShowEarthAlarm = false;
        private static int _WindowEarthAlarmID = 0;
        private static Rect _WindowEarthAlarmRect;

        //Earth Alarm Window
        private Boolean _ShowBackupFailedMessage = false;
        private DateTime _ShowBackupFailedMessageAt = DateTime.Now;
        private int _ShowBackupFailedMessageForSecs=10;
        private static int _WindowBackupFailedID = 0;
        private static Rect _WindowBackupFailedRect;

        //Window Size Constants
        int intMainWindowWidth = 300;
        int intMainWindowMinHeight = 114;
        int intMainWindowBaseHeight = 114;

        int intMainWindowAlarmListItemHeight = 26;
        int intMainWindowAlarmListScrollPad = 3;
        int intMainWindowEarthTimeHeight = 26;

        int intPaneWindowWidth = 380;
        int intAddPaneWindowWidth = 320;
        long AddWindowHeight;

        long EarthWindowHeight = 216;

        public void DrawWindows()
        {
#if DEBUG
            if (_ShowDebugPane)
            {
                _WindowDebugRect = GUILayout.Window(_WindowDebugID, _WindowDebugRect, FillDebugWindow, "Debug");
            }
#endif
            //set initial values for rect from old ones - ignore old width
            Rect MainWindowPos = new Rect(WindowPosByActiveScene.x, WindowPosByActiveScene.y, intMainWindowWidth, WindowPosByActiveScene.height);
            
            //Min or normal window
            if (WindowMinimizedByActiveScene)
            {
                MainWindowPos.height = intMainWindowMinHeight + 2;
            }
            else
            {
                MainWindowPos.height = intMainWindowBaseHeight;
                //Work out the number of alarms and therefore the height of the window
                if (Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title).Count > 1)
                {
                    if (Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title).Count<2)
                        MainWindowPos.height = intMainWindowBaseHeight;
                    else if (Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title).Count <= Settings.AlarmListMaxAlarmsInt)
                        MainWindowPos.height = intMainWindowBaseHeight + 
                            ((Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title).Count - 1) * intMainWindowAlarmListItemHeight) +
                            Settings.Alarms.Sum(x=>x.AlarmLineHeightExtra);
                    else
                        //this is scrolling
                        MainWindowPos.height = (intMainWindowBaseHeight -3) + 
                            ((Settings.AlarmListMaxAlarmsInt - 1) * intMainWindowAlarmListItemHeight) +
                            Settings.Alarms.Take(Settings.AlarmListMaxAlarmsInt).Sum(x => x.AlarmLineHeightExtra) +
                            intMainWindowAlarmListScrollPad;
                }
                else MainWindowPos.height = intMainWindowBaseHeight+2;
            }
            if (Settings.ShowEarthTime)
            {
                MainWindowPos.height += intMainWindowEarthTimeHeight;
            }

            //Now show the window
            WindowPosByActiveScene = GUILayout.Window(_WindowMainID, MainWindowPos, FillWindow, "Kerbal Alarm Clock - " + Settings.Version,KACResources.styleWindow);

            //Do we have anything to show in the right pane
            if (_ShowSettings)
            {
                _WindowSettingsRect = GUILayout.Window(_WindowSettingsID, new Rect(WindowPosByActiveScene.x + WindowPosByActiveScene.width, WindowPosByActiveScene.y, intPaneWindowWidth, intSettingsHeight), FillSettingsWindow, "Settings and Globals", KACResources.styleWindow);
            }
            else if (_ShowAddPane)
            {
                switch (AddType)
                {
                    case KACAlarm.AlarmType.Raw: 
                        AddWindowHeight = 250; break;
                    case KACAlarm.AlarmType.Maneuver:
                    case KACAlarm.AlarmType.SOIChange:
                        AddWindowHeight = 182; break;
                    case KACAlarm.AlarmType.Apoapsis:
                    case KACAlarm.AlarmType.Periapsis:
                        AddWindowHeight = 208; break;
                    case KACAlarm.AlarmType.AscendingNode:
                    case KACAlarm.AlarmType.DescendingNode:
                    case KACAlarm.AlarmType.LaunchRendevous:
                        AddWindowHeight = 234; break;
                    case KACAlarm.AlarmType.Transfer:                        
                    case KACAlarm.AlarmType.TransferModelled:
                        AddWindowHeight = intAddXferHeight; break;
                    case KACAlarm.AlarmType.Closest:
                        AddWindowHeight = 230; break;
                    case KACAlarm.AlarmType.Distance:
                        AddWindowHeight = intAddDistanceHeight; break;
                    case KACAlarm.AlarmType.Crew:
                        AddWindowHeight = intAddCrewHeight; break;
                    default: AddWindowHeight = 250; break;
                }
                AddWindowHeight += intHeight_AddWindowCommon;
                _WindowAddRect = GUILayout.Window(_WindowAddID, new Rect(WindowPosByActiveScene.x + WindowPosByActiveScene.width, WindowPosByActiveScene.y, intAddPaneWindowWidth, AddWindowHeight), FillAddWindow, "Add New Alarm", KACResources.styleWindow);                //switch (AddInterfaceType)

                if (_ShowAddMessages)
                {
                    _WindowAddMessagesRect = GUILayout.Window(_WindowAddMessagesID, new Rect(_WindowAddRect.x + _WindowAddRect.width, _WindowAddRect.y, 200, AddWindowHeight), FillAddMessagesWindow, "");
                }
            }
            else if (_ShowEarthAlarm)
            {
                float _WindowEarthTop = WindowPosByActiveScene.y + WindowPosByActiveScene.height - EarthWindowHeight;
                if (EarthWindowHeight > MainWindowPos.height) _WindowEarthTop = WindowPosByActiveScene.y;
                _WindowEarthAlarmRect = GUILayout.Window(_WindowEarthAlarmID, new Rect(WindowPosByActiveScene.x + WindowPosByActiveScene.width, _WindowEarthTop, intAddPaneWindowWidth, EarthWindowHeight), FillEarthAlarmWindow, "Add Earth Time Alarm", KACResources.styleWindow);                //switch (AddInterfaceType)
                if (_ShowAddMessages)
                {
                    _WindowAddMessagesRect = GUILayout.Window(_WindowAddMessagesID, new Rect(_WindowEarthAlarmRect.x + _WindowEarthAlarmRect.width, _WindowEarthAlarmRect.y, 200, EarthWindowHeight), FillAddMessagesWindow, "");
                }
            }
            else if (_ShowEditPane)
            {
                _WindowEditRect = GUILayout.Window(_WindowEditID, new Rect(WindowPosByActiveScene.x + WindowPosByActiveScene.width, WindowPosByActiveScene.y, intPaneWindowWidth, intAlarmEditHeight), FillEditWindow, "Editing Alarm", KACResources.styleWindow);
            }

            if (_ShowBackupFailedMessage)
            {
                _WindowBackupFailedRect = GUILayout.Window(_WindowBackupFailedID, _WindowBackupFailedRect, FillBackupFailedWindow, "Save Backup Failed", KACResources.styleWindow);
                if (DateTime.Now.Subtract(_ShowBackupFailedMessageAt).Seconds > _ShowBackupFailedMessageForSecs)
                    ResetBackupFailedWindow();
            }

            DrawToolTip();
        }


        //Now the layout
        public void FillWindow(int intWindowID)
        {
            GUILayout.BeginVertical();

            //Heading Part
            GUILayout.BeginHorizontal();
            GUILayout.Label("Alarm List", KACResources.styleHeading, GUILayout.ExpandWidth(true));

            //No longer relevant
            //hide this stuff when not in alarm edit mode/flight mode
            //if (!parentBehaviour.ViewAlarmsOnly)
            //{
                if (Settings.AlarmNodeRecalc)
                {
                    GUIContent XferIcon = new GUIContent(KACResources.iconAp, "Orbit Node (Ap,Pe,AN,DN) Recalculation is enabled");
                    GUILayout.Label(XferIcon, KACResources.styleFlagIcon);
                }
                if (Settings.AlarmXferRecalc)
                {
                    GUIContent XferIcon = new GUIContent(KACResources.iconXFer, "Transfer Recalculation is enabled");
                    GUILayout.Label(XferIcon, KACResources.styleFlagIcon);
                }

                //SOI AutoAlarm stuff
                if (Settings.AlarmAddSOIAuto || Settings.AlarmSOIRecalc)
                {
                    String SOITooltip = "";
                    if (Settings.AlarmSOIRecalc)
                        SOITooltip = "SOI Recalculation is enabled";
                    if (Settings.AlarmAddSOIAuto)
                    {
                        if (SOITooltip != "") SOITooltip += "\r\n";
                        SOITooltip += "SOI Auto Add Enabled";
                        if (Settings.AlarmCatchSOIChange)
                        {
                            SOITooltip += "-plus catchall";
                        }
                        if (Settings.AlarmAddSOIAuto_ExcludeEVA)
                        {
                            SOITooltip += "-excluding EVA";
                        }
                        if (Settings.AlarmOnSOIChange_Action > 1) SOITooltip += " (Pause Action)";
                        else if (Settings.AlarmOnSOIChange_Action > 0) SOITooltip += " (Warp Kill Action)";
                    }
                    GUIContent SOIIcon = new GUIContent(KACResources.iconSOI, SOITooltip);
                    GUILayout.Label(SOIIcon, KACResources.styleSOIIndicator);
                }

                if (Settings.AlarmAddManAuto)
                {
                    String strTooltip = "Man Node Auto Add Enabled";
                    if (Settings.AlarmAddManAuto_andRemove)
                        strTooltip += " (and Removal)";

                    GUIContent ManIcon = new GUIContent(KACResources.iconMNode, strTooltip);
                    GUILayout.Label(ManIcon, KACResources.styleFlagIcon);
                }
            //}

            //Set a default for the MinMax button
            GUIContent contMaxMin = new GUIContent(KACResources.btnChevronUp, "Minimize");
            if (WindowMinimizedByActiveScene)
            {
                contMaxMin.image = KACResources.btnChevronDown;
                contMaxMin.tooltip = "Maximize";
            }
            //Draw the button
            if (GUILayout.Button(contMaxMin, KACResources.styleSmallButton))
            {
                WindowMinimizedByActiveScene = !WindowMinimizedByActiveScene;
                Settings.Save();
            }

            GUIContent contSettings = new GUIContent(KACResources.GetSettingsButtonIcon(Settings.VersionAttentionFlag), "Settings...");
            if (Settings.VersionAvailable) contSettings.tooltip = "Updated Version Available - Settings...";
            if (DrawToggle(ref _ShowSettings, contSettings, KACResources.styleSmallButton) && _ShowSettings)
            {
                NewSettingsWindow();
                _ShowAddPane = false;
                _ShowEditPane = false;
                _ShowEarthAlarm = false;
            }
            //No longer relevant
            //if (!parentBehaviour.ViewAlarmsOnly)
            //{

                if (DrawToggle(ref _ShowAddPane, new GUIContent(KACResources.btnAdd, "Add New Alarm..."), KACResources.styleSmallButton) && _ShowAddPane)
                {
                    //reset the add stuff
                    NewAddAlarm();
                    _ShowSettings = false;
                    _ShowEditPane = false;
                    _ShowEarthAlarm = false;
                }
            //}

            GUILayout.EndHorizontal();

            //Text Area for content portion
            GUILayout.BeginVertical(KACResources.styleAlarmListArea);
            if (WindowMinimizedByActiveScene)
            {
                WindowLayout_Minimized();
            }
            else
            {
                WindowLayout_AlarmList();
            }
            
            GUILayout.EndVertical();

            //Current Game time at the botttom of the control 
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Current Time:","Toggle Display of time from an alternate galaxy on a planet called \"Earth\""), KACResources.styleHeading))
            {
                Settings.ShowEarthTime = !Settings.ShowEarthTime;
            }

            //Work out the right text and tooltip and display the button as a label
            KACTime.PrintTimeFormat MainClockFormat = KACTime.PrintTimeFormat.DateTimeString;
            if (Settings.TimeFormat == KACTime.PrintTimeFormat.TimeAsUT) MainClockFormat = KACTime.PrintTimeFormat.TimeAsUT;
            GUIContent contCurrentTime = new GUIContent(KACTime.PrintDate(KACWorkerGameState.CurrentTime, MainClockFormat), "Click to toggle through time formats");
            if (GUILayout.Button(contCurrentTime, KACResources.styleContent))
            {
                switch (Settings.TimeFormat)
                {
                    case KACTime.PrintTimeFormat.TimeAsUT: Settings.TimeFormat = KACTime.PrintTimeFormat.KSPString; break;
                    case KACTime.PrintTimeFormat.KSPString: Settings.TimeFormat = KACTime.PrintTimeFormat.DateTimeString; break;
                    case KACTime.PrintTimeFormat.DateTimeString: Settings.TimeFormat = KACTime.PrintTimeFormat.TimeAsUT; break;
                    default: Settings.TimeFormat = KACTime.PrintTimeFormat.KSPString; break;
                }
                Settings.Save();
            }
            GUILayout.EndHorizontal();

            if (Settings.ShowEarthTime)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Earth Time:", "Hide Display of \"Real\" Time"), KACResources.styleHeadingEarth))
                {
                    Settings.ShowEarthTime = !Settings.ShowEarthTime;
                }
                GUILayout.Label(DateTime.Now.ToLongTimeString(), KACResources.styleContentEarth);
                if (DrawToggle(ref _ShowEarthAlarm, new GUIContent(KACResources.btnAdd, "Add New Alarm..."), KACResources.styleSmallButton) && _ShowEarthAlarm)
                {
                    //reset the add stuff
                    NewEarthAlarm();
                    _ShowSettings = false;
                    _ShowEditPane = false;
                    _ShowAddPane = false;
                }
                GUILayout.EndHorizontal();
            }


            GUILayout.EndVertical();
            SetTooltipText();
            GUI.DragWindow();
        }

        String strAlarmEarthHour="";
        String strAlarmEarthMin = "";

        //Display minimal info about the next alarm
        private void WindowLayout_Minimized()
        {
            KACAlarm nextAlarm = null;

            //Find the Alarm to display
            if (Settings.Alarms != null)
            {
                if (Settings.WindowMinimizedType == MiminalDisplayType.NextAlarm)
                {
                    foreach (KACAlarm tmpAlarm in Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title))
                    {
                        Boolean blnSwitch = false;
                        if (tmpAlarm.AlarmTime.UT > KACWorkerGameState.CurrentTime.UT && tmpAlarm.Enabled && !tmpAlarm.Actioned)
                        {
                            if (nextAlarm == null)
                            {
                                blnSwitch = true;
                            }
                            else
                            {
                                if (tmpAlarm.AlarmTime.UT < nextAlarm.AlarmTime.UT)
                                    blnSwitch = true;
                            }

                        }
                        if (blnSwitch)
                            nextAlarm = tmpAlarm;
                    }
                }
                else
                {
                    nextAlarm = Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title).OrderBy(a=>a.AlarmTime.UT).FirstOrDefault();
                }
            }

            if (nextAlarm == null)
            {
                GUILayout.Label("No Enabled Future Alarms in list");
            }
            else
            {
                if (Event.current.type == EventType.repaint)
                    rectScrollview = new Rect(0, 0, 0, 0);
                if (DrawAlarmLine(nextAlarm))
                    Settings.Alarms.Remove(nextAlarm);
            }
        }

        //Display Full alarm list 
        public static Rect rectScrollview;
        Vector2 scrollPosition = Vector2.zero;
        private void WindowLayout_AlarmList()
        {
            GUIStyle styleTemp = new GUIStyle();


            scrollPosition = GUILayout.BeginScrollView(scrollPosition, styleTemp);
            if (Settings.Alarms.CountInSave(HighLogic.CurrentGame.Title) == 0)
            {
                GUILayout.Label("No Alarms in the List");
            }
            else
            {
                List<KACAlarm> AlarmsToRemove = new List<KACAlarm>();
                List<KACAlarm> AlarmsToSort = Settings.Alarms.BySaveName(HighLogic.CurrentGame.Title);
                AlarmsToSort.Sort(KACAlarm.SortByUT);
                foreach (KACAlarm tmpAlarm in AlarmsToSort)
                {
                    //Draw a line for each alarm, returns true is person clicked delete
                    if (DrawAlarmLine(tmpAlarm))
                        AlarmsToRemove.Add(tmpAlarm);
                }

                if (AlarmsToRemove.Count > 0)
                {
                    foreach (KACAlarm tmpAlarm in AlarmsToRemove)
                    {
                        Settings.Alarms.Remove(tmpAlarm);
                        Settings.SaveAlarms();
                    }
                    //is the game paused, yet we deleted any active pause alarms??
                    if (Settings.Alarms.FirstOrDefault(a => (a.AlarmWindowID != 0 && a.PauseGame == true)) == null)
                    {
                        if (FlightDriver.Pause)
                            FlightDriver.SetPause(false);
                    }
                }

            }
            GUILayout.EndScrollView();
            //Get the visible portion of the Scrollview and record it for hittesting later - needs to just be a box from the 0,0 point for the hit test
            // not sure why as the cursor test point is from the screen 0,0
            if (Event.current.type == EventType.repaint)
                rectScrollview = new Rect(0, scrollPosition.y, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height);

        }

        private Boolean DrawAlarmLine(KACAlarm tmpAlarm)
        {
            Boolean blnReturn = false;

            GUILayout.BeginHorizontal();

            switch (tmpAlarm.TypeOfAlarm)
            {
                case KACAlarm.AlarmType.Raw:
                    GUILayout.Label(KACResources.iconRaw, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Maneuver:
                case KACAlarm.AlarmType.ManeuverAuto:
                    GUILayout.Label(KACResources.iconMNode, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.SOIChange:
                case KACAlarm.AlarmType.SOIChangeAuto:
                    GUILayout.Label(KACResources.iconSOI, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Transfer:
                case KACAlarm.AlarmType.TransferModelled:
                    GUILayout.Label(KACResources.iconXFer, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Apoapsis:
                    GUILayout.Label(KACResources.iconAp, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Periapsis:
                    GUILayout.Label(KACResources.iconPe, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.AscendingNode:
                    GUILayout.Label(KACResources.iconAN, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.DescendingNode:
                    GUILayout.Label(KACResources.iconDN, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.LaunchRendevous:
                    GUILayout.Label(KACResources.iconLaunchRendezvous, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Closest:
                    GUILayout.Label(KACResources.iconClosest, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Distance:
                    //TODO - SOMETHING HERE MAYBE
                    GUILayout.Label(KACResources.iconClosest, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.Crew:
                    GUILayout.Label(KACResources.iconCrew, KACResources.styleAlarmIcon);
                    break;
                case KACAlarm.AlarmType.EarthTime:
                    GUILayout.Label(KACResources.iconEarth, KACResources.styleAlarmIcon);
                    break;
                default:
                    GUILayout.Label(KACResources.iconNone, KACResources.styleAlarmIcon);
                    break;
            }

            //Set the Content up
            //int intMaxWidth = intTestheight;
            //String strTimeToAlarm = String.Format(" ({0})",KerbalTime.PrintInterval(tmpAlarm.Remaining, Settings.TimeFormat));
            //float fTimeToAlarmWidth;
            //KACResources.styleAlarmText.CalcMinMaxWidth(new GUIContent(strTimeToAlarm),out fOutMin1,out fOutMax1);
            //fTimeToAlarmWidth = fOutMax1;

            //String strTextToDisplay = tmpAlarm.Name;
            //KACResources.styleAlarmText.CalcMinMaxWidth(new GUIContent(strTextToDisplay), out fOutMin, out fOutMax);
            //while (strTextToDisplay.Length>10 &&(  fOutMax+fTimeToAlarmWidth>intMaxWidth) )
            //{
            //    strTextToDisplay = strTextToDisplay.Remove(strTextToDisplay.Length - 2);
            //    KACResources.styleAlarmText.CalcMinMaxWidth(new GUIContent(strTextToDisplay), out fOutMin, out fOutMax);
            //}

            ////String strLabelText = strTextToDisplay + strTimeToAlarm;
            //String strTimeText = String.Format("({0})", KerbalTime.PrintInterval(tmpAlarm.Remaining, Settings.TimeFormat));
            //String strLabelText = tmpAlarm.Name;

            //GUIStyle styleLabel = new GUIStyle(KACResources.styleAlarmText);
            //if ((!tmpAlarm.Enabled || tmpAlarm.Actioned))
            //    styleLabel.normal.textColor=Color.gray;

            //GUIStyle styleTime = new GUIStyle(styleLabel);
            //styleTime.stretchWidth = true;

            //GUIContent contAlarmLabel = new GUIContent(strLabelText, tmpAlarm.Notes);
            //GUIContent contAlarmTime = new GUIContent(strTimeText, tmpAlarm.Notes);

            ////calc correct width for first part
            //KACResources.styleAlarmText.CalcMinMaxWidth(contAlarmTime, out fOutMin, out fOutMax);
            //styleLabel.CalcMinMaxWidth(contAlarmLabel, out fOutMin1, out fOutMax1);

            //int intMaxWidth = intTestheight;
            //if (fOutMax1 + fOutMax > intMaxWidth)
            //    fOutMax1 = intMaxWidth - fOutMax;

            //if ((alarmEdit == tmpAlarm) && _ShowEditPane)
            //{
            //    intMaxWidth -= 20;
            //}

            //float width1 = fOutMin1;

            String strLabelText = "";
            strLabelText = String.Format("{0} ({1})", tmpAlarm.Name, KACTime.PrintInterval(tmpAlarm.Remaining, Settings.TimeFormat));

            GUIStyle styleLabel = new GUIStyle( KACResources.styleAlarmText);
            if ((!tmpAlarm.Enabled || tmpAlarm.Actioned))
                styleLabel.normal.textColor=Color.gray;
            GUIContent contAlarmLabel = new GUIContent(strLabelText, tmpAlarm.Notes);

            //Calc the line height
            ////////////////////////////////////////////////////
            //Currently the max width doesnt work out when an alarm is in edit mode correctly in edit mode
            //Also need to test max list length and when the scrollbar kicks in
            ///////////////////////////////////////////////////
            Single sOutMin1,sOutMax1;
            styleLabel.CalcMinMaxWidth(contAlarmLabel,out sOutMin1, out sOutMax1);
            tmpAlarm.AlarmLineWidth  = Convert.ToInt32(sOutMax1);
            Int32 intMaxwidth = 220;// 228;
            if (_ShowEditPane && (alarmEdit == tmpAlarm)) intMaxwidth = 198;// 216;
            tmpAlarm.AlarmLineHeight = Convert.ToInt32(styleLabel.CalcHeight(contAlarmLabel, intMaxwidth));

            //Draw a button that looks like a label.
            if (GUILayout.Button(contAlarmLabel, styleLabel, GUILayout.MaxWidth(218)))
            {
                if (!_ShowSettings)
                {
                    if (alarmEdit == tmpAlarm)
                    {
                        //If there was an alarm open, then save em again
                        if (_ShowEditPane) Settings.Save();
                        _ShowEditPane = !_ShowEditPane;
                    }
                    else
                    {
                        //If there was an alarm open, then save em again
                        if (_ShowEditPane) Settings.Save();
                        alarmEdit = tmpAlarm;
                        _ShowEditPane = true;
                        _ShowSettings = false;
                        _ShowAddPane = false;
                    }
                }
            }

            if ((alarmEdit == tmpAlarm) && _ShowEditPane)
            {
                GUILayout.Label(new GUIContent(KACResources.iconEdit, "Editing..."), KACResources.styleLabelWarp);
            }
            if (tmpAlarm.PauseGame)
            {
                GUILayout.Label(new GUIContent(KACResources.GetPauseListIcon(tmpAlarm.WarpInfluence),"Pause"), KACResources.styleLabelWarp);
            }
            else if (tmpAlarm.HaltWarp)
            {
                GUILayout.Label(new GUIContent(KACResources.GetWarpListIcon(tmpAlarm.WarpInfluence), "Kill Warp"), KACResources.styleLabelWarp);
            }
            else
            {
                GUILayout.Label(new GUIContent(KACResources.iconNone), KACResources.styleLabelWarp);
            }

            if (GUILayout.Button(new GUIContent(KACResources.btnRedCross,"Delete Alarm"), GUI.skin.button, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                blnReturn = true;

            GUILayout.EndHorizontal();

            return blnReturn;
        }

        public void ResetPanes()
        {
            _ShowAddPane = false;
            _ShowEditPane = false;
            _ShowSettings = false;
        }
        
        #endregion

        private void WindowLayout_CommonFields(ref String strName, ref String strMessage, ref int Action, ref Double Margin, KACAlarm.AlarmType TypeOfAlarm, int WindowHeight)
        {
            KACTimeStringArray tmpTime = new KACTimeStringArray(Margin,KACTimeStringArray.TimeEntryPrecisionEnum.Hours);
            WindowLayout_CommonFields(ref strName, ref strMessage, ref Action, ref tmpTime, TypeOfAlarm, WindowHeight);
            Margin = tmpTime.UT;
        }

        /// <summary>
        /// Layout of Common Parts of every alarm
        /// </summary>
        private void WindowLayout_CommonFields(ref String strName, ref String strMessage, ref int Action, ref KACTimeStringArray Margin, KACAlarm.AlarmType TypeOfAlarm, int WindowHeight)
        {
            //Two Columns
            GUILayout.Label("Common Alarm Properties", KACResources.styleAddSectionHeading);
            GUILayout.BeginVertical(KACResources.styleAddFieldAreas, GUILayout.Height(WindowHeight));

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(90));
            GUILayout.Label("Alarm Name:", KACResources.styleAddHeading);
            GUILayout.Label("Message:", KACResources.styleAddHeading);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(260), GUILayout.MaxWidth(260));
            strName = GUILayout.TextField(strName, KACResources.styleAddField).Replace("|", "");
            strMessage = GUILayout.TextArea(strMessage, KACResources.styleAddField).Replace("|", "");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            //Full width one under the two columns for the kill time warp
            DrawAlarmActionChoice(ref Action, "On Alarm:", 90);

            if (TypeOfAlarm != KACAlarm.AlarmType.Raw && TypeOfAlarm != KACAlarm.AlarmType.EarthTime && TypeOfAlarm != KACAlarm.AlarmType.Crew)
            {
                DrawTimeEntry(ref Margin, KACTimeStringArray.TimeEntryPrecisionEnum.Hours, "Alarm Margin:", 90);
            }

            GUILayout.EndVertical();

        }

        /// <summary>
        /// Layout of Common Parts of every alarm
        /// </summary>
        private void WindowLayout_CommonFields2(ref String strName, ref Boolean blnAttachVessel, ref KACAlarm.AlarmAction Action, ref KACTimeStringArray Margin, KACAlarm.AlarmType TypeOfAlarm, int WindowHeight)
        {
            //Two Columns
            String strTitle = "";
            switch (TypeOfAlarm)
            {
                case KACAlarm.AlarmType.Raw: strTitle = "Raw Time"; break;
                case KACAlarm.AlarmType.Maneuver: strTitle = "Maneuver Node"; break;
                case KACAlarm.AlarmType.SOIChange: strTitle = "SOI Change"; break;
                case KACAlarm.AlarmType.Transfer: strTitle = "Transfer Window"; break;
                case KACAlarm.AlarmType.TransferModelled: strTitle = "Transfer Window"; break;
                case KACAlarm.AlarmType.Apoapsis: strTitle = "Apoapsis"; break;
                case KACAlarm.AlarmType.Periapsis: strTitle = "Periapsis"; break;
                case KACAlarm.AlarmType.AscendingNode: strTitle = "Ascending Node"; break;
                case KACAlarm.AlarmType.DescendingNode: strTitle = "Descending Node"; break;
                case KACAlarm.AlarmType.LaunchRendevous: strTitle = "Launch Ascent"; break;
                case KACAlarm.AlarmType.Closest: strTitle = "Closest Approach"; break;
                case KACAlarm.AlarmType.Distance: strTitle = "Target Distance"; break;
                case KACAlarm.AlarmType.Crew: strTitle = "Crew"; break;
                case KACAlarm.AlarmType.EarthTime: strTitle = "Earth Time"; break;
                default: strTitle = "Raw Time"; break;
            }
            strTitle += " Alarm - Common Properties";
            GUILayout.Label(strTitle, KACResources.styleAddSectionHeading);
            GUILayout.BeginVertical(KACResources.styleAddFieldAreas, GUILayout.Height(WindowHeight));

            if (KACWorkerGameState.CurrentGUIScene == GameScenes.TRACKSTATION)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Selected Vessel:", KACResources.styleAddHeading);
                String strVesselName = "No Selected Vessel";
                if (KACWorkerGameState.CurrentVessel != null) strVesselName = KACWorkerGameState.CurrentVessel.vesselName;
                GUILayout.Label(strVesselName, KACResources.styleLabelWarning);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Alarm:", KACResources.styleAddHeading, GUILayout.Width(60));
            strName = GUILayout.TextField(strName, KACResources.styleAddField, GUILayout.MaxWidth(200)).Replace("|", "");

            GUIContent guiBtnMessages = new GUIContent(KACResources.btnChevRight, "Show Extra Details");
            if (_ShowAddMessages) guiBtnMessages = new GUIContent(KACResources.btnChevLeft, "Hide Details");
            if (GUILayout.Button(guiBtnMessages, KACResources.styleSmallButton))
                _ShowAddMessages = !_ShowAddMessages;
            GUILayout.EndHorizontal();


            if (ScenesForAttachOption.Contains(KACWorkerGameState.CurrentGUIScene) && TypesForAttachOption.Contains(TypeOfAlarm)
                && KACWorkerGameState.CurrentVessel!=null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                DrawCheckbox(ref blnAttachVessel, "Attach to Active Vessel");
                GUILayout.EndHorizontal();
            }

            //Full width one under the two columns for the kill time warp
            DrawAlarmActionChoice2(ref Action, "Action:", 60);

            if (TypeOfAlarm != KACAlarm.AlarmType.Raw && TypeOfAlarm != KACAlarm.AlarmType.EarthTime && TypeOfAlarm != KACAlarm.AlarmType.Crew)
            {
                DrawTimeEntry(ref Margin, KACTimeStringArray.TimeEntryPrecisionEnum.Hours, "Margin:", 60);
            }
            GUILayout.EndVertical();
        }


        #region "Control Drawing"
        /// <summary>
        /// Draws a Toggle Button and sets the boolean variable to the state of the button
        /// </summary>
        /// <param name="blnVar">Boolean variable to set and store result</param>
        /// <param name="ButtonText"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns>True when the button state has changed</returns>
        public Boolean DrawToggle(ref Boolean blnVar, String ButtonText, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, ButtonText, style, options);

            return ToggleResult(ref blnVar, ref  blnReturn);
        }

        public Boolean DrawToggle(ref Boolean blnVar, Texture image, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, image, style, options);

            return ToggleResult(ref blnVar, ref blnReturn);
        }

        public Boolean DrawToggle(ref Boolean blnVar, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Boolean blnReturn = GUILayout.Toggle(blnVar, content, style, options);

            return ToggleResult(ref blnVar, ref blnReturn);
        }

        private Boolean ToggleResult(ref Boolean Old, ref Boolean New)
        {
            if (Old != New)
            {
                Old = New;
                DebugLogFormatted("Toggle Changed:" + New.ToString());
                return true;
            }
            return false;
        }



        public Boolean DrawTextBox(ref String strVar, GUIStyle style, params GUILayoutOption[] options)
        {
            String strReturn = GUILayout.TextField(strVar, style, options);
            if (strReturn != strVar)
            {
                strVar = strReturn;
                DebugLogFormatted("String Changed:" + strVar.ToString());
                return true;
            }
            return false;
        }


        /// <summary>
        /// Draws a toggle button like a checkbox
        /// </summary>
        /// <param name="blnVar"></param>
        /// <returns>True when check state has changed</returns>
        public Boolean DrawCheckbox(ref Boolean blnVar, String strText, params GUILayoutOption[] options)
        {
            return DrawCheckbox(ref blnVar, new GUIContent(strText),15, options);
        }
        public Boolean DrawCheckbox(ref Boolean blnVar, GUIContent content, params GUILayoutOption[] options)
        {
            return DrawCheckbox(ref blnVar, content , 15, options);
        }
        public Boolean DrawCheckbox(ref Boolean blnVar, String strText, int CheckboxSpace, params GUILayoutOption[] options)
        {
            return DrawCheckbox(ref blnVar, new GUIContent(strText), CheckboxSpace, options);
        }
        //CHANGED
        /// <summary>
        /// Draws a toggle button like a checkbox
        /// </summary>
        /// <param name="blnVar"></param>
        /// <returns>True when check state has changed</returns>
        public Boolean DrawCheckbox(ref Boolean blnVar,  GUIContent content, int CheckboxSpace , params GUILayoutOption[] options)
        {
            // return DrawToggle(ref blnVar, strText, KACResources.styleCheckbox, options);
            Boolean blnReturn = false;
            Boolean blnToggleInitial = blnVar;

            GUILayout.BeginHorizontal();
            //Draw the radio
            DrawToggle(ref blnVar, "", KACResources.styleCheckbox, options);
            //Spacing
            GUILayout.Space(CheckboxSpace);
            
            //And the button like a label
            if (GUILayout.Button(content,KACResources.styleCheckboxLabel, options))
            {
                //if its clicked then toggle the boolean
                blnVar = !blnVar;
                KACWorker.DebugLogFormatted("Toggle Changed:" + blnVar);
            }

            GUILayout.EndHorizontal();

            //If output value doesnt = input value
            if (blnToggleInitial != blnVar)
            {
                //KACWorker.DebugLogFormatted("Toggle recorded:" + blnVar);
                blnReturn = true;
            }
            return blnReturn;
        }

        public Boolean DrawRadioListVertical(ref int Selected, params String[] Choices)
        {
            return DrawRadioList(false, ref Selected, Choices);
        }
        public Boolean DrawRadioList(ref int Selected, params String[] Choices)
        {
            return DrawRadioList(true, ref Selected, Choices);
        }
        public Boolean DrawRadioList(Boolean Horizontal, ref int Selected, params String[] Choices)
        {
            int InitialChoice = Selected;

            if (Horizontal)
                GUILayout.BeginHorizontal();
            else
                GUILayout.BeginVertical();

            for (int intChoice = 0; intChoice < Choices.Length; intChoice++)
            {
                //checkbox
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle((intChoice == Selected), "", KACResources.styleCheckbox))
                    Selected = intChoice;
                //button that looks like a label
                if (GUILayout.Button(Choices[intChoice], KACResources.styleCheckboxLabel))
                    Selected = intChoice;
                GUILayout.EndHorizontal();
            }
            if(Horizontal)
                GUILayout.EndHorizontal();
            else
                GUILayout.EndVertical();

            if (InitialChoice != Selected)
                DebugLogFormatted(String.Format("Radio List Changed:{0} to {1}", InitialChoice, Selected));


            return !(InitialChoice == Selected);
        }

        public Boolean DrawAlarmActionChoice(ref int intChoice, String LabelText, int LabelWidth)
        {
            Boolean blnReturn = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label(LabelText, KACResources.styleAddHeading, GUILayout.Width(LabelWidth));
            blnReturn = DrawRadioList(ref intChoice, "Message Only", "Kill Time Warp", "Pause Game");
            GUILayout.EndHorizontal();
            return blnReturn;
        }
        public Boolean DrawAlarmActionChoice2(ref KACAlarm.AlarmAction Choice, String LabelText, int LabelWidth)
        {
            Boolean blnReturn = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label(LabelText, KACResources.styleAddHeading, GUILayout.Width(LabelWidth-10));
            int intChoice = (int)Choice;
            blnReturn = DrawRadioList(ref intChoice, "Message", "Kill Warp", "Pause");
            Choice = (KACAlarm.AlarmAction)intChoice;
            GUILayout.EndHorizontal();
            return blnReturn;
        }



        public Boolean DrawTimeEntry(ref KACTimeStringArray time, KACTimeStringArray.TimeEntryPrecisionEnum Prec, params GUILayoutOption[] options)
        {
            return DrawTimeEntry(ref time, Prec, "", 0, 40,20);
        }

        public Boolean DrawTimeEntry(ref KACTimeStringArray time, KACTimeStringArray.TimeEntryPrecisionEnum Prec, String LabelText, int LabelWidth, params GUILayoutOption[] options)
        {
            return DrawTimeEntry(ref time, Prec, LabelText, LabelWidth, 40,20);
        }

        public Boolean DrawTimeEntry(ref KACTimeStringArray time, KACTimeStringArray.TimeEntryPrecisionEnum Prec, String LabelText, int LabelWidth, int FieldWidth, int SuffixWidth, params GUILayoutOption[] options)
        {
            Boolean blnReturn = false;

            GUILayout.BeginHorizontal();
            if (LabelText!="")
                GUILayout.Label(LabelText, KACResources.styleAddHeading, GUILayout.Width(LabelWidth));
            
            String strTemp;
            if (Prec >= KACTimeStringArray.TimeEntryPrecisionEnum.Years)
            {
                strTemp = time.Years;
                if (DrawTimeField(ref strTemp, "y", FieldWidth, SuffixWidth))
                {
                    blnReturn = true;
                    time.Years = strTemp;
                }
            }
            if (Prec >= KACTimeStringArray.TimeEntryPrecisionEnum.Days)
            {
                strTemp = time.Days;
                if (DrawTimeField(ref strTemp, "d", FieldWidth, SuffixWidth))
                {
                    blnReturn = true;
                    time.Days = strTemp;
                }
            }
            if (Prec >= KACTimeStringArray.TimeEntryPrecisionEnum.Hours)
            {
                strTemp = time.Hours;
                if (DrawTimeField(ref strTemp, "h", FieldWidth, SuffixWidth))
                {
                    blnReturn = true;
                    time.Hours = strTemp;
                }
            }
            if (Prec >= KACTimeStringArray.TimeEntryPrecisionEnum.Minutes)
            {
                strTemp = time.Minutes;
                if (DrawTimeField(ref strTemp, "m", FieldWidth, SuffixWidth))
                {
                    blnReturn = true;
                    time.Minutes = strTemp;
                }
            }
            if (Prec >= KACTimeStringArray.TimeEntryPrecisionEnum.Seconds)
            {
                strTemp = time.Seconds;
                if (DrawTimeField(ref strTemp, "s", FieldWidth, SuffixWidth))
                {
                    blnReturn = true;
                    time.Seconds = strTemp;
                }
            }
            //blnReturn = DrawTimeField(ref time.Seconds, "s", FieldWidth, SuffixWidth) && blnReturn;
            if (!time.Valid)
            {
                GUILayout.Label(new GUIContent("*","Invalid fields treated as 0"), KACResources.styleLabelError, GUILayout.Width(SuffixWidth));
            }
            GUILayout.EndHorizontal();

            return blnReturn;
        }

        public Boolean DrawTimeField(ref String Value, String LabelText, int FieldWidth,int SuffixWidth)
        {
            Boolean blnReturn = false;
            Int32 intParse;
            GUIStyle styleTextBox = KACResources.styleAddField;
            GUIContent contText = new GUIContent(Value);
            Boolean BlnIsNum = Int32.TryParse(Value,out intParse);
            if (!BlnIsNum) styleTextBox = KACResources.styleAddFieldError;

            //styleTextBox.alignment = TextAnchor.MiddleRight;
            blnReturn = DrawTextBox(ref Value, styleTextBox, GUILayout.MaxWidth(FieldWidth));

            //String strReturn = GUILayout.TextField(Value, styleTextBox, GUILayout.MaxWidth(FieldWidth));
            //Attempt at fancy tint - looks weird
            //if(!BlnIsNum)
            //{
            //    Rect overlay = GUILayoutUtility.GetLastRect();
            //    overlay.x -= 1; overlay.y -= 2; overlay.width += 2; overlay.height += 5;
            //    GUI.depth--;
            //    GUI.Label(overlay,"", KACResources.styleAddFieldErrorOverlay);
            //}

            GUILayout.Label(LabelText,KACResources.styleAddHeading,GUILayout.Width(SuffixWidth));

            return blnReturn;
        }

        public Boolean DrawButtonList(ref KACAlarm.AlarmType selType, params GUIContent[] Choices)
        {
            int Selection = KACAlarm.AlarmTypeToButton[selType];
            if (KACWorkerGameState.CurrentGUIScene == GameScenes.TRACKSTATION) Selection = KACAlarm.AlarmTypeToButtonTS[selType];
            else if (KACWorkerGameState.CurrentGUIScene == GameScenes.SPACECENTER) Selection = KACAlarm.AlarmTypeToButtonSC[selType];

            Boolean blnReturn = DrawButtonList(ref Selection, Choices);
            if (blnReturn)
            {
                if (KACWorkerGameState.CurrentGUIScene== GameScenes.TRACKSTATION)
                    selType = KACAlarm.AlarmTypeFromButtonTS[Selection];
                else if (KACWorkerGameState.CurrentGUIScene == GameScenes.SPACECENTER)
                    selType = KACAlarm.AlarmTypeFromButtonSC[Selection];
                else
                    selType = KACAlarm.AlarmTypeFromButton[Selection];
            }
            return blnReturn;
        }

        public Boolean DrawButtonList(ref int Selected, params GUIContent[] Choices)
        {
            int InitialChoice = Selected;

            GUILayout.BeginHorizontal();

            for (int intChoice = 0; intChoice < Choices.Length; intChoice++)
            {
                //button
                Boolean blnResult=(Selected==intChoice);
                if (DrawToggle(ref blnResult,Choices[intChoice], KACResources.styleButtonList))
                {
                    if (blnResult)
                        Selected=intChoice;
                }
            }
            GUILayout.EndHorizontal();

            if (InitialChoice != Selected)
                DebugLogFormatted(String.Format("Button List Changed:{0} to {1}", InitialChoice, Selected));


            return !(InitialChoice == Selected);
        }


        #endregion


    }


}
