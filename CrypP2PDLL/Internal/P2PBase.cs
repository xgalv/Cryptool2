﻿/*
   Copyright 2010 Paul Lelgemann and Christian Arnold,
                  University of Duisburg-Essen

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

using System;
using System.Linq;
using System.Text;
using System.Threading;
using Cryptool.P2P;
using Cryptool.P2P.Interfaces;
using Cryptool.P2P.Types;
using Cryptool.PluginBase;
using Gears4Net;
using PeersAtPlay;
using PeersAtPlay.Monitoring;
using PeersAtPlay.P2PLink;
using PeersAtPlay.P2PLink.SnalNG;
using PeersAtPlay.P2POverlay;
using PeersAtPlay.P2POverlay.Bootstrapper;
using PeersAtPlay.P2POverlay.Bootstrapper.DnsBootstrapper;
using PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2;
using PeersAtPlay.P2POverlay.Bootstrapper.LocalMachineBootstrapper;
using PeersAtPlay.P2POverlay.FullMeshOverlay;
using PeersAtPlay.P2PStorage.DHT;
using PeersAtPlay.P2PStorage.FullMeshDHT;
using PeersAtPlay.PapsClient;
using PeersAtPlay.Util.Logging;
using PeersAtPlay.P2PStorage.WebDHT;
using PeersAtPlay.P2POverlay.Chordi;
using System.Security.Cryptography;

/* TODO:
 * - Delete UseNatTraversal-Flag and insert CertificateCheck and CertificateSetup
 * - Testing asynchronous methods incl. EventHandlers
 */

namespace Cryptool.P2PDLL.Internal
{
    /// <summary>
    ///   Wrapper class to integrate peer@play environment into CrypTool. 
    ///   This class synchronizes asynchronous methods for easier usage in CT2.
    /// </summary>
    public class P2PBase : IP2PBase
    {
        #region Variables

        private AutoResetEvent systemJoined;
        private readonly AutoResetEvent systemLeft;
        private IBootstrapper bootstrapper;
        private IP2PLinkManager linkmanager;
        private P2POverlay overlay;
        internal IDHT Dht;
        internal IVersionedDHT VersionedDht;

        /// <summary>
        ///   True if system was successfully joined, false if system is COMPLETELY left
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///   True if the underlying peer to peer system has been fully initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region Delegates

        public event P2PMessageReceived OnP2PMessageReceived;
        public delegate void P2PMessageReceived(PeerId sourceAddr, byte[] data);

        public event Delegates.SystemJoined OnSystemJoined;
        public event Delegates.SystemLeft OnSystemLeft;

        #endregion

        public P2PBase()
        {
            IsConnected = false;
            IsInitialized = false;

            systemJoined = new AutoResetEvent(false);
            systemLeft = new AutoResetEvent(false);
        }

        public static string Password { get; set; }

        #region Basic P2P Methods (Init, Start, Stop)

        /// <summary>
        ///   Initializes the underlying peer-to-peer system with settings configured in P2PSettings. This step is required in order to be able to establish a connection.
        /// </summary>
        public void Initialize()
        {
            Scheduler scheduler = new STAScheduler("pap_snal");
            Scheduler scheduler_2 = new STAScheduler("pap_mysql");

            switch (P2P.P2PSettings.Default.LinkManager)
            {
                case P2PLinkManagerType.Snal:
                    LogToMonitor("Init LinkMgr: Using NAT Traversal stuff");

                    // NAT-Traversal stuff needs a different Snal-Version
                    linkmanager = new Snal(scheduler);

                    var settings = new PeersAtPlay.P2PLink.SnalNG.Settings();
                    settings.LoadDefaults();
                    settings.ConnectInternal = true;
                    settings.LocalReceivingPort = P2P.P2PSettings.Default.LocalReceivingPort;
                    settings.UseLocalAddressDetection = P2P.P2PSettings.Default.UseLocalAddressDetection;
                    settings.NoDelay = false;
                    settings.ReuseAddress = false;
                    settings.UseNetworkMonitorServer = true;
                    settings.CloseConnectionAfterPingTimeout = true;

                    settings.FragmentMessages = true;
                    settings.FragmentMessageSize = 10*1024;

                    linkmanager.Settings = settings;
                    linkmanager.ApplicationType = ApplicationType.CrypTool;

                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (P2P.P2PSettings.Default.Bootstrapper)
            {
                case P2PBootstrapperType.LocalMachineBootstrapper:
                    // LocalMachineBootstrapper = only local connection (runs only on one machine)
                    bootstrapper = new LocalMachineBootstrapper();
                    break;
                case P2PBootstrapperType.IrcBootstrapper:
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2.Settings.DelaySymmetricResponse = true;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2.Settings.IncludeSymmetricResponse = false;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2.Settings.UsePeerCache = false;

                    bootstrapper = new IrcBootstrapper(scheduler);
                    break;
                case P2PBootstrapperType.DnsBootstrapper:
                    bootstrapper = new DnsBootstrapper();
                    break;
                default:
                    throw new NotImplementedException();
            }

            try
            {
                switch (P2P.P2PSettings.Default.Architecture)
                {
                    case P2PArchitecture.FullMesh:
                        overlay = new FullMeshOverlay(scheduler);
                        Dht = new FullMeshDHT(scheduler);
                        break;
                    case P2PArchitecture.Chord:
                        overlay = new ChordiOverlay(scheduler);
                        Dht = new ChordiDHT(scheduler);
                        break;
                    case P2PArchitecture.Server:
                        PeersAtPlay.PapsClient.Properties.Settings.Default.ServerHost = P2P.P2PSettings.Default.ServerHost;
                        PeersAtPlay.PapsClient.Properties.Settings.Default.ServerPort = P2P.P2PSettings.Default.ServerPort;
                        bootstrapper = new LocalMachineBootstrapper();
                        overlay = new PapsClientOverlay();
                        Dht = new PapsClientDht(scheduler_2);
                        break;
                    case P2PArchitecture.WebDHT:
                        Dht = new WebDHT(scheduler_2);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch(Exception e)
            {
                P2PManager.GuiLogMessage("Error initializing P2P network: " + e.Message, NotificationLevel.Error);
                return;
            }

            if (overlay != null)
            {
                overlay.MessageReceived += OverlayMessageReceived;
            }
            Dht.SystemJoined += OnDhtSystemJoined;
            Dht.SystemLeft += OnDhtSystemLeft;

            VersionedDht = (IVersionedDHT) Dht;

            P2PManager.GuiLogMessage("Initializing DHT with world name " + P2P.P2PSettings.Default.WorldName,
                                        NotificationLevel.Info);
            IsInitialized = true;
            
            string password = null;
            if (P2P.P2PSettings.Default.RememberPassword)
            {
                password = StringHelper.DecryptString(P2P.P2PSettings.Default.Password);
            }else{
                password = StringHelper.DecryptString(P2PBase.Password);                
            }

            Dht.Initialize(P2P.P2PSettings.Default.PeerName, password, P2P.P2PSettings.Default.WorldName, overlay,
                           bootstrapper, linkmanager, null);
        }

        /// <summary>
        ///   Starts the P2P System. When the given P2P world doesn't exist yet, 
        ///   inclusive creating the and bootstrapping to the P2P network.
        ///   In either case joining the P2P world.
        ///   This synchronized method returns true not before the peer has 
        ///   successfully joined the network (this may take one or two minutes).
        /// </summary>
        /// <exception cref = "InvalidOperationException">When the peer-to-peer system has not been initialized. 
        /// After validating the settings, this can be done by calling Initialize().</exception>
        /// <returns>True, if the peer has completely joined the p2p network</returns>
        public bool SynchStart()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Peer-to-peer is not initialized.");
            }

            if (IsConnected)
            {
                return true;
            }

            try
            {
                Dht.BeginStart(BeginStartEventHandler);

                // Wait for event SystemJoined. When it's invoked, the peer completely joined the P2P system
                systemJoined.WaitOne();
                P2PManager.GuiLogMessage("System join process ended.", NotificationLevel.Debug);
            }
            catch (Exception e)
            {
                e.GetBaseException();
            }

            return true;
        }

        private void BeginStartEventHandler(DHTEventArgs eventArgs)
        {
            P2PManager.GuiLogMessage("Received DHTEventArgs: " + eventArgs + ", state: " + eventArgs.State, NotificationLevel.Debug);
        }

        /// <summary>
        ///   Disconnects from the peer-to-peer system.
        /// </summary>
        /// <returns>True, if the peer has completely left the p2p network</returns>
        public bool SynchStop()
        {
            if (Dht == null) return false;

            Dht.BeginStop(null);

            if (IsConnected)
                systemLeft.WaitOne();

            systemJoined.Reset();
            systemLeft.Reset();

            return true;
        }

        #endregion

        #region Peer related method (GetPeerId, Send message to peer)

        /// <summary>
        ///   Get PeerName of the actual peer
        /// </summary>
        /// <param name = "sPeerName">out: additional peer information UserName on LinkManager</param>
        /// <returns>PeerID as a String</returns>
        internal PeerId GetPeerId(out string sPeerName)
        {
            sPeerName = linkmanager.UserName;
            return new PeerId(overlay.LocalAddress);
        }

        /// <summary>
        ///   Construct PeerId object for a specific byte[] id
        /// </summary>
        /// <param name = "byteId">overlay address as byte array</param>
        /// <returns>corresponding PeerId for given byte[] id</returns>
        internal PeerId GetPeerId(byte[] byteId)
        {
            LogToMonitor("GetPeerID: Converting byte[] to PeerId-Object");
            return new PeerId(overlay.GetAddress(byteId));
        }

        // overlay.LocalAddress = Overlay-Peer-Address/Names
        public void SendToPeer(byte[] data, byte[] destinationPeer)
        {
            // get stack size of the pap use-data and add own use data (for optimizing Stack size)
            var realStackSize = overlay.GetHeaderSize() + data.Length;

            var stackData = new ByteStack(realStackSize);
            stackData.Push(data);

            var destinationAddr = overlay.GetAddress(destinationPeer);
            var overlayMsg = new OverlayMessage(MessageReceiverType.P2PBase, 
                                                overlay.LocalAddress, destinationAddr, stackData);

            overlay.Send(overlayMsg);
        }

        private void OverlayMessageReceived(object sender, OverlayMessageEventArgs e)
        {
            if (OnP2PMessageReceived == null) return;

            var pid = new PeerId(e.Message.Source);
            /* You have to fire this event asynchronous, because the main 
                 * thread will be stopped in this wrapper class for synchronizing
                 * the asynchronous stuff (AutoResetEvent) --> so this could run 
                 * into a deadlock, when you fire this event synchronous (normal Invoke)
                 * ATTENTION: This could change the invocation order!!! In my case 
                              no problem, but maybe in future cases... */

            // TODO: not safe: The delegate must have only one target
            // OnP2PMessageReceived.BeginInvoke(pid, e.Message.Data.PopBytes(e.Message.Data.CurrentStackSize), null, null);

            foreach (var del in OnP2PMessageReceived.GetInvocationList())
            {
                var data = e.Message.Data.ToArray();
                if (e.Message.Data.CurrentStackSize != 0)
                    data = e.Message.Data.PopBytes(e.Message.Data.CurrentStackSize);

                del.DynamicInvoke(pid, data);
            }
        }

        #endregion

        #region Event Handling (System Joined, Left and Message Received)

        private void OnDhtSystemJoined(object sender, EventArgs e)
        {
            IsConnected = true;

            if (OnSystemJoined != null)
                OnSystemJoined();

            systemJoined.Set();
        }

        private void OnDhtSystemLeft(object sender, SystemLeftEventArgs e)
        {
            IsConnected = false;
            IsInitialized = false;
            Dht.BeginStop(null);

            // Allow new connection to start and check for waiting / blocked tasks
            // TODO reset running ConnectionWorkers?
            systemLeft.Set();
            systemJoined.Set();

            LogToMonitor("CrypP2P left the system.");

            if (OnSystemLeft != null)
                OnSystemLeft();
        }

        #endregion

        #region Synchronous Methods + their Callbacks

        /// <summary>
        ///   Stores a value in the DHT at the given key
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <param name = "value">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public IRequestResult SynchStore(string key, string value)
        {
            return SynchStore(key, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        ///   Stores a value in the DHT at the given key
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <param name = "data">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public IRequestResult SynchStore(string key, byte[] data)
        {
            var autoResetEvent = new AutoResetEvent(false);

            // LogToMonitor("testcrash" + Encoding.UTF8.GetString(new byte[5000]));
            LogToMonitor("Begin: SynchStore. Key: " + key + ", " + (data != null ? data.Length : 0) + " bytes");

            var requestResult = new RequestResult {WaitHandle = autoResetEvent, Key = key, Data = data};
            VersionedDht.Store(OnSynchStoreCompleted, key, data, requestResult);

            if (P2P.P2PSettings.Default.UseTimeout)
            {
                // blocking till response
                bool success = autoResetEvent.WaitOne(1000*60*2);
                if (!success)
                {
                    P2PManager.GuiLogMessage("Timeout in DHT Operation. Just a quick hack!", NotificationLevel.Warning);
                    if (!IsConnected)
                        throw new NotConnectedException();
                    else
                        throw new P2POperationFailedException("SynchStore failed for some reason!");
                }
            }
            else
            {
                autoResetEvent.WaitOne();
            }

            LogToMonitor("End: SynchStore. Key: " + key + ". Status: " + requestResult.Status);

            return requestResult;
        }

        /// <summary>
        ///   Callback for a the synchronized store method
        /// </summary>
        /// <param name = "storeResult">retrieved data container</param>
        private static void OnSynchStoreCompleted(StoreResult storeResult)
        {
            var requestResult = storeResult.AsyncState as RequestResult;
            if (requestResult == null)
            {
                LogToMonitor("Received OnSynchStoreCompleted, but RequestResult object is missing. Discarding.");
                return;
            }

            requestResult.Parse(storeResult);

            // unblock WaitHandle in the synchronous method
            requestResult.WaitHandle.Set();
        }

        /// <summary>
        ///   Get the value of the given DHT Key or null, if it doesn't exist.
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <returns>Value of DHT Entry</returns>
        public IRequestResult SynchRetrieve(string key)
        {
            LogToMonitor("Begin: SynchRetrieve. Key: " + key);

            var autoResetEvent = new AutoResetEvent(false);
            var requestResult = new RequestResult {WaitHandle = autoResetEvent, Key = key};

            Dht.Retrieve(OnSynchRetrieveCompleted, key, requestResult);

            if (P2P.P2PSettings.Default.UseTimeout)
            {
                // blocking till response
                bool success = autoResetEvent.WaitOne(1000*60*2);
                if (!success)
                {
                    P2PManager.GuiLogMessage("Timeout in DHT Operation. Just a quick hack!", NotificationLevel.Warning);
                    if (!IsConnected)
                        throw new NotConnectedException();
                    else
                        throw new P2POperationFailedException("SynchRetrieve failed for some reason!");
                }
            }
            else
            {
                autoResetEvent.WaitOne();
            }

            LogToMonitor("End: SynchRetrieve. Key: " + key + ". Status: " + requestResult.Status);

            return requestResult;
        }

        /// <summary>
        ///   Callback for a the synchronized retrieval method
        /// </summary>
        /// <param name = "retrieveResult"></param>
        private static void OnSynchRetrieveCompleted(RetrieveResult retrieveResult)
        {
            var requestResult = retrieveResult.AsyncState as RequestResult;
            if (requestResult == null)
            {
                LogToMonitor("Received OnSynchRetrieveCompleted, but RequestResult object is missing. Discarding.");
                return;
            }

            requestResult.Parse(retrieveResult);

            // unblock WaitHandle in the synchronous method
            requestResult.WaitHandle.Set();
        }

        /// <summary>
        ///   Removes a key/value pair out of the DHT
        /// </summary>
        /// <param name = "key">Key of the DHT Entry</param>
        /// <returns>True, when removing is completed!</returns>
        public IRequestResult SynchRemove(string key)
        {
            LogToMonitor("Begin SynchRemove. Key: " + key);

            var autoResetEvent = new AutoResetEvent(false);
            var requestResult = new RequestResult { WaitHandle = autoResetEvent, Key = key };
            VersionedDht.Remove(OnSynchRemoveCompleted, key, requestResult);

            if (P2P.P2PSettings.Default.UseTimeout)
            {
                // blocking till response
                bool success = autoResetEvent.WaitOne(1000*60*2);
                if (!success)
                {
                    P2PManager.GuiLogMessage("Timeout in DHT Operation. Just a quick hack!", NotificationLevel.Warning);
                    if (!IsConnected)
                        throw new NotConnectedException();
                    else
                        throw new P2POperationFailedException("SynchRemove failed for some reason!");
                }
            }
            else
            {
                autoResetEvent.WaitOne();
            }

            LogToMonitor("End: SynchRemove. Key: " + key + ". Status: " + requestResult.Status);

            return requestResult;
        }

        /// <summary>
        ///   Callback for a the synchronized remove method
        /// </summary>
        /// <param name = "removeResult"></param>
        private static void OnSynchRemoveCompleted(RemoveResult removeResult)
        {
            var requestResult = removeResult.AsyncState as RequestResult;
            if (requestResult == null)
            {
                LogToMonitor("Received OnSynchRemoveCompleted, but RequestResult object is missing. Discarding.");
                return;
            }

            requestResult.Parse(removeResult);

            // unblock WaitHandle in the synchronous method
            requestResult.WaitHandle.Set();
        }

        #endregion

        #region Statistic Methods

        public long TotalBytesSentOnAllLinks()
        {
            if (linkmanager != null)
                return (long) linkmanager.GetAllLinkInformation().Sum(linkInformation => linkInformation.TotalBytesSent);
            return 0;
        }

        public long TotalBytesReceivedOnAllLinks()
        {
            if (linkmanager != null)
                return (long) linkmanager.GetAllLinkInformation().Sum(linkInformation => linkInformation.TotalBytesReceived);
            return 0;
        }

        #endregion

        #region Log facility

        /// <summary>
        ///   To log the internal state in the Monitoring Software of P@play
        /// </summary>
        public void LogInternalState()
        {
            if (Dht != null)
            {
                Dht.LogInternalState();
            }
        }

        private static void LogToMonitor(string sTextToLog)
        {
            if (P2P.P2PSettings.Default.Log2Monitor)
                Log.Debug(sTextToLog);
        }

        #endregion
    }
}