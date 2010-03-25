/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Documents;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;

namespace RIPEMD160
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "RIPEMD160", "RIPEMD160 hash function", "", "RIPEMD160/RMD160.png")]    
    public class RIPEMD160 : ICryptographicHash
    {
        #region Private variables
        private RIPEMD160Settings settings;
        private CryptoolStream inputData;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private byte[] outputData;
        #endregion

        #region Public interface
        public RIPEMD160()
        {
            this.settings = new RIPEMD160Settings();
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RIPEMD160Settings)value; }
        }

        // [QuickWatch(QuickWatchFormat.Hex, DisplayLevel.Beginner, null)]
        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be hashed", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputData
        {
            get 
            {
              if (this.inputData != null)
              {
                CryptoolStream cs = new CryptoolStream();
                listCryptoolStreamsOut.Add(cs);
                cs.OpenRead(inputData.FileName);
                return cs;
              }
              return null;
            }
            set 
            {
              if (value != inputData)
              {
                this.inputData = value;
                OnPropertyChanged("InputData");
              }
            }
        }

        // [QuickWatch(QuickWatchFormat.Hex, DisplayLevel.Beginner, null)]
        [PropertyInfo(Direction.OutputData, "Hashed value", "Output data of the hashed value as Stream", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputDataStream
        {
          get
          {
              CryptoolStream outputDataStream = null;
              if (OutputData != null)
              {
                outputDataStream = new CryptoolStream();
                outputDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, OutputData);
                listCryptoolStreamsOut.Add(outputDataStream);
              }
              return outputDataStream;
          }
          set { } // readonly
        }

        // [QuickWatch(QuickWatchFormat.Hex, DisplayLevel.Beginner, null)]
        [PropertyInfo(Direction.OutputData, "Hashed value", "Output data of the hashed value as byte array", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] OutputData
        {
          get { return this.outputData; }
          set
          {
            if (value != outputData)
            {
              outputData = value;
              OnPropertyChanged("OutputData");
              OnPropertyChanged("OutputDataStream");              
            }
          }
        }

        public void Execute()
        {
          Progress(0.5, 1.0);
          if (inputData != null && inputData.Length > 0)
          {
            System.Security.Cryptography.RIPEMD160 ripeMd160Hash = System.Security.Cryptography.RIPEMD160.Create();
            byte[] data = ripeMd160Hash.ComputeHash((Stream)inputData);

            
            GuiLogMessage("Hash created.", NotificationLevel.Info);            
            Progress(1, 1);

            OutputData = data;
          }
          else
          {            
            if (inputData == null)
              GuiLogMessage("Received null value for CryptoolStream.", NotificationLevel.Warning);
            else
              GuiLogMessage("Stream length is 0.", NotificationLevel.Warning);
          }
          Progress(1.0, 1.0);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
          if (inputData != null)
          {
            inputData.Close();
            inputData = null;
          }
          foreach (CryptoolStream cryptoolStream in listCryptoolStreamsOut)
          {
            cryptoolStream.Close();
          }
          listCryptoolStreamsOut.Clear();
        }
        #endregion

        #region IPlugin Members       

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public UserControl Presentation
        {
          get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
          get { return null; }
        }

        public void Stop()
        {

        }

        public void PostExecution()
        {
          Dispose();
        }

        public void PreExecution()
        {
          
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IHashAlgorithm Members

        public FlowDocument DetailedDescription
        {
            get { return null; }
        }

        #endregion

        private void Progress(double value, double max)
        {
          EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Pause()
        {
          
        }

        #endregion
    }
}
