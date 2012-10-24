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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Validation;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Achterbahn
{
    public class AchterbahnSettings : ISettings
    {
        #region Private Variables

        private int mode = 0; // 0 = Achterbahn-80, 1 = Achterbahn-128 

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(1)]
        [TaskPane("ModeCaption", "ModeTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2" })]
        public int Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (value != mode)
                {
                    this.mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}