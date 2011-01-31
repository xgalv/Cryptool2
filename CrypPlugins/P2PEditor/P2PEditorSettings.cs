﻿/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

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

using System.ComponentModel;
using System.Windows;
using Cryptool.P2P;
using Cryptool.P2PEditor.GUI;
using Cryptool.PluginBase;
using Cryptool.Plugins.PeerToPeer.Internal;
using System.Text;
using System.Security.Cryptography;
using System;
using Cryptool.P2P.Internal;

namespace Cryptool.P2PEditor
{
    internal class P2PEditorSettings : ISettings
    {
        private readonly P2PEditor p2PEditor;
        private readonly P2PSettings settings;

        private const string GroupExperienced = "experienced_settings";
        private const string GroupExpert = "expert_settings";
        private const string GroupServer = "server_settings";

        public P2PEditorSettings(P2PEditor p2PEditor)
        {
            this.p2PEditor = p2PEditor;
            settings = P2PSettings.Default;
            try
            {
                if (settings.UpdateFlag)
                {
                    settings.Upgrade();
                    settings.UpdateFlag = false;
                    this.p2PEditor.GuiLogMessage("Upgrading settings", NotificationLevel.Debug);
                }
            }catch
            {
            
            }

            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);

            UpdateSettings();
        }


        #region Update visibility of server group

        public void UpdateSettings()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (P2PSettings.Default.Architecture == P2PArchitecture.Server)
                UpdateServerOptionVisibilitySetting(Visibility.Visible);
            else
                UpdateServerOptionVisibilitySetting(Visibility.Hidden);
        }

        private void UpdateServerOptionVisibilitySetting(Visibility newVisibility)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ServerHost", newVisibility)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ServerPort", newVisibility)));
        }

        #endregion

        #region Events
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        #endregion

        #region ISettings Members

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasChanges { get; set; }

        #endregion

        #region Settings

        /*[TaskPane("username_caption",
            "username_tooltip"
            , null, 0, false, ControlType.TextBox)]*/
        public string PeerName
        {
            get { return settings.PeerName; }
            set
            {
                if (value != settings.PeerName)
                {
                    settings.PeerName = value;
                    OnPropertyChanged("PeerName");
                    HasChanges = true;
                }
            }
        }

        /*[TaskPane("password_caption",
            "password_tooltip"
            , null, 1, false, ControlType.TextBoxHidden)]*/
        public string Password
        {
            get {
                if (RememberPassword)
                {
                    return P2PBase.DecryptString(settings.Password); 
                }
                else
                {
                    return P2PBase.DecryptString(P2PBase.Password); 
                }
                }
            set
            {
                if (RememberPassword)
                {
                    if (P2PBase.EncryptString(value) != settings.Password)
                    {
                        settings.Password = P2PBase.EncryptString(value);
                        OnPropertyChanged("Password");
                        HasChanges = true;
                    }
                }
                else
                {
                    if (P2PBase.EncryptString(value) != P2PBase.Password)
                    {
                        P2PBase.Password = P2PBase.EncryptString(value);
                        OnPropertyChanged("Password");
                        HasChanges = true;
                    }                    
                }
            }
        }

        /*[TaskPane("rememberPassword_caption", 
            "rememberPassword_tooltip"
            , null, 1, false, ControlType.CheckBox)]*/
        public bool RememberPassword
        {
            get { return settings.RememberPassword; }
            set
            {
                if (value != settings.RememberPassword)
                {
                    settings.RememberPassword = value;
                    if (value)
                    {
                        settings.Password = P2PBase.Password;
                        P2PBase.Password = "";
                    }
                    else
                    {
                        P2PBase.Password = settings.Password;
                        settings.Password = "";
                    }
                    HasChanges = true;
                    OnPropertyChanged("RememberPassword");
                }
            }
        }

        [TaskPane("worldname_caption", "worldname_tooltip", null, 2, false, ControlType.TextBox)]
        public string WorldName
        {
            get { return settings.WorldName; }
            set
            {
                if (value != settings.WorldName)
                {
                    settings.WorldName = value;
                    OnPropertyChanged("WorldName");
                    HasChanges = true;
                }
            }
        }

       
        [TaskPane("start_caption", "start_tooltip", null, 3, true, ControlType.Button)]
        public void ButtonStart()
        {
            if (!P2PManager.IsConnected)
            {
                P2PManager.Connect();
                OnPropertyChanged("ButtonStart");
                p2PEditor.GuiLogMessage(Resources.Attributes.start_launched, NotificationLevel.Info);
                ((P2PEditorPresentation)p2PEditor.Presentation).UpdateConnectionState();
            } else
            {
                p2PEditor.GuiLogMessage(Resources.Attributes.start_failed, NotificationLevel.Warning);
            }
        }

        [TaskPane("stop_caption", "stop_tooltip", null, 4, true, ControlType.Button)]
        public void ButtonStop()
        {
            if (P2PManager.IsConnected)
            {
                P2PManager.Disconnect();
                OnPropertyChanged("ButtonStop");
                p2PEditor.GuiLogMessage(Resources.Attributes.stop_launched, NotificationLevel.Info);
            }
            else
            {
                p2PEditor.GuiLogMessage(Resources.Attributes.stop_failed, NotificationLevel.Warning);
            }
        }



        [TaskPane("networksize_caption", "networksize_tooltip", null, 6, false, ControlType.TextBoxReadOnly)]
        public string NetSize
        {
            get { return settings.NetSize; }
            set
            {
                if (value != settings.NetSize)
                {
                    settings.NetSize = value;
                    OnPropertyChanged("NetSize");   
                }
            }
        }


        // TODO New ControlType needed to choose dialogs? OpenFileDialog not fitting.
        [TaskPane("workspacePath_caption", "workspacePath_tooltip", GroupExperienced, 0, true, ControlType.TextBox)]
        public string WorkspacePath
        {
            get { return settings.WorkspacePath; }
            set
            {
                if (value != settings.WorkspacePath)
                {
                    settings.WorkspacePath = value;
                    OnPropertyChanged("WorkspacePath");
                    HasChanges = true;
                }
            }
        }


        [TaskPane("distributedJobListRefreshInterval_caption", "distributedJobListRefreshInterval_tooltip", GroupExperienced, 1, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int DistributedJobListRefreshInterval
        {
            get { return settings.DistributedJobListRefreshInterval; }
            set
            {
                if (value != settings.DistributedJobListRefreshInterval)
                {
                    settings.DistributedJobListRefreshInterval = value;
                    OnPropertyChanged("DistributedJobListRefreshInterval");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("connectOnStartup_caption", "connectOnStartup_tooltip", GroupExperienced, 2, true,
            ControlType.CheckBox)]
        public bool ConnectOnStartup
        {
            get { return settings.ConnectOnStartup; }
            set
            {
                if (value != settings.ConnectOnStartup)
                {
                    settings.ConnectOnStartup = value;
                    OnPropertyChanged("ConnectOnStartup");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("linkmanager_caption", "linkmanager_tooltip", GroupExpert, 3, false,
            ControlType.ComboBox, new[] {"Snal"})]
        public int LinkManager
        {
            get { return (int) settings.LinkManager; }
            set
            {
                if ((P2PLinkManagerType) value != settings.LinkManager)
                {
                    settings.LinkManager = (P2PLinkManagerType) value;
                    OnPropertyChanged("LinkManager");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("bootstrapper_caption", "bootstrapper_tooltip", GroupExpert, 4, false
            , ControlType.ComboBox, new[] {"LocalMachineBootstrapper", "IrcBootstrapper", "DnsBootstrapper"})]
        public int Bootstrapper
        {
            get { return (int) settings.Bootstrapper; }
            set
            {
                if ((P2PBootstrapperType) value != settings.Bootstrapper)
                {
                    settings.Bootstrapper = (P2PBootstrapperType) value;
                    OnPropertyChanged("Bootstrapper");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("architecture_caption", "architecture_tooltip", GroupExpert, 5, false,
            ControlType.ComboBox, new[] { "FullMesh", "Chord", "Server" , "WebDHT" })]
        public int Architecture
        {
            get { return (int)settings.Architecture; }
            set
            {
                if ((P2PArchitecture)value != settings.Architecture)
                {
                    settings.Architecture = (P2PArchitecture)value;
                    UpdateSettings();
                    OnPropertyChanged("Architecture");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("localPort_caption", "localPort_tooltip", GroupExpert, 6, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 65535)]
        public int LocalPort
        {
            get { return settings.LocalReceivingPort; }
            set
            {
                if (value != settings.LocalReceivingPort)
                {
                    settings.LocalReceivingPort = value;
                    OnPropertyChanged("LocalPort");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("useLocalAddressDetection_caption", "useLocalAddressDetection_tooltip", GroupExpert, 7, false,
            ControlType.CheckBox)]
        public bool UseLocalAddressDetection
        {
            get { return settings.UseLocalAddressDetection; }
            set
            {
                if (value != settings.UseLocalAddressDetection)
                {
                    settings.UseLocalAddressDetection = value;
                    OnPropertyChanged("UseLocalAddressDetection");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("log2monitor_caption", "log2monitor_tooltip", GroupExpert, 8, false,
            ControlType.CheckBox)]
        public bool Log2Monitor
        {
            get { return settings.Log2Monitor; }
            set
            {
                if (value != settings.Log2Monitor)
                {
                    settings.Log2Monitor = value;
                    OnPropertyChanged("Log2Monitor");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("serverHost_caption", "serverHost_tooltip", GroupServer, 9, false, ControlType.TextBox)]
        public string ServerHost
        {
            get { return settings.ServerHost; }
            set
            {
                if (value != settings.ServerHost)
                {
                    settings.ServerHost = value;
                    OnPropertyChanged("ServerHost");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("serverPort_caption", "serverPort_tooltip", GroupServer, 10, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 65535)]
        public int ServerPort
        {
            get { return settings.ServerPort; }
            set
            {
                if (value != settings.ServerPort)
                {
                    settings.ServerPort = value;
                    OnPropertyChanged("ServerPort");
                    HasChanges = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// This is needed, if some settings are changed via "P2PSettings.Default.xyz". In some cases (e.g. controltype: TextBoxreadonly)
        /// the GUI will not be updated automatically without this additionaly firing of the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            //this.p2PEditor.GuiLogMessage("Property changed: " + e.PropertyName, NotificationLevel.Debug);
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }

            P2PSettings.Default.Save();
        }

    }
}