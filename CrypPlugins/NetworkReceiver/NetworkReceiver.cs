﻿/*
    Copyright 2013 Christopher Konze, University of Kassel
 
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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Net.Sockets;
using System.Net;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.NetworkReceiver
{
    [Author("Christopher Konze", "Christopher.Konze@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("NetworkReceiver.Properties.Resources", "PluginCaption", "PluginTooltip", "NetworkReceiver/userdoc.xml", new[] { "NetworkReceiver/Images/package.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class NetworkReceiver : ICrypComponent
    {
 
        #region Private variables

        private readonly NetworkReceiverSettings settings;
        private readonly NetworkReceiverPresentation presentation;
        private Timer calculateSpeedrate = null;

        private IPEndPoint endPoint;

        private HashSet<string> uniqueSrcIps;
        private List<byte[]> lastPackages;
        private int receivedPackagesCount;
        private bool isRunning;
        private bool returnLastPackage = true;

        private DateTime startTime;
        private UdpClient udpSocked;
        private CStreamWriter streamWriter = new CStreamWriter();
       
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Socket serverSocket;
        private Socket clientSocket;

        public int ReceivedDataSize
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }


        #endregion

        public NetworkReceiver()
        {
            presentation = new NetworkReceiverPresentation(this);
            settings = new NetworkReceiverSettings(this);
        }

        #region Helper Functions

        /// <summary>
        /// the maximum left time in ms till we abroche the user's whised timeout time 
        /// </summary>
        /// <returns></returns>
        private int TimeTillTimelimit()
        {
            int timeTillTimeout = settings.Timeout - DateTime.Now.Subtract(startTime).Seconds;
            return (timeTillTimeout <= 0) ? 0 : timeTillTimeout*1000;
        }
        
        
       
        /// <summary>
        /// decides on the basis of the given timeout, whether the component continues to wait for more packages
        /// remember timeout = 0 means: dont time out
        /// </summary>
        /// <returns></returns>
        private bool IsTimeLeft()
        {
            return (settings.Timeout > DateTime.Now.Subtract(startTime).Seconds || settings.Timeout == 0);
        }

        
        /// <summary>
        /// decides on basis of user's input whether the component is allowed to receiving another package
        /// remember PackageLimit  = 0 means: dont limit the amount of packages
        /// </summary>
        /// <returns></returns>
        private bool IsPackageLimitNotReached()
        {
            return (settings.PackageLimit < receivedPackagesCount || settings.PackageLimit == 0);
        }
        
         
        /// <summary>
        /// creates a size string corespornsing to the size of the given amount of bytes with a B or kB ending
        /// </summary>
        /// <returns></returns>
        private string generateSizeString(int size)
        {
            if (size < 1024)
            {
                return size + " B";
            }
            else
            {
                return Math.Round((double)(size / 1024.0),2) + " kB";
            }
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

            if (settings.Protocol == 0)
            {
                endPoint = new IPEndPoint(!settings.NetworkDevice ? IPAddress.Parse(settings.DeviceIp) : IPAddress.Any, 0);
                udpSocked = new UdpClient(settings.Port);
            }
            else if (settings.Protocol == 1)
            {

                tcpListener = new TcpListener(!settings.NetworkDevice ? IPAddress.Parse(settings.DeviceIp) : IPAddress.Any, settings.Port);
                tcpListener.Start();

            }
            
  
            //init / reset
            uniqueSrcIps = new HashSet<string>();
            isRunning = true;
            returnLastPackage = true;
            startTime = DateTime.Now;
            lastPackages = new List<byte[]>();
            receivedPackagesCount = 0;

            // reset gui
            presentation.ClearPresentation();
            presentation.SetStaticMetaData(startTime.ToLongTimeString(), settings.Port.ToString(CultureInfo.InvariantCulture));  
      
            //start speedrate calculator
            calculateSpeedrate = new System.Timers.Timer { Interval = settings.SpeedrateIntervall*1000}; // seconds
            calculateSpeedrate.Elapsed += new ElapsedEventHandler(CalculateSpeedrateTick);
            calculateSpeedrate.Start();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {

            while (IsTimeLeft() && IsPackageLimitNotReached() && isRunning)
            {

                try
                {
                    byte[] data = null;
                    string ipFrom = null;
                    if (settings.Protocol == 0) // UDP receiver
                    {
                        // wait for package
                        ProgressChanged(1, 1);
                        udpSocked.Client.ReceiveTimeout = TimeTillTimelimit();
                        data = udpSocked.Receive(ref endPoint);
                        ProgressChanged(0.5, 1);
                         ipFrom = endPoint.Address.ToString();
                    }
                    else if (settings.Protocol == 1) // TCP receiver
                    {
                        //TODO @mirco: ive refactort a bit, cause i dont want to write code twice ;)
                        // your old "data" is now "dataBuffer" and your "buffer" is now "data"
                        
                        #region tcpStuff

                        if (tcpClient != null && tcpClient.Client != null && tcpClient.Connected)
                        {
                            tcpListener.Stop();
                        }
                        else
                        {
                            tcpListener.Start();
                            tcpClient = tcpListener.AcceptTcpClient();
                            tcpClient.ReceiveTimeout = TimeTillTimelimit();
                            networkStream = tcpClient.GetStream();
                        }

                        var dataBuffer = new byte[tcpClient.ReceiveBufferSize];
                        data = new byte[tcpClient.ReceiveBufferSize];
                        networkStream.Read(dataBuffer, 0, dataBuffer.Length);

                        while ((networkStream.Read(dataBuffer, 0, dataBuffer.Length)) != 0)
                        {

                            int counter = 0;
                            /*
                            for (int i = data.Length-1; i >= 0; i--)
                            {
                                if (data[i].ToString() == "62" && data[i - 1].ToString() == "77" && data[i - 2].ToString() == "79" &&
                                       data[i - 3].ToString() == "69" &&
                                       data[i - 4].ToString() == "60")
                                {
                                    buffer = new byte[data.Length-counter-5];

                                    for (int j = 0; j < data.Length-counter-5; j++)
                                    {
                                        buffer[j] = data[j];
                                    }
                                    break;

                                }
                                if (data[i].ToString() == "62" && data[i - 1].ToString() == "77" && data[i - 2].ToString() == "79" &&
                                         data[i - 3].ToString() == "83" &&
                                         data[i - 4].ToString() == "60")
                                {
                                    break;
                                }
                                counter++;
                            }

                            */
                            for (int i = 0; i < dataBuffer.Length; i++)
                            {

                                if (dataBuffer[i].ToString() == "60" && dataBuffer[i + 1].ToString() == "36" &&
                                    dataBuffer[i + 2].ToString() == "69"
                                    && dataBuffer[i + 3].ToString() == "79" && dataBuffer[i + 4].ToString() == "77" &&
                                    dataBuffer[i + 5].ToString() == "36" && dataBuffer[i + 6].ToString() == "62")
                                {
                                    dataBuffer = new byte[counter];

                                    for (int j = 0; j < counter; j++)
                                    {
                                        data[j] = dataBuffer[j];
                                    }
                                    break;

                                }
                                counter++;
                            }
                            

                           ipFrom = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address.ToString();


                        }
                        #endregion
                    }

                    if (data != null && ipFrom != null) // wont be "flase" anyway, but you never know
                    {
                        receivedPackagesCount++;
                       // package recieved. fill local storage
                        if (lastPackages.Count > NetworkReceiverPresentation.MaxStoredPackage)
                        {
                            lastPackages.RemoveAt(lastPackages.Count - 1);
                        }
                        else
                        {
                            lastPackages.Add(data);
                        }
                       
                        uniqueSrcIps.Add(ipFrom);
                        ReceivedDataSize += data.Length;
                        

                        // create Package
                        var packet = new PresentationPackage
                        {
                          PackageSize = generateSizeString(data.Length) + "yte", // 42B + "yte"
                          IPFrom = ipFrom,
                          
                          Payload = (settings.ByteAsciiSwitch
                               ? Encoding.ASCII.GetString(data)
                               : BitConverter.ToString(data))
                        };

                        // update Presentation
                        presentation.UpdatePresentation(packet, receivedPackagesCount, uniqueSrcIps.Count);

                        //update output
                        streamWriter = new CStreamWriter();
                        PackageStream = streamWriter;
                        streamWriter.Write(data);
                        streamWriter.Close();
                        OnPropertyChanged("PackageStream");
                        if (returnLastPackage) //change single output if no item is selected
                        {
                            SingleOutput = data;
                            OnPropertyChanged("SingleOutput");
                        }
                    }
                    ProgressChanged(0.5, 1);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != 10004 || isRunning) // if we stop during the socket waits,if we stop 
                    {                             //during the socket waits, we won't show an error message
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            if (settings.Protocol == 0)
            {
                udpSocked.Close();

            }
            if (settings.Protocol == 1 && tcpListener != null)
            {
                  
            }
            streamWriter.Close();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            calculateSpeedrate.Stop();
            isRunning = false;
            if (settings.Protocol == 0)
            {
                udpSocked.Close(); //we have to close it forcely, but we catch the error
            }
            if (settings.Protocol == 1 && tcpClient != null && tcpListener != null)
            {
                tcpListener.Stop();
                networkStream.Close();
                tcpClient.Close(); 
                tcpClient.Close();
               
            }
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
                && presentation.ListView.SelectedIndex < NetworkReceiverPresentation.MaxStoredPackage)
            {
                returnLastPackage = false;
                SingleOutput = lastPackages[presentation.ListView.SelectedIndex];
                OnPropertyChanged("SingleOutput");
            } 
            else
            {
                returnLastPackage = true;
            }
        }

        /// <summary>
        /// tickmethod for the CalculateSpeedrateTick timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateSpeedrateTick(object sender, EventArgs e)
        {
            var speedrate = ReceivedDataSize/settings.SpeedrateIntervall;
            presentation.UpdateSpeedrate(generateSizeString(speedrate) + "/s"); // 42kb +"/s"
            ReceivedDataSize = 0;
        }

        #endregion
    }
}
