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
using System;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.FileInputWPF
{
    // HOWTO: rename class (click name, press F2)
    public class ExamplePluginCT2Settings : ISettings
    {

        private string saveAndRestoreState;
        public string SaveAndRestoreState
        {
            get { return saveAndRestoreState; }
            set
            {
                if (value != saveAndRestoreState) hasChanges = true;
                saveAndRestoreState = value;
                OnPropertyChanged("SaveAndRestoreState");
            }
        }

        private bool hasChanges;

        private string openFilename;
        [TaskPane("OpenFilenameCaption", "OpenFilenameTooltip", null, 1, false, ControlType.OpenFileDialog, FileExtension = "All Files (*.*)|*.*")]
        public string OpenFilename
        {
            get { return openFilename; }
            set
            {
                if (value != openFilename)
                {
                    openFilename = value;
                    OnPropertyChanged("OpenFilename");
                }
            }
        }

        [TaskPane("CloseFileCaption", "CloseFileTooltip", null, 2, false, ControlType.Button)]
        public void CloseFile()
        {
            OpenFilename = null;
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
