﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Net.Sockets;
using System.Net;
using Cryptool.PluginBase.IO;
using UDPReceiver;
using UDPReceiver.Model;

namespace Cryptool.Plugins.UDPReceiver
{
    [Author("Christopher Konze", "ckonze@uni.de", "University of Kassel", "")]
    [PluginInfo("UDPReceiver.Properties.Resources", "PluginCaption", "PluginTooltip", "UDPReceiver/userdoc.xml", new[] { "UDPReceiver/Images/package.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class UDPReceiver : ICrypComponent
    {
 
        #region Private variables

        private readonly UDPReceiverSettings settings;
        private readonly UDPReceiverQuickWatchPresentation presentation= new UDPReceiverQuickWatchPresentation();

        private IPEndPoint endPoint;

        private HashSet<IPAddress> uniqueSrcIps;
        private List<byte[]> receivedPackages;
        private bool isRunning;
        private bool returnLastPackage = true;

        private DateTime startTime;
        private UdpClient udpSocked;
        private CStreamWriter streamWriter = new CStreamWriter();

        #endregion

        public UDPReceiver()
        {
           settings = new UDPReceiverSettings(this);
        }

        #region Helper Functions

        //the maximum left time in ms till we abroche the user's whised timeout time 
        private int TimeTillTimelimit()
        {
            int timeTillTimeout = settings.Timeout - DateTime.Now.Subtract(startTime).Seconds;
            return (timeTillTimeout <= 0) ? 0 : timeTillTimeout*1000;
        }
        
        // decides on the basis of the given timeout, whether the component continues to wait for more packages
        // remember timeout = 0 means: dont time out
        private bool IsTimeLeft()
        {
            return (settings.Timeout > DateTime.Now.Subtract(startTime).Seconds || settings.Timeout == 0);
        }

        // decides on basis of user's input whether the component is allowed to receiving another package
        // remember PackageLimit  = 0 means: dont limit the amount of packages
        private bool IsPackageLimitNotReached()
        {
            return (settings.PackageLimit < receivedPackages.Count || settings.PackageLimit == 0);
        }
        
        #endregion
       
        #region Data Properties

        [PropertyInfo(Direction.OutputData, "StreamOutput", "StreamOutputTooltip")]
        public ICryptoolStream PackageStream
        {
            get;
            private set;
        }


        [PropertyInfo(Direction.OutputData, "SingleOutput", "SingleOutputTooltip")]
        public byte[] SingleOutput
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members



        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            ProgressChanged(0, 1);

            endPoint = new IPEndPoint(!settings.NetworkDevice? IPAddress.Parse(settings.DeviceIp) : IPAddress.Any, 0);
            udpSocked = new UdpClient(settings.Port);
  
            //init / reset
            uniqueSrcIps = new HashSet<IPAddress>();
            isRunning = true;
            startTime = DateTime.Now;
            receivedPackages = new List<byte[]>();

            // reset gui
            presentation.ClearList();
            presentation.RefreshMetaData(0, 0);
            presentation.SetStartTime(startTime.ToLongTimeString());

            //stream prepair
            streamWriter = new CStreamWriter();
            PackageStream = streamWriter;
            OnPropertyChanged("PackageStream");
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(1, 100);
            while (IsTimeLeft() && IsPackageLimitNotReached() && isRunning)
            {
                try
                {
                    // read
                    udpSocked.Client.ReceiveTimeout = TimeTillTimelimit();
                    byte[] data = udpSocked.Receive(ref endPoint);

                    // package recieved. fill local storage
                    uniqueSrcIps.Add(endPoint.Address);
                    receivedPackages.Add(data);

                    // redirect to gui
                    presentation.AddPresentationPackage(new PresentationPackage
                    {
                        IPFrom = endPoint.Address.ToString(),
                        Payload = BitConverter.ToString(data)
                    });
                    presentation.RefreshMetaData(receivedPackages.Count, uniqueSrcIps.Count);

                    // set outputs
                    streamWriter.Write(data);
                    if (returnLastPackage) //change single output if no item is selected
                    {
                        SingleOutput = data;
                        OnPropertyChanged("SingleOutput");
                    }

                } 
                catch (SocketException e)
                {
                    if (e.ErrorCode != 10004 || isRunning) // if we stop during the socket waits,if we stop 
                    {                             //during the socket waits, we won't show an error message
                       throw;
                    }
                } 
             
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
           udpSocked.Close();
           streamWriter.Close();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            udpSocked.Close(); //we have to close it forcely, but we catch the error
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            presentation.ListView.MouseDoubleClick -= MouseDoubleClick;
            presentation.ListView.MouseDoubleClick += MouseDoubleClick;
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
            presentation.ListView.MouseDoubleClick -= MouseDoubleClick;
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void MouseDoubleClick(object sender, EventArgs e)
        {
            if (-1 < presentation.ListView.SelectedIndex
                && presentation.ListView.SelectedIndex < receivedPackages.Capacity)
            {
                returnLastPackage = false;
                SingleOutput = receivedPackages[presentation.ListView.SelectedIndex];
                OnPropertyChanged("SingleOutput");
            } 
            else
            {
                returnLastPackage = true;
            }
        }


        #endregion
    }
}