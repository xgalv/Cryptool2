﻿/* 
   Copyright 2011 Corinna John

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
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    public class StegoLeastSignificantBitSettings : ISettings
    {
        #region Private Variables

        private int selectedAction = 0;
        private int outputFileFormat = 0;
        private int bitCount = 1;
        //private int noisePercent = 0;
        private bool customizeRegions = false;

        #endregion

        #region TaskPane Settings

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, null, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Action");   
                }
            }
        }

        [TaskPane("BitCountSettingsCaption", "BitCountSettingsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int BitCount
        {
            get
            {
                return bitCount;
            }
            set
            {
                if (bitCount != value)
                {
                    bitCount = value;
                    OnPropertyChanged("BitCount");
                }
            }
        }

        /* TODO: Enable this property only if Action==Encrypt
         * [TaskPane("Noise", "Percentage of the carrier that will be covered with noise", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int NoisePercent
        {
            get
            {
                return noisePercent;
            }
            set
            {
                if (noisePercent != value)
                {
                    noisePercent = value;
                    OnPropertyChanged("NoisePercent");
                }
            }
        }*/

        [TaskPane("CustomizeRegionsCaption", "CustomizeRegionsTooltip", null, 1, false, ControlType.CheckBox)]
        public bool CustomizeRegions
        {
            get
            {
                return customizeRegions;
            }
            set
            {
                if (customizeRegions != value)
                {
                    customizeRegions = value;
                    OnPropertyChanged("CustomizeRegions");
                }
            }
        }

        [ContextMenu("OutputFileFormatCaption", "OutputFileFormatTooltip", 1, ContextMenuControlType.ComboBox, null, "OutputFileFormatList1", "OutputFileFormatList2", "OutputFileFormatList3")]
        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 1, true, ControlType.ComboBox, new string[] { "OutputFileFormatList1", "OutputFileFormatList2", "OutputFileFormatList3" })]
        public int OutputFileFormat
        {
            get
            {
                return this.outputFileFormat;
            }
            set
            {
                if (value != outputFileFormat)
                {
                    this.outputFileFormat = value;
                    OnPropertyChanged("OutputFileFormat");   
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (Action)
            {
                case 0: // Encryption
                    settingChanged("OutputFileFormat", Visibility.Visible);
                    settingChanged("BitCount", Visibility.Visible);
                    break;
                case 1: // Decryption
                    settingChanged("OutputFileFormat", Visibility.Collapsed);
                    settingChanged("BitCount", Visibility.Collapsed);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
