using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport; // For Generic Device Support
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharp.Scheduler;
using Crestron.SimplSharpPro.Keypads;

namespace Ex16_Scheduler
{
    public class ControlSystem : CrestronControlSystem
    {
        #region GlobalVariables

        private HzKpcn myKeypad;
        private ScheduledEventGroup myEventGroup;
        private ScheduledEvent myEvent;
        
        #endregion

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);

                #region UIs

                myKeypad = new HzKpcn(0x07, this);
                myKeypad.Register();
                myKeypad.ButtonStateChange += new ButtonEventHandler(myKeypad_ButtonStateChange);

                #endregion

                #region Scheduler

                myEventGroup = new ScheduledEventGroup("myEventGroup");
                myEventGroup.RetrieveAllEvents();
                
                #endregion


                if (this.SupportsComPort)
                {
                    this.ComPorts[1].Register();
                    this.ComPorts[1].SetComPortSpec(
                        ComPort.eComBaudRates.ComspecBaudRate9600,
                        ComPort.eComDataBits.ComspecDataBits8,
                        ComPort.eComParityType.ComspecParityNone,
                        ComPort.eComStopBits.ComspecStopBits1,
                        ComPort.eComProtocolType.ComspecProtocolRS232,
                        ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                        ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                        false);

                }

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        void myKeypad_ButtonStateChange(GenericBase device, ButtonEventArgs args)
        {
            try
            {
                if (args.Button.State == eButtonState.Pressed)
                {
                    CrestronConsole.PrintLine("Button {0} was pressed.", args.Button.Number);
                    switch (args.Button.Number)
                    {
                        case 1:
                            var selWeekDays = new ScheduledEventCommon.eWeekDays();
                            selWeekDays |= ScheduledEventCommon.eWeekDays.Monday;
                            selWeekDays |= ScheduledEventCommon.eWeekDays.Tuesday;
                            selWeekDays |= ScheduledEventCommon.eWeekDays.Friday;

                            CrestronConsole.PrintLine("Initializing program response for button {0}", args.Button.Number);
                            myEventGroup.ClearAllEvents();
                            if (myEventGroup.ScheduledEvents.ContainsKey("MyEvent") == false)
                            {
                                myEvent = new ScheduledEvent("MyEvent", myEventGroup);
                                myEvent.Description = "My first event";
                                myEvent.DateAndTime.SetAbsoluteEventTime(2019, 03, 11, 10, 30);
                                myEvent.Acknowledgeable = false;
                                myEvent.Persistent = true;
                                myEvent.UserCallBack += new ScheduledEvent.UserEventCallBack(myEvent_UserCallBack);
                                myEvent.Recurrence.Weekly(selWeekDays);
                                /*
                                myEvent.Recurrence.Weekly(
                                    ScheduledEventCommon.eWeekDays.Monday & 
                                    ScheduledEventCommon.eWeekDays.Tuesday &
                                    ScheduledEventCommon.eWeekDays.Friday, 10);
                                 */
                                myEvent.Enable();

                                CrestronConsole.PrintLine("Event {0} created for {1}:{2}",
                                    myEvent.Name,
                                    myEvent.DateAndTime.Hour,
                                    myEvent.DateAndTime.Minute);
                            }
                            break;
                        case 2:
                            CrestronConsole.PrintLine("Initializing program response for button {0}", args.Button.Number);
                            myEventGroup.ClearAllEvents();
                            break;

                        case 3:
                            CrestronConsole.PrintLine("Initializing program response for button {0}", args.Button.Number);
                            CrestronConsole.PrintLine("Event {0} is scheduled for {1}/{2}/{3} {4}:{5}",
                                myEvent.Name,
                                myEvent.DateAndTime.Month,
                                myEvent.DateAndTime.Day,
                                myEvent.DateAndTime.Year,
                                myEvent.DateAndTime.Hour,
                                myEvent.DateAndTime.Minute);
                            break;

                        case 4:
                            ComPorts[1].Send(String.Format("{0}{1}{2}{3}{4}", 
                                Convert.ToChar(0x31), 
                                Convert.ToChar(0x32), 
                                Convert.ToChar(0x33), 
                                Convert.ToChar(0x34), 
                                Convert.ToChar(0x0D)));
                            break;
                    }


                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception occurred --> {0}", e.Message);
                ErrorLog.Notice(String.Format("Exception occurred --> {0}", e.Message));
            }
            finally { }
        }

        

        void myEvent_UserCallBack(ScheduledEvent SchEvent, ScheduledEventCommon.eCallbackReason type)
        {
            CrestronConsole.PrintLine("Event {0}, triggered @ {1}:{2}", SchEvent.Name, SchEvent.DateAndTime.Hour.ToString(), SchEvent.DateAndTime.Minute.ToString());
        }

        public override void InitializeSystem()
        {
            try
            {

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}